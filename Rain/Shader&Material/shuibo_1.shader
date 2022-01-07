Shader "Unlit/ShuiBo1"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}

        [Enum(UnityEngine.Rendering.BlendMode)]_SrcBlend("__src", Float) = 1.0
        [Enum(UnityEngine.Rendering.BlendMode)]_DstBlend("__dst", Float) = 0.0
        [Enum(off, 0, on, 1)]_ZWrite("__zw", Float) = 1.0
        [Enum(UnityEngine.Rendering.CullMode)]_Cull("__cull", Float) = 2.0

        _Vertical("_vertical", Range(0,1)) = 0
        _RainAmount("_RainAmount", Float) = 1
        _CircleSize("_CircleSize", Range(0,1)) = 1
        _Interval("_Interval",Range(0,2)) = 0

        _DeltaScale ("_DeltaScale", Float) = 1
        _HeightScale ("_HeightScale", Float) = 1
        _Speed("_Speed", Float) = 1


        
        [Toggle] _RAIN ("_Rain", Float) = 0
    }
    SubShader
    {
        Tags{"RenderType" = "Transparent" "Queue"="Transparent+700" "RenderPipeline" = "UniversalPipeline" "UniversalMaterialType" = "Lit" "IgnoreProjector" = "True"}
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

            #pragma vertex vert
            #pragma fragment frag

            #pragma shader_feature _RAIN_ON

            #include "Assets/ArtAssets/Shader/GArt/Library/SceneLitInput.hlsl"
            #include "Assets/ArtAssets/Shader/GArt/Library/SceneLitForwardPass.hlsl"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                float3 worldPos : TEXCOORD1;
                float4 screenPos : TEXCOORD2;
                UNITY_VERTEX_INPUT_INSTANCE_ID
                UNITY_VERTEX_OUTPUT_STEREO
            };

            uniform sampler2D _CameraOpaqueAndTransparentTexture;
            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform float4 _MainTex_TexelSize;
            uniform float _Vertical;
            
            uniform float _RainAmount;
            uniform float _CircleSize;
            uniform float _Interval;

            uniform float _DeltaScale;
            uniform float _HeightScale;
            uniform float _Speed;

            float RangeRandom(float minValue, float maxValue, float2 fractor)
            {
                float a = dot(fractor, float2(411.12, 111.65));
                float b = frac(sin(1361.12) * cos(a) * a);
                return lerp(minValue, maxValue, b);
            }

            float2 GetRainRadient(float2 uv)
            {
                    float2 floorUV = floor(uv);   
                    float2 fracUV = frac(uv);
                    float randomUV1 = RangeRandom(0.3,0.7,floorUV); 
                    float randomUV2 = RangeRandom(0.3,0.7,floorUV + float2(1,1));
                    float distance = length(float2(randomUV1,randomUV2) - fracUV);
                    float timeFractor = 1 - (frac(RangeRandom(0,1,floorUV + float2(2,2)) + _Time.y * _Speed) + 0.5);


                    float gradient1 = lerp(0,_Interval,frac(RangeRandom(0,1,floorUV + float2(2,2)) + _Time.y * _Speed));

                    float gradient2 = lerp(1,0, (timeFractor + distance) * (1/_CircleSize)) * lerp(0,1, (timeFractor + distance) * (1/_CircleSize));

                    float lerpValue = smoothstep(0, 1, gradient2);
                    float alpha = (1 - gradient1);
                    return 1 - lerpValue * alpha;
            }

            float3 GetNormalByGray(float2 uv)
            {
                float2 deltaU = float2(1 / (_ScreenParams.x * _RainAmount) * _DeltaScale, 0);
                float h1_u = GetRainRadient(uv - deltaU);
                float h2_u = GetRainRadient(uv + deltaU);
                float3 tangent_u = float3(deltaU.x, 0, (h2_u - h1_u) * _HeightScale);

                float2 deltaV = float2(0, 1 / (_ScreenParams.y * _RainAmount) * _DeltaScale);
                float h1_v = GetRainRadient(uv - deltaV);
                float h2_v = GetRainRadient(uv + deltaV);
                float3 tangent_v = float3(0, deltaV.y, (h2_v - h1_v)  * _HeightScale);

                float3 normal = normalize(cross(tangent_u, tangent_v));

                return normal;

            }

            v2f vert (appdata v)
            {
                v2f o = (v2f)0;

                UNITY_SETUP_INSTANCE_ID(v);
                UNITY_TRANSFER_INSTANCE_ID(v, o);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o);

                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                #if _RAIN_ON

                    //广告牌(向上版)
                    //float3 normalDirOS = mul(unity_WorldToObject, float4(0, 1, 0, 0)).xyz;
                    //normalDirOS.y *= _Vertical;
                    //normalDirOS = normalize(normalDirOS);
                    //float3 upDirOS = abs(normalDirOS.y) > 0.999 ? float3(0,0,1) : float3(0,1,0);
                    //float3 rightDirOS = normalize(cross(upDirOS, normalDirOS));
                    //upDirOS = normalize(cross(normalDirOS, rightDirOS));
                    //float3 offset = v.vertex.xyz;
                    //v.vertex.xyz = normalDirOS * offset.z + upDirOS * offset.y + rightDirOS * offset.x;

                    

                    float3 binormal = cross(normalize(v.normal), normalize(v.tangent.xyz)) * v.tangent.w;
                    float3x3 rotation = float3x3(v.tangent.xyz, binormal, v.normal);

                #endif
                
                o.vertex = mul(UNITY_MATRIX_MVP, v.vertex);
                o.worldPos = mul(unity_ObjectToWorld, v.vertex);
                o.screenPos = ComputeScreenPos(o.vertex);
                
                return o;
            }

            half4 frag (v2f i) : SV_Target
            {
                UNITY_SETUP_INSTANCE_ID(i);
                UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
                half3 finalColor = tex2D(_CameraOpaqueAndTransparentTexture, i.screenPos.xy / i.screenPos.w).rgb;
                #if _RAIN_ON
                    float rainColor = GetRainRadient(i.worldPos.xz * _RainAmount);
                    float3 normal = half4(GetNormalByGray(i.worldPos.xz * _RainAmount), 1);
                    finalColor = tex2D(_CameraOpaqueAndTransparentTexture, i.screenPos.xy / i.screenPos.w + normal.xy ).rgb;
                    clip(saturate(1 - rainColor) - 0.03);
                #endif




                //return rain.x * rain.y;
                //return saturate(float4(rainColor, rainColor, rainColor, 1 - rainColor));
                
                return half4(finalColor, 1);
                //return half4(GetNormalByGray(i.uv * _RainAmount), 1);
            }
            ENDHLSL
        }
    }
}
