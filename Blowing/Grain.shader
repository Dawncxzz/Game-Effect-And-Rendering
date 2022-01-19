Shader "Unlit/Grain"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {} 
        _Mask ("Mask", 2D) = "gray" {}
        _FlowMap ("_FlowMap", 2D) = "black" {}

        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("__src", int) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("__dst", int) = 0.0
        [Enum(off, 0, on, 1)]_ZWrite("__zw", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("__cull", Float) = 2.0



        [HDR]_Color("_Color", Color) = (1,1,1,1)
        _Speed ("_Speed", Range(0,20)) = 1
        _Alpha ("_Alpha", Range(-1,1)) = 1
        _WarpInt ("_WarpInt", Float) = 0

        _Weight("_Weight", Vector) = (1,1,1,1)
        
        
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
            uniform sampler2D _Mask;
            uniform float4 _Mask_ST;
            uniform sampler2D _FlowMap;
            uniform float4 _FlowMap_ST;

            UNITY_INSTANCING_BUFFER_START(fengsha)
            UNITY_DEFINE_INSTANCED_PROP(half4, _Color) 
            UNITY_DEFINE_INSTANCED_PROP(float, _Speed) 
            UNITY_DEFINE_INSTANCED_PROP(float, _Alpha) 
            UNITY_DEFINE_INSTANCED_PROP(float, _WarpInt) 
            UNITY_INSTANCING_BUFFER_END(fengsha)

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                VertexPositionInputs vertexInput = GetVertexPositionInputs(v.vertex.xyz);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = TRANSFORM_TEX(v.uv, _Mask);
                
                

                #if _SAND_ON
                    half4 warp = tex2Dlod(_FlowMap, float4(frac(_Time.y * _Speed), o.uv.y,0,0));
                    v.vertex.xy += frac(_Time.y * _Speed) * (warp.xy * 2 - 1) * _WarpInt;
                #endif
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                
                return o;
            }
            

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half mask = tex2D(_Mask, i.uv.zw).r;
                half4 warp = tex2D(_FlowMap, i.uv.xy);
                i.uv.x += frac(_Time.y * _Speed);
                i.uv.xy += (warp.xy * 2 - 1) * _WarpInt;
                half4 col = tex2D(_MainTex, i.uv.xy);

                float depth = tex2D(_CameraDepthTexture, i.screenPos.xy / i.screenPos.w).r;
                float eyeDepth = LinearEyeDepth(depth, _ZBufferParams);
                float depthDifference = eyeDepth - i.screenPos.w;
                clip(col.r * mask - _Alpha);
                col.r *= saturate(depthDifference);
                return col.r * mask * _Color;
            }
            ENDHLSL
        }
    }
}
