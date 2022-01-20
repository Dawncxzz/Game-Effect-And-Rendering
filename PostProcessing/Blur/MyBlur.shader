Shader "Unlit/MyBlur"
{
    SubShader
    {
        Tags { "RenderPipeline" = "UniversalPipeline" "RenderType"="Opaque" }

        Pass
        {
            Name "My Blur"

            HLSLPROGRAM
            #pragma vertex FullscreenVert
            #pragma fragment frag

            #pragma multi_compile _ _GAUSSIANBLUR _BOXBLUR _KAWASEBLUR _DUALBLUR _BOKEHBLUR _TILTSHIFTBLUR _IRISBLUR _GRAINYBLUR _RADIALBLUR _DIRECTIONALBLUR

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"
            
            TEXTURE2D_X(_SourceTex);
            float4 _SourceTex_TexelSize;
            TEXTURE2D_X(_SourceTexLowMip);
            float4 _SourceTexLowMip_TexelSize;

            #if _GAUSSIANBLUR
                float4 _GAUSSIANBLUR_Offsets;
                float4 FragGaussianBlur(float2 uv)
                {
                    half4 color = float4(0, 0, 0, 1);

                    half4 uv01 = uv.xyxy + _GAUSSIANBLUR_Offsets.xyxy * float4(1, 1, -1, -1);
		            half4 uv23 = uv.xyxy + _GAUSSIANBLUR_Offsets.xyxy * float4(1, 1, -1, -1) * 2.0;
		            half4 uv45 = uv.xyxy + _GAUSSIANBLUR_Offsets.xyxy * float4(1, 1, -1, -1) * 3.0;

                    color += 0.40 * SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv);
                    color += 0.15 * SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv01.xy);
                    color += 0.15 * SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv01.zw);
                    color += 0.10 * SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv23.xy);
                    color += 0.10 * SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv23.zw);
                    color += 0.05 * SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv45.xy);
                    color += 0.05 * SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, uv45.zw);

                    return color;
                }
            #endif

            #if _BOXBLUR
                half4 _BOXBLUR_BlurOffset;
                half4 BoxFilter_4Tap(float2 uv, float2 texelSize)
	            {
		            float4 d = texelSize.xyxy * float4(-1.0, -1.0, 1.0, 1.0);
		
		            half4 s = 0;
		            s = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, uv + d.xy) * 0.25h;  // 1 MUL
		            s += SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, uv + d.zy) * 0.25h; // 1 MAD
		            s += SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, uv + d.xw) * 0.25h; // 1 MAD
		            s += SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, uv + d.zw) * 0.25h; // 1 MAD
		
		            return s;
	            }
            #endif

            #if _BOKEHBLUR
                float4 _BOKEHBLUR_GoldenRot;
                float _BOKEHBLUR_Iteration;
                float _BOKEHBLUR_Radius;
                float2 _BOKEHBLUR_PixelSize;
                half4 BokehBlur(Varyings i)
                {
                    half2x2 rot = half2x2(_BOKEHBLUR_GoldenRot);
                    half4 accumulator = 0.0;
                    half4 divisor = 0.0;

                    half r = 1.0;
                    half2 angle = half2(0.0, _BOKEHBLUR_Radius);

                    for (int j = 0; j < _BOKEHBLUR_Iteration; j++)
                    {
                        r += 1.0 / r;
                        angle = mul(rot, angle);
                        half4 bokeh = SAMPLE_TEXTURE2D(_SourceTex, sampler_LinearClamp, float2(i.uv + _BOKEHBLUR_PixelSize * (r - 1.0) * angle));
                        accumulator += bokeh * bokeh;
                        divisor += bokeh;
                    }
                    return accumulator / divisor;
                }
            #endif


            half4 frag (Varyings input) : SV_Target
            {
                half4 col = half4(0, 0, 0, 1);
                #if _GAUSSIANBLUR
                    col = FragGaussianBlur(input.uv);
                #elif _BOXBLUR
                    col = BoxFilter_4Tap(input.uv, _BOXBLUR_BlurOffset.xy).rgba;
                #elif _KAWASEBLUR
                    
                #elif _DUALBLUR

                #elif _BOKEHBLUR
                    col = BokehBlur(input);
                #elif _TILTSHIFTBLUR

                #elif _IRISBLUR

                #elif _GRAINYBLUR

                #elif _RADIALBLUR

                #elif _DIRECTIONALBLUR

                #endif

                return col;
            }
            ENDHLSL
        }
    }
}
