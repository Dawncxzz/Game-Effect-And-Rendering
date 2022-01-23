Shader "Unlit/Cutscene"
{
    SubShader
    {
        Tags { "RenderType"="Opaque" "RenderPipeline" = "UniversalPipeline"}

        Stencil
        {
            Ref 1
            Comp Equal
            Pass Keep
            Fail Keep
        }

        Pass
        {
            Name "Ghost Effect"

            HLSLPROGRAM
            
            #pragma vertex FullscreenVert
            #pragma fragment frag

            #pragma multi_compile _ _PLAYER

            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
            #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Filtering.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

            TEXTURE2D_X(_SourceTex);
            float4 _SourceTex_TexelSize;
            TEXTURE2D_X(_SourceTexLowMip);
            float4 _SourceTexLowMip_TexelSize;

            #if _PLAYER
                float _PLAYER_Width; 
                float _PLAYER_Progress;
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

                #if _PLAYER
                    col *= 0.5;
                #endif

                return half4(col, 1);
            }
            ENDHLSL
        }
    }
}
