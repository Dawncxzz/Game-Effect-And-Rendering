Shader "Unlit/Glitch"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "Glitch"

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag

            #pragma multi_compile _ _RGBSPLITGLITCH _IMAGEBLOCKGLITCH _LINEBLOCKGLITCH _TILEJITTERGLITCH _SCANLINEJITTERGLITCH _DIGITALSTRIPEGLITCH _ANALOGNOISEGLITCH _SCREENJUMPGLITCH _SCREENSHAKEGLITCH _WAVEJITTERGLITCH

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            
            TEXTURE2D_X(_SourceTex);
            float4 _SourceTex_TexelSize;
            TEXTURE2D_X(_SourceTexLowMip);
            float4 _SourceTexLowMip_TexelSize;

            #if _RGBSPLITGLITCH
                TEXTURE2D_X(_RGBSPLITGLITCH_NoiseTex);
                SAMPLER(sampler_RGBSPLITGLITCH_NoiseTex);
                float _RGBSPLITGLITCH_Speed;
                float _RGBSPLITGLITCH_Amplitude;
                inline float4 Pow4(float4 v, float p)
	            {
		            return float4(pow(v.x, p), pow(v.y, p), pow(v.z, p), v.w);
	            }

	            inline float4 Noise(float2 p)
	            {
		            return SAMPLE_TEXTURE2D(_RGBSPLITGLITCH_NoiseTex, sampler_RGBSPLITGLITCH_NoiseTex, p);
	            }
            #endif

            #if _IMAGEBLOCKGLITCH
                float _IMAGEBLOCKGLITCH_BlockSize;
                float4 _IMAGEBLOCKGLITCH_MaxRGBSplit;
                float _IMAGEBLOCKGLITCH_Speed;
                #define _IMAGEBLOCKGLITCH_MaxRGBSplitX _IMAGEBLOCKGLITCH_MaxRGBSplit.x
                #define _IMAGEBLOCKGLITCH_MaxRGBSplitY _IMAGEBLOCKGLITCH_MaxRGBSplit.y
                inline float randomNoise(float2 seed)
	            {
		            return frac(sin(dot(seed * floor(_Time.y * _IMAGEBLOCKGLITCH_Speed), float2(17.13, 3.71))) * 43758.5453123);
	            }

	            inline float randomNoise(float seed)
	            {
		            return randomNoise(float2(seed, 1.0));
	            }
            #endif

            #if _LINEBLOCKGLITCH
				float _LINEBLOCKGLITCH_Frequency;
				float _LINEBLOCKGLITCH_TimeX;
				float _LINEBLOCKGLITCH_Amount;
				float _LINEBLOCKGLITCH_Offset;
				float _LINEBLOCKGLITCH_LinesWidth;
				float _LINEBLOCKGLITCH_Alpha;
				float randomNoise(float2 c)
				{
					return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
				}
	
				float trunc(float x, float num_levels)
				{
					return floor(x * num_levels) / num_levels;
				}
	
				float2 trunc(float2 x, float2 num_levels)
				{
					return floor(x * num_levels) / num_levels;
				}
	
				float3 rgb2yuv(float3 rgb)
				{
					float3 yuv;
					yuv.x = dot(rgb, float3(0.299, 0.587, 0.114));
					yuv.y = dot(rgb, float3(-0.14713, -0.28886, 0.436));
					yuv.z = dot(rgb, float3(0.615, -0.51499, -0.10001));
					return yuv;
				}
	
				float3 yuv2rgb(float3 yuv)
				{
					float3 rgb;
					rgb.r = yuv.x + yuv.z * 1.13983;
					rgb.g = yuv.x + dot(float2(-0.39465, -0.58060), yuv.yz);
					rgb.b = yuv.x + yuv.y * 2.03211;
					return rgb;
				}

                float4 Horizontal(Varyings i)
				{
					float2 uv = i.uv;
		
					half strength = 0;
					#if USING_FREQUENCY_INFINITE
						strength = 10;
					#else
						strength = 0.5 + 0.5 * cos(_LINEBLOCKGLITCH_TimeX * _LINEBLOCKGLITCH_Frequency);
					#endif
		
					_LINEBLOCKGLITCH_TimeX *= strength;
		
					//	[1] 生成随机强度梯度线条
					float truncTime = trunc(_LINEBLOCKGLITCH_TimeX, 4.0);
					float uv_trunc = randomNoise(trunc(uv.yy, float2(8, 8)) + 100.0 * truncTime);
					float uv_randomTrunc = 6.0 * trunc(_LINEBLOCKGLITCH_TimeX, 24.0 * uv_trunc);
		
					// [2] 生成随机非均匀宽度线条
					float blockLine_random = 0.5 * randomNoise(trunc(uv.yy + uv_randomTrunc, float2(8 * _LINEBLOCKGLITCH_LinesWidth, 8 * _LINEBLOCKGLITCH_LinesWidth)));
					blockLine_random += 0.5 * randomNoise(trunc(uv.yy + uv_randomTrunc, float2(7, 7)));
					blockLine_random = blockLine_random * 2.0 - 1.0;
					blockLine_random = sign(blockLine_random) * saturate((abs(blockLine_random) - _LINEBLOCKGLITCH_Amount) / (0.4));
					blockLine_random = lerp(0, blockLine_random, _LINEBLOCKGLITCH_Offset);
		
		
					// [3] 生成源色调的blockLine Glitch
					float2 uv_blockLine = uv;
					uv_blockLine = saturate(uv_blockLine + float2(0.1 * blockLine_random, 0));
					float4 blockLineColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, abs(uv_blockLine));
		
					// [4] 将RGB转到YUV空间，并做色调偏移
					// RGB -> YUV
					float3 blockLineColor_yuv = rgb2yuv(blockLineColor.rgb);
					// adjust Chrominance | 色度
					blockLineColor_yuv.y /= 1.0 - 3.0 * abs(blockLine_random) * saturate(0.5 - blockLine_random);
					// adjust Chroma | 浓度
					blockLineColor_yuv.z += 0.125 * blockLine_random * saturate(blockLine_random - 0.5);
					float3 blockLineColor_rgb = yuv2rgb(blockLineColor_yuv);
		
		
					// [5] 与源场景图进行混合
					float4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, i.uv);
					return lerp(sceneColor, float4(blockLineColor_rgb, blockLineColor.a), _LINEBLOCKGLITCH_Alpha);
				}
				float4 Vertical(Varyings i)
				{
					float2 uv = i.uv;
		
					half strength = 0;
					#if USING_FREQUENCY_INFINITE
						strength = 10;
					#else
						strength = 0.5 + 0.5 * cos(_LINEBLOCKGLITCH_TimeX * _LINEBLOCKGLITCH_Frequency);
					#endif
		
					_LINEBLOCKGLITCH_TimeX *= strength;
		
					// [1] 生成随机均匀宽度线条
					float truncTime = trunc(_LINEBLOCKGLITCH_TimeX, 4.0);
					float uv_trunc = randomNoise(trunc(uv.xx, float2(8, 8)) + 100.0 * truncTime);
					float uv_randomTrunc = 6.0 * trunc(_LINEBLOCKGLITCH_TimeX, 24.0 * uv_trunc);
		
					// [2] 生成随机非均匀宽度线条 | Generate Random inhomogeneous Block Line
					float blockLine_random = 0.5 * randomNoise(trunc(uv.xx + uv_randomTrunc, float2(8 * _LINEBLOCKGLITCH_LinesWidth, 8 * _LINEBLOCKGLITCH_LinesWidth)));
					blockLine_random += 0.5 * randomNoise(trunc(uv.xx + uv_randomTrunc, float2(7, 7)));
					blockLine_random = blockLine_random * 2.0 - 1.0;
					blockLine_random = sign(blockLine_random) * saturate((abs(blockLine_random) - _LINEBLOCKGLITCH_Amount) / (0.4));
					blockLine_random = lerp(0, blockLine_random, _LINEBLOCKGLITCH_Offset);
		
					// [3] 生成源色调的blockLine Glitch
					float2 uv_blockLine = uv;
					uv_blockLine = saturate(uv_blockLine + float2(0, 0.1 * blockLine_random));
					float4 blockLineColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, abs(uv_blockLine));
		
					// [4] 将RGB转到YUV空间，并做色调偏移
					// RGB -> YUV
					float3 blockLineColor_yuv = rgb2yuv(blockLineColor.rgb);
					// adjust Chrominance | 色度
					blockLineColor_yuv.y /= 1.0 - 3.0 * abs(blockLine_random) * saturate(0.5 - blockLine_random);
					// adjust Chroma | 浓度
					blockLineColor_yuv.z += 0.125 * blockLine_random * saturate(blockLine_random - 0.5);
					float3 blockLineColor_rgb = yuv2rgb(blockLineColor_yuv);
		
					// [5] 与源场景图进行混合
					float4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, i.uv);
					return lerp(sceneColor, float4(blockLineColor_rgb, blockLineColor.a), _LINEBLOCKGLITCH_Alpha);
				}
            #endif

			#if _TILEJITTERGLITCH
				float _TILEJITTERGLITCH_SplittingNumber;
				float _TILEJITTERGLITCH_JitterAmount;
				float _TILEJITTERGLITCH_JitterSpeed;
				float _TILEJITTERGLITCH_Frequency;
				float randomNoise(float2 c)
				{
					return frac(sin(dot(c.xy, float2(12.9898, 78.233))) * 43758.5453);
				}

				float4 Vertical(Varyings i)
				{
					float2 uv = i.uv.xy;
					half strength = 1.0;
					half pixelSizeX = 1.0 / _ScreenParams.x;
		
					// --------------------------------Prepare Jitter UV--------------------------------
					#if USING_FREQUENCY_INFINITE
						strength = 1;
					#else
						strength = 0.5 + 0.5 *cos(_Time.y * _TILEJITTERGLITCH_Frequency);
					#endif

					if (fmod(uv.x * _TILEJITTERGLITCH_SplittingNumber, 2) < 1.0)
					{
						#if JITTER_DIRECTION_HORIZONTAL
							uv.x += pixelSizeX * cos(_Time.y * _TILEJITTERGLITCH_JitterSpeed) * _TILEJITTERGLITCH_JitterAmount * strength;
						#else
							uv.y += pixelSizeX * cos(_Time.y * _TILEJITTERGLITCH_JitterSpeed) * _TILEJITTERGLITCH_JitterAmount * strength;
						#endif
					}

					// -------------------------------Final Sample------------------------------
					half4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, uv);
					return sceneColor;
				}
	
				float4 Horizontal(Varyings i)
				{
					float2 uv = i.uv.xy;
					half strength = 1.0;
					half pixelSizeX = 1.0 / _ScreenParams.x;

					// --------------------------------Prepare Jitter UV--------------------------------
					#if USING_FREQUENCY_INFINITE
						strength = 1;
					#else
						strength = 0.5 + 0.5 * cos(_Time.y * _TILEJITTERGLITCH_Frequency);
					#endif
					if(fmod(uv.y * _TILEJITTERGLITCH_SplittingNumber, 2) < 1.0)
					{
						#if JITTER_DIRECTION_HORIZONTAL
							uv.x += pixelSizeX * cos(_Time.y * _TILEJITTERGLITCH_JitterSpeed) * _TILEJITTERGLITCH_JitterAmount * strength;
						#else
							uv.y += pixelSizeX * cos(_Time.y * _TILEJITTERGLITCH_JitterSpeed) * _TILEJITTERGLITCH_JitterAmount * strength;
						#endif
					}

					// -------------------------------Final Sample------------------------------
					half4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, uv);
					return sceneColor;
				}
			#endif

			#if _SCANLINEJITTERGLITCH
				float _SCANLINEJITTERGLITCH_Amount;
				float _SCANLINEJITTERGLITCH_Threshold;
				float _SCANLINEJITTERGLITCH_Frequency;
				float randomNoise(float x, float y)
				{
					return frac(sin(dot(float2(x, y), float2(12.9898, 78.233))) * 43758.5453);
				}
				half4 Horizontal(Varyings i)
				{
					half strength = 0;
					#if USING_FREQUENCY_INFINITE
						strength = 1;
					#else
						strength = 0.5 + 0.5 * cos(_Time.y * _SCANLINEJITTERGLITCH_Frequency);
					#endif
		
		
					float jitter = randomNoise(i.uv.y, _Time.x) * 2 - 1;
					jitter *= step(_SCANLINEJITTERGLITCH_Threshold, abs(jitter)) * _SCANLINEJITTERGLITCH_Amount * strength;
		
					half4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, frac(i.uv + float2(jitter, 0)));
		
					return sceneColor;
				}
	
				half4 Vertical(Varyings i)
				{
					half strength = 0;
					#if USING_FREQUENCY_INFINITE
						strength = 1;
					#else
						strength = 0.5 + 0.5 * cos(_Time.y * _SCANLINEJITTERGLITCH_Frequency);
					#endif
		
					float jitter = randomNoise(i.uv.x, _Time.x) * 2 - 1;
					jitter *= step(_SCANLINEJITTERGLITCH_Threshold, abs(jitter)) * _SCANLINEJITTERGLITCH_Amount * strength;
		
					half4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, frac(i.uv + float2(0, jitter)));
		
					return sceneColor;
				}
			#endif

			#if _DIGITALSTRIPEGLITCH
				TEXTURE2D(_DIGITALSTRIPEGLITCH_NoiseTex);
				SAMPLER(sampler_DIGITALSTRIPEGLITCH_NoiseTex);
				half _DIGITALSTRIPEGLITCH_Indensity;
				half4 _DIGITALSTRIPEGLITCH_StripColorAdjustColor;
				half _DIGITALSTRIPEGLITCH_StripColorAdjustIndensity;
	
				half4 _DIGITALSTRIPEGLITCH_Frag(Varyings i)
				{
					// 基础数据准备
					 half4 stripNoise = SAMPLE_TEXTURE2D(_DIGITALSTRIPEGLITCH_NoiseTex, sampler_DIGITALSTRIPEGLITCH_NoiseTex, i.uv);
					 half threshold = 1.001 - _DIGITALSTRIPEGLITCH_Indensity * 1.001;

					// uv偏移
					half uvShift = step(threshold, pow(abs(stripNoise.x), 3));
					float2 uv = frac(i.uv + stripNoise.yz * uvShift);
					half4 source = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, uv);

					#ifndef NEED_TRASH_FRAME
							return source;
					#endif 	

					// 基于废弃帧插值
					half stripIndensity = step(threshold, pow(abs(stripNoise.w), 3)) * _DIGITALSTRIPEGLITCH_StripColorAdjustIndensity;
					half3 color = lerp(source, _DIGITALSTRIPEGLITCH_StripColorAdjustColor, stripIndensity).rgb;
					return float4(color, source.a);
				}
			#endif

			#if _SCREENJUMPGLITCH
				float _SCREENJUMPGLITCH_JumpIndensity;
				float _SCREENJUMPGLITCH_JumpTime;
				#define _SCREENJUMPGLITCH_JumpTime frac(_Time.x)*_SCREENJUMPGLITCH_JumpIndensity*9.8
				half4 Vertical(Varyings i)
				{
					float jump = lerp(i.uv.y, frac(i.uv.y + _SCREENJUMPGLITCH_JumpTime), _SCREENJUMPGLITCH_JumpIndensity);        
					half4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, frac(float2(i.uv.x, jump)));
					return sceneColor;
				}
				half4 Horizontal(Varyings i)
				{
					float jump = lerp(i.uv.y, frac(i.uv.y + _SCREENJUMPGLITCH_JumpTime), _SCREENJUMPGLITCH_JumpIndensity);        
					half4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, frac(float2(jump, i.uv.y)));
					return sceneColor;
				}
						
			#endif

			#if _SCREENSHAKEGLITCH
				float _SCREENSHAKEGLITCH_ScreenShake;
				float randomNoise(float x, float y)
				{
					return frac(sin(dot(float2(x, y), float2(127.1, 311.7))) * 43758.5453);
				}
				half4 Horizontal(Varyings i)
				{
					float shake = (randomNoise(_Time.x, 2) - 0.5) * _SCREENSHAKEGLITCH_ScreenShake;
					half4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, frac(float2(i.uv.x + shake, i.uv.y)));
					return sceneColor;
				}
				half4 Vertical(Varyings i)
				{
					float shake = (randomNoise(_Time.x, 2) - 0.5) * _SCREENSHAKEGLITCH_ScreenShake;
					half4 sceneColor = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, frac(float2(i.uv.x, i.uv.y + shake)));
					return sceneColor;
				}
			#endif

			struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            Varyings FullscreenVert (appdata v)
            {
                Varyings o;
                o.positionCS = TransformObjectToHClip(v.vertex);
				//uv直接传过去即可
                o.uv=v.uv;
                
                return o;
            }

            half4 frag (Varyings input) : SV_Target
            {
                half4 col = half4(0, 0, 0, 1);
                #if _RGBSPLITGLITCH
                    float4 splitAmount = Pow4(Noise(float2(_RGBSPLITGLITCH_Speed * frac(_Time.y), 2.0 * _RGBSPLITGLITCH_Speed * frac(_Time.y) / 25.0)), 8.0) * float4(_RGBSPLITGLITCH_Amplitude, _RGBSPLITGLITCH_Amplitude, _RGBSPLITGLITCH_Amplitude, 1.0);
		            splitAmount *= 2.0 * splitAmount.w - 1.0;
		            half colorR = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, (input.uv.xy + float2(splitAmount.x, -splitAmount.y))).r;
		            half colorG = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, (input.uv.xy + float2(splitAmount.y, -splitAmount.z))).g;
		            half colorB = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, (input.uv.xy + float2(splitAmount.z, -splitAmount.x))).b;
		            col.rgb = half3(colorR, colorG, colorB);
                #elif _IMAGEBLOCKGLITCH
                    half2 block = randomNoise(floor(input.uv * _IMAGEBLOCKGLITCH_BlockSize));
		            float displaceNoise = pow(block.x, 8.0) * pow(block.x, 3.0);
		            float splitRGBNoise = pow(randomNoise(7.2341), 17.0);
		            float offsetX = displaceNoise - splitRGBNoise * _IMAGEBLOCKGLITCH_MaxRGBSplitX;
		            float offsetY = displaceNoise - splitRGBNoise * _IMAGEBLOCKGLITCH_MaxRGBSplitY;
		            float noiseX = 0.05 * randomNoise(13.0);
		            float noiseY = 0.05 * randomNoise(7.0);
		            float2 offset = float2(offsetX * noiseX, offsetY * noiseY);
		            half4 colorR = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, input.uv);
		            half4 colorG = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, input.uv + offset);
		            half4 colorB = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, input.uv - offset);
                    col = half4(colorR.r , colorG.g, colorB.z, (colorR.a + colorG.a + colorB.a));
                #elif _LINEBLOCKGLITCH
					_LINEBLOCKGLITCH_TimeX = frac(_Time.x) * 100;
					col = Horizontal(input); 
                #elif _TILEJITTERGLITCH
					col = Vertical(input);
                #elif _SCANLINEJITTERGLITCH
					col = Horizontal(input);
                #elif _DIGITALSTRIPEGLITCH
					col = _DIGITALSTRIPEGLITCH_Frag(input);
                #elif _ANALOGNOISEGLITCH

                #elif _SCREENJUMPGLITCH
					col = Vertical(input);
                #elif _SCREENSHAKEGLITCH
					col = Vertical(input);
                #elif _WAVEJITTERGLITCH
					
                #endif

                return col;
            }
            ENDHLSL
        }
    }
}

