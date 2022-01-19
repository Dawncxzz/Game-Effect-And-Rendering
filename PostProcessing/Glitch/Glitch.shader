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

                #elif _TILEJITTERGLITCH

                #elif _SCANLINEJITTERGLITCH

                #elif _DIGITALSTRIPEGLITCH

                #elif _ANALOGNOISEGLITCH

                #elif _SCREENJUMPGLITCH

                #elif _SCREENSHAKEGLITCH

                #elif _WAVEJITTERGLITCH

                #endif

                return col;
            }
            ENDHLSL
        }
    }
}

