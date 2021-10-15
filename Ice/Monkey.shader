Shader "Unlit/Monkey"
{
    Properties
    {
        _MainTex         ("Main Tex",2D)                                 = "white" {}
        _IceColor        ("Ice Color",Color)                             = (1.0,1.0,1.0,1.0)
        _SpecInt         ("Specular Intensity",Float)                    = 0.0

        _BumpMap         ("Normal Tex",2D)                               = "black"{}
        [HDR]_SpecCol    ("Specular Color",Color)                        = (1.0,1.0,1.0,1.0)
        _BumpScale       ("Bump Scale",Float)                            = 0.0
        _SpecGloss       ("Specular Gloss",Float)                        = 0.0   
        _EnvGloss        ("Envornment Gloss",Float)                      = 0.0   
        _EnvSpecInt      ("Envornment Specular Intensity", Range(0, 5))  = 0.2

        _CubeMap         ("Cube Map", Cube)                              = "_Skybox"{}
        _CubemapMip      ("Cubemap Mip", Range(0, 7))                    = 0

        _EmissionTex     ("Emission Texture",2D)                         = "black"{}
        [HDR]_EmissionColor   ("Emission Color",Color)                   = (1.0,1.0,1.0,1.0)
            
        _ID              ("Stencil ID",Int)                              = 1
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1" }

        Stencil{
            Ref [_ID]
            ReadMask 255
            WriteMask 255
            Comp Equal
        }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #pragma multi_compile_fwdbase

            #include "UnityCG.cginc"
            #include "Lighting.cginc"
            #include "AutoLight.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0; //xy存储脏迹uv，zw为原始uv
                float3 lDirTS : TEXCOORD1;
                float3 vDirTS : TEXCOORD2;
                float3 vrDirWS : TEXCOORD3;
                float3 nDirOS : TEXCOORD4;
                float3 vDirOS : TEXCOORD5;
                SHADOW_COORDS(6)
                float4 pos : SV_POSITION;
            };
            uniform sampler2D _Background;

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform fixed4 _IceColor;


            uniform sampler2D _BumpMap;
            uniform float4 _BumpMapTex_ST;
            uniform float4 _BumpScale;
            uniform fixed4 _SpecCol;
            uniform float _SpecGloss;
            uniform float _SpecInt;
            

            uniform samplerCUBE _CubeMap;
            uniform float _CubemapMip;
            uniform float _EnvSpecInt;
            uniform float _EnvGloss;

            uniform sampler2D _EmissionTex;
            uniform fixed4 _EmissionColor;

          
            v2f vert (appdata v)
            {
                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = v.uv;

                //BumpMap向量准备
                TANGENT_SPACE_ROTATION;
                o.lDirTS = mul(rotation, ObjSpaceLightDir(v.vertex)).xyz;
                o.vDirTS = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;

                //CubeMap向量准备
                float3 posWS = mul(unity_ObjectToWorld, v.vertex);
                float3 nDirWS = UnityObjectToWorldNormal(v.normal);
                float3 vDirWS = normalize(_WorldSpaceCameraPos.xyz - posWS);
                o.vrDirWS = reflect(-vDirWS,nDirWS);
                o.nDirOS = v.normal;
                o.vDirOS = ObjSpaceViewDir(v.vertex);

                TRANSFER_SHADOW(o);

                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                //纹理采样
                //xy用于反射率和法线贴图
                fixed4 albedo = tex2D(_MainTex, i.uv.xy)* _IceColor;
                fixed4 packednormal = tex2D(_BumpMap, i.uv.xy);
                fixed3 cubemap = texCUBElod(_CubeMap, float4(i.vrDirWS, _CubemapMip));
                fixed3 emisionmap = tex2D(_EmissionTex,i.uv.xy).rgb;


                /*向量处理*/

                //向量归一化
                fixed3 lDirTS = normalize(i.lDirTS);
                fixed3 vDirTS = normalize(i.vDirTS);
                fixed3 nDirOS = normalize(i.nDirOS);
                fixed3 vDirOS = normalize(i.vDirOS);
                //法线处理
                fixed3 nDirTS = UnpackNormal(packednormal);
                nDirTS.xy *= _BumpScale;
                nDirTS.z = sqrt(1 - dot(nDirTS.xy, nDirTS.xy));
                //向量计算
                fixed vdotn = max(0,dot(vDirOS, nDirOS));
                fixed ndotl = max(0,dot(UnityObjectToWorldDir(nDirOS), _WorldSpaceLightPos0.xyz));
                /*光照计算*/
                //自发光
                fixed3 emission = emisionmap.r * _EmissionColor;
                //漫反射计算
                fixed3 diffuse =  albedo.rgb * ndotl;
                //高光计算
                fixed3 halfdir = normalize(vDirTS + lDirTS);
                fixed3 specular = _LightColor0 * pow(max(0,dot(halfdir,nDirTS)), _SpecGloss) * _SpecInt * ndotl * _SpecCol;
                //环境光计算 
                float fresnel = pow(max(0.0, 1 - vdotn), _EnvGloss);
                float3 envSpecLighting = fresnel * cubemap * _EnvSpecInt;
                //阴影
                //平行光的衰减值为1
                fixed shadow = SHADOW_ATTENUATION(i);
                //光照混合
                fixed3 finalColor;
                finalColor = (diffuse + specular + envSpecLighting) * shadow + emission;

                //return edge;
                return fixed4(finalColor,1);
            }
            ENDCG
        }

    }
    FallBack "Diffuse"
}
