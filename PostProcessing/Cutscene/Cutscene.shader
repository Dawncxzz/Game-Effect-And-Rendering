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

            #pragma multi_compile _ _FLIPOVER _CLOCKWIPE _DOUBLECLOCKWIPE _WEDGEWIPE _INKFADE _SLIDINGBANDS _CHECKERWIPE _DISSOLVE _DIAMONDDISSOLVE _TRIANGLEDISSOLVE _DOOR _SPIN _CENTERMERGE _CENTERSPLIT _BANDSLIDE _IRISROUND _RANDOMBLOCKS _RANDOMWIPE _GRAYSCALE

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

            TEXTURE2D_X(_SourceTex);
            float4 _SourceTex_TexelSize;
            TEXTURE2D_X(_SourceTexLowMip);
            float4 _SourceTexLowMip_TexelSize;

            float _FLIPOVER_Width;
            float _FLIPOVER_Progress;

            float _GRAYSCALE_Value;

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
                #endif

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
