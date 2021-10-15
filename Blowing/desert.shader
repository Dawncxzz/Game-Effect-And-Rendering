Shader "Unlit/Desert"
{
    Properties
    {
        
        [Header(Wind)]
        _MainTex ("Texture", 2D) = "white" {}
        _Mask0 ("Mask0", 2D) = "gray" {}

        [Header(Sand)]
        _Mask1_1 ("Mask1_1", 2D) = "gray" {}
        _Mask1_2 ("Mask1_2", 2D) = "gray" {}
        _FlowMap1 ("FlowMap1", 2D) = "gray" {}
        _Intensity1 ("_Intensity" , Range(0,1)) = 0

        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("__src", int) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("__dst", int) = 0.0
        [Enum(off, 0, on, 1)]_ZWrite("__zw", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("__cull", Float) = 2.0



        [HDR]_Color("_Color", Color) = (1,1,1,1)
        _Speed ("_Speed", Range(0,20)) = 1
        _Alpha ("_Alpha", Range(-1,1)) = 1

        _Weight("_Weight", Vector) = (1,1,1,1)

        [Toggle] _WIND ("_Wind", Float) = 0
        [Toggle] _SAND ("_Sand", Float) = 0
    }
    SubShader
    {
        Tags{"RenderType" = "Transparent" "Queue"="Transparent" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"}
        LOD 100

        Pass
        {
            Name "BillBoard"
            Tags{"LightMode" = "UniversalForward"}
            Blend[_SrcBlend][_DstBlend]
            ZWrite[_ZWrite]
            Cull[_Cull]

            HLSLPROGRAM

            #pragma target 3.0

            #pragma multi_compile_instancing
            #pragma instancing_options maxcount:1024
            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _WIND_ON
            #pragma shader_feature _SAND_ON

            #include "Assets/ArtAssets/Shader/GArt/Library/SceneLitInput.hlsl"
            #include "Assets/ArtAssets/Shader/GArt/Library/SceneLitForwardPass.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float4 uv : TEXCOORD0;
                float4 screenPos : TEXCOORD1;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            uniform sampler2D _CameraDepthTexture;

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;

            uniform sampler2D _Mask0;
            uniform float4 _Mask0_ST;

            uniform sampler2D _Mask1_1;
            uniform float4 _Mask1_1_ST;
            uniform sampler2D _Mask1_2;
            uniform float4 _Mask1_2_ST;
            uniform sampler2D _FlowMap1;
            uniform float4 _FlowMap1_ST;
            uniform float _Intensity1;

            UNITY_INSTANCING_BUFFER_START(fengsha)
            UNITY_DEFINE_INSTANCED_PROP(half4, _Color) 
            UNITY_DEFINE_INSTANCED_PROP(float, _Speed) 
            UNITY_DEFINE_INSTANCED_PROP(float, _Alpha) 

            UNITY_INSTANCING_BUFFER_END(fengsha)

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.uv, _Mask0);
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);

                #if _WIND_ON
                    o.uv.w += frac(_Time.y * _Speed);
                #endif

                #if _SAND_ON
                    o.uv.w += frac(_Time.y * _Speed);
                #endif 
                
                o.screenPos = ComputeScreenPos(o.vertex);
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half4 col;
                float depth = tex2D(_CameraDepthTexture, i.screenPos.xy / i.screenPos.w).r;
                float eyeDepth = LinearEyeDepth(depth, _ZBufferParams);
                float depthDifference = eyeDepth - i.screenPos.w;
                #if _WIND_ON
                    col = tex2D(_MainTex, i.uv.xy);
                    half mask = tex2D(_Mask0, i.uv.zw).r;
                    

                    clip(col.r * mask - _Alpha);
                    col.r *= saturate(depthDifference);
                    return col.r * mask * _Color;
                #endif

                #if _SAND_ON
                    col = tex2D(_Mask1_1, i.uv.wz).r;
                    half4 flowmap = tex2D(_FlowMap1, i.uv.xy);
                    i.uv.zw += (flowmap.xy * 2 - 1) * _Intensity1; 
                    half mask2 = tex2D(_Mask1_2, i.uv.zw).r * 4;

                    clip(col.r * mask2 - _Alpha);
                    col.r *= saturate(depthDifference);
                    //return mask2 * _Color;
                    return col.r * mask2 * _Color;
                #endif 

                return col;
                
            }
            ENDHLSL
        }
    }
}
