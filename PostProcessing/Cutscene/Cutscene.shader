Shader "Unlit/Cutscene"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        Pass
        {
            Name "Cut scene"

            HLSLPROGRAM
            
            #pragma vertex FullscreenVert
            #pragma fragment frag

            #pragma multi_compile _ _FLIPOVER _CLOCKWIPE _DOUBLECLOCKWIPE _WEDGEWIPE _INKFADE _SLIDINGBANDS _CHECKERWIPE _DISSOLVE _DIAMONDDISSOLVE _TRIANGLEDISSOLVE _DOOR _SPIN _CENTERMERGE _CENTERSPLIT _BANDSLIDE _IRISROUND _RANDOMBLOCKS _RANDOMWIPE _GRAYSCALE _DENSEFOG1 _DENSEFOG2

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

            TEXTURE2D_X(_SourceTex);
            float4 _SourceTex_TexelSize;
            TEXTURE2D_X(_SourceTexLowMip);
            float4 _SourceTexLowMip_TexelSize;

            #if _FLIPOVER
                float _FLIPOVER_Width; 
                float _FLIPOVER_Progress;
            #endif

            #if _CLOCKWIPE
                float _CLOCKWIPE_Blend;
                float _CLOCKWIPE_Width;
            #endif

            #if _GRAYSCALE
                float _GRAYSCALE_Value;
            #endif

            #if _DENSEFOG1
                TEXTURE2D(_DENSEFOG1_MainTex); SAMPLER(sampler_DENSEFOG1_MainTex);
                TEXTURE2D(_DENSEFOG1_FlowMapTex); SAMPLER(sampler_DENSEFOG1_FlowMapTex);
                half _DENSEFOG1_Offset;
                half _DENSEFOG1_Speed;
                half _DENSEFOG1_Intensity;
                half remap(half x, half t1, half t2, half s1, half s2)
                {
                    return (x - t1) / (t2 - t1) * (s2 - s1) + s1;
                }
            #endif

            #if _DENSEFOG2
                // 输入参数
                TEXTURE2D(_DENSEFOG2_Mask);    SAMPLER(sampler_DENSEFOG2_Mask);
                TEXTURE2D(_DENSEFOG2_Noise);      SAMPLER(sampler_DENSEFOG2_Noise);
                half3 _DENSEFOG2_Noise1Params;
                half3 _DENSEFOG2_Noise2Params;
                half3 _DENSEFOG2_Color1;
                half3 _DENSEFOG2_Color2;
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
                //UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);
                // sample the texture
                //float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
                half3 col = SAMPLE_TEXTURE2D_X(_SourceTex, sampler_LinearClamp, input.uv).rgb;

                #if _FLIPOVER
                    float posmod= fmod(input.uv.x,_FLIPOVER_Width);
                    half2 uvA = half2(input.uv.x+(posmod-_FLIPOVER_Width)* _FLIPOVER_Progress/(1-_FLIPOVER_Progress), input.uv.y);
			        half2 uvB = half2(input.uv.x+(1-_FLIPOVER_Progress)*posmod /_FLIPOVER_Progress, input.uv.y);
                    half4 col2 = half4(col * 0.5, 1);
                    if(posmod/_FLIPOVER_Width<=_FLIPOVER_Progress){
                            col=col2;
                    }
                #elif _CLOCKWIPE
                    float2 newuv = floor(input.uv * _CLOCKWIPE_Width) / _CLOCKWIPE_Width;
                    float f = acos(dot(float2(1,0),normalize(newuv-0.5)));
                    if(newuv.y - 0.5 > 0)
                    {
                        f = 6.28 - f;
                    }
                    if(f < _CLOCKWIPE_Blend)
                    {
                        col.rgb *= 0.5;
                    }
                #elif _DOUBLECLOCKWIPE

                #elif _WEDEWIPE

                #elif _INKFADE

                #elif _SLIDINGBANDS

                #elif _CHECKERWIPE

                #elif _DISSOLVE

                #elif _DIAMONDDISSOLVE

                #elif _TRIANGLEDISSOLVE

                #elif _DOOR

                #elif _SPIN

                #elif _CENTERMERGE

                #elif _CENTERSPLIT

                #elif _BANDSLIDE

                #elif _IRISROUND

                #elif _RANDOMBLOCKS

                #elif _RANDOMBWIPE

                #elif _GRAYSCALE
                    col.rgb *= _GRAYSCALE_Value;
                #elif _DENSEFOG1
                    half2 newUV = input.uv;
                    newUV.x = remap(newUV.x, 1 - _DENSEFOG1_Offset, 1, 0, 1);
                    float3 flowDir = SAMPLE_TEXTURE2D(_DENSEFOG1_FlowMapTex, sampler_DENSEFOG1_FlowMapTex, newUV) * 2.0f - 1.0f;
                    flowDir *= _DENSEFOG1_Intensity;
 
                    float phase0 = abs(fmod(_Time.x * _DENSEFOG1_Speed, 2) - 1);
                    float phase1 = frac(_Time.x * _DENSEFOG1_Speed + 0.5f);
 
                    half4 tex0 = SAMPLE_TEXTURE2D(_DENSEFOG1_MainTex, sampler_DENSEFOG1_MainTex, newUV - flowDir.xy * phase0);
                    half4 tex1 = SAMPLE_TEXTURE2D(_DENSEFOG1_MainTex, sampler_DENSEFOG1_MainTex, newUV - flowDir.xy * phase1);
 
                    float flowLerp = abs((phase0 - 0.5f) / 0.5f);
                    half4 finalColor = lerp(tex0, tex1, flowLerp);
 
                    col = lerp(col, finalColor, finalColor.a * newUV.x);
                #elif _DENSEFOG2
                    half2 uv1 = input.uv * _DENSEFOG2_Noise1Params.x - float2(frac(_Time.x * _DENSEFOG2_Noise1Params.y), 0.0);
                    half2 uv2 = input.uv * _DENSEFOG2_Noise2Params.x - float2(frac(_Time.x * _DENSEFOG2_Noise2Params.y), 0.0);
                    // 扰动遮罩
                    half warpMask = SAMPLE_TEXTURE2D(_DENSEFOG2_Mask, sampler_DENSEFOG2_Mask, input.uv).b;
                    // 噪声1
                    half var_Noise1 = SAMPLE_TEXTURE2D(_DENSEFOG2_Noise, sampler_DENSEFOG2_Noise, uv1).r;
                    // 噪声2
                    half var_Noise2 = SAMPLE_TEXTURE2D(_DENSEFOG2_Noise, sampler_DENSEFOG2_Noise, uv2).g;
                    // 噪声混合
                    half noise = var_Noise1 * _DENSEFOG2_Noise1Params.z + var_Noise2 * _DENSEFOG2_Noise2Params.z;
                    // 扰动UV
                    float2 warpUV = input.uv - float2(noise, 0.0) * warpMask;
                    // 采样Mask
                    half4 var_Mask = SAMPLE_TEXTURE2D(_DENSEFOG2_Mask, sampler_DENSEFOG2_Mask, warpUV);
                    // 计算FinalRGB 不透明度
                    half3 finalRGB = _DENSEFOG2_Color1 * var_Mask.r + _DENSEFOG2_Color2 * var_Mask.g;
                    half opacity = var_Mask.r + var_Mask.g;
                    col.rgb = lerp(col, finalRGB, opacity * var_Mask.a);
                #endif

                

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
