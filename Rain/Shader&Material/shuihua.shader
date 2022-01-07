Shader "Unlit/ShuiHua"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("__src", int) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("__dst", int) = 0.0
        [Enum(off, 0, on, 1)]_ZWrite("__zw", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("__cull", Float) = 2.0

        _Color("_Color", Color) = (1,1,1,1)

        _Row ("_row", Int) = 0
        _Col ("_col", Int) = 0
        _Alpha ("_Alpha", Range(0,1)) = 1
        _Correct ("_Correct", Float) = 200
        
        [Toggle] _RAIN ("_Rain", Float) = 0
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

            #pragma shader_feature _RAIN_ON

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
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;

            UNITY_INSTANCING_BUFFER_START(shuihua)
            UNITY_DEFINE_INSTANCED_PROP(half4, _Color) 
            UNITY_DEFINE_INSTANCED_PROP(int  , _Row) 
            UNITY_DEFINE_INSTANCED_PROP(int  , _Col) 
            UNITY_DEFINE_INSTANCED_PROP(float, _Alpha) 
            UNITY_DEFINE_INSTANCED_PROP(float, _Scale) 
            UNITY_DEFINE_INSTANCED_PROP(float, _Correct) 
            UNITY_DEFINE_INSTANCED_PROP(float, _TimeLine) 
            UNITY_INSTANCING_BUFFER_END(shuihua)

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                #if _RAIN_ON

                    //广告牌

                    //乐乐版(旋转版)
                    //float3 normalDirOS = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz, 1)).xyz;
                    //normalDirOS.y *= _Vertical;
                    //normalDirOS = normalize(normalDirOS);
                    //float3 upDirOS = abs(normalDirOS.y) > 0.999 ? float3(0,0,1) : float3(0,1,0);
                    //float3 rightDirOS = normalize(cross(upDirOS, normalDirOS)) * -1;
                    //upDirOS = normalize(cross(normalDirOS, rightDirOS));
                    //float3 offset = v.vertex.xyz;
                    //v.vertex.xyz = normalDirOS * offset.z + upDirOS * offset.y + rightDirOS * offset.x;

                    //不旋转版(针对矩形)
                    o.vertex = mul(UNITY_MATRIX_P, 
              mul(UNITY_MATRIX_MV, float4(0.0, 0.0, 0.0, 1.0))
              + float4(-v.vertex.x, v.vertex.y, 0.0, 0.0)
              * float4(UNITY_ACCESS_INSTANCED_PROP(shuihua, _Correct) * UNITY_ACCESS_INSTANCED_PROP(shuihua, _Scale), UNITY_ACCESS_INSTANCED_PROP(shuihua, _Correct) * UNITY_ACCESS_INSTANCED_PROP(shuihua, _Scale), 1.0, 1.0));

                    //序列帧
                    float id = floor(UNITY_ACCESS_INSTANCED_PROP(shuihua, _TimeLine) * UNITY_ACCESS_INSTANCED_PROP(shuihua, _Col) * UNITY_ACCESS_INSTANCED_PROP(shuihua, _Row));
                    float rowID = floor(id / UNITY_ACCESS_INSTANCED_PROP(shuihua, _Col));
                    float colID = UNITY_ACCESS_INSTANCED_PROP(shuihua, _Col) - (rowID * UNITY_ACCESS_INSTANCED_PROP(shuihua, _Col) - id);
                    float stepRow = 1.0 / UNITY_ACCESS_INSTANCED_PROP(shuihua, _Row);
                    float stepCol = 1.0 / UNITY_ACCESS_INSTANCED_PROP(shuihua, _Col);
                    float2 initUV = o.uv * float2(stepCol, stepRow) + float2(0.0, stepRow * (UNITY_ACCESS_INSTANCED_PROP(shuihua, _Row) - 1.0));
                    o.uv = initUV + float2(stepCol * colID, -stepRow * rowID);
                    
                #else
                    o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                #endif
                
                
                
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);

                half4 col = tex2D(_MainTex, i.uv);
                col.rgb = (col.rgb + 0.5) * UNITY_ACCESS_INSTANCED_PROP(shuihua, _Color);
                col.a *= UNITY_ACCESS_INSTANCED_PROP(shuihua, _Alpha);
                return col;
            }
            ENDHLSL
        }
    }
}
