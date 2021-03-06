Shader "Unlit/Ice1"
{
    Properties
    {
        [Header(Main Texture)]
        _MainTex         ("Main Tex",2D)                                 = "white" {}
        _IceColor        ("Ice Color",Color)                             = (1.0,1.0,1.0,1.0)
        _SpecInt         ("Specular Intensity",Float)                    = 0.0
        [HDR]_SpecCol    ("Specular Color",Color)                        = (1.0,1.0,1.0,1.0)

        [Header(Ramp Texture)]
        _RampTex         ("Texture", 2D)                                 = "white" {}
        _OutColor        ("Outside Color",Color)                         = (1.0,1.0,1.0,1.0)
        _InColor         ("Inside Color",Color)                          = (1.0,1.0,1.0,1.0)

        [Header(Program Texture)]
        _DirtTex         ("Texture", 2D)                                 = "white" {}
        _EdgeColor       ("EdgeColor",Color)                             = (1.0,1.0,1.0,1.0)
        _EdgeThickness   ("Edge thickness",Float)                        = 0.0
        _EdgeInt         ("Edge Itentsity",Float)                        = 0.0
        _EdgeScale       ("Edge Scale",Float)                            = 0.0
        _IceAmount       ("Ice Amount", Float)                           = 0.0

        [Header(Warp Texture)]
        _WarpTex         ("Warp Tex",2D)                                 = "gray"  {}
        _WarpInt         ("Warp Intensity",Float)                        = 0.0
        _WarpInt2        ("Warp Intensity2",Float)                        = 0.0

        [Header(Normal Texture)]
        _BumpMap         ("Normal Tex",2D)                               = "black"{}
        _BumpScale       ("Bump Scale",Float)                            = 0.0
        _SpecGloss       ("Specular Gloss",Float)                        = 0.0   
        _EnvGloss        ("Envornment Gloss",Float)                      = 0.0   
        _EnvSpecInt      ("Envornment Specular Intensity", Range(0, 5))  = 0.2

        [Header(Cubemap Texture)]
        _CubeMap         ("Cube Map", Cube)                              = "_Skybox"{} 
        _CubemapMip      ("Cubemap Mip", Range(0, 7))                    = 0
        
        [Header(Depth Texture)]
        _DepthTex        ("Depth Tex", 2D)                               = "white" {}
        _DepthFace       ("Depth Face", Range(0,2))                      = 0.0
        _DepthInt        ("Depth Intensity", Range(0,10))                = 0.0
        _Brightness      ("Brightness", Range(1,10))                     = 0.0
            
        _ID              ("Stencil ID",Int)                              = 1
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent+1"}

        ZWrite off

        Stencil{
            Ref [_ID]
            ReadMask 255
            WriteMask 255
            Comp Equal
        }

        GrabPass
        {
            "_Background"
        }

        Pass
        {
            Tags { "LightMode" = "ForwardBase" }

            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"
            #include "Lighting.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float3 normal : NORMAL;
                float4 tangent : TANGENT;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float4 uv : TEXCOORD0; //xy????????????uv???zw?????????uv
                float3 lDirTS : TEXCOORD1;
                float3 vDirTS : TEXCOORD2;
                float3 vrDirWS : TEXCOORD3;
                float3 nDirOS : TEXCOORD4;
                float3 vDirOS : TEXCOORD5;
                float4 scrPos : TEXCOORD6;
                float4 vertex : SV_POSITION;
            };
            uniform sampler2D _Background;
            sampler2D _CameraDepthTexture;

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform fixed4 _IceColor;
            uniform fixed4 _SpecCol;

            uniform sampler2D _RampTex;
            uniform float4 _RampTex_ST;
            uniform fixed4 _OutColor;
            uniform fixed4 _InColor;

            uniform sampler2D _DirtTex;
            uniform float4 _DirtTex_ST;
            

            uniform sampler2D _WarpTex;
            uniform float4 _WarpTex_ST;
            uniform float _WarpInt;
            uniform float _WarpInt2;

            uniform sampler2D _BumpMap;
            uniform float4 _BumpMapTex_ST;
            uniform float _BumpScale;
            uniform float _SpecGloss;
            uniform float _SpecInt;
            

            uniform samplerCUBE _CubeMap;
            uniform float _CubemapMip;
            uniform float _EnvSpecInt;
            uniform float _EnvGloss;

            uniform fixed4 _EdgeColor;
            uniform float _IceAmount;
            uniform float _EdgeThickness;
            uniform float _EdgeInt;
            uniform float _EdgeScale;


            uniform sampler2D _DepthTex;
            uniform float2 _DepthTex_ST;
            uniform float _DepthFace;
            uniform float _DepthInt;
            uniform float _Brightness;
            

            fixed2 randVec(fixed2 value)
			{
				fixed2 pos = fixed2(dot(value, fixed2(127.1, 337.1)), dot(value, fixed2(269.5, 183.3)));
				pos = frac(sin(pos) * 43758.5453123);
				return pos;
			}

            //????????????????????????(????????????)
            float2 worleyNoise(float2 uv)
			{
                //??????????????????????????????
				fixed2 index = floor(uv);
                //????????????????????????
				float2 pos = frac(uv);
				//??????x???????????????????????????y??????????????????????????????
				float2 d = float4(1.5, 1.5, 1.5, 1.5);

                fixed2 p;
                fixed2 randomp;
                float dist;

				//??????8??????????????????9??????????????????-1???1???,????????????????????????index
				for (int i = -1; i < 2; i++)
                {
					for (int j = -1; j < 2; j++)
					{
						//????????????????????????Voronoi???????????????????????????????????????????????????????????????

						//?????????????????????????????????????????????????????????
						p = randVec(index + fixed2(i, j));
						/*
                        ??????????????????????????????uv???????????????
                        randomp????????????????????????????????????????????????
                        pos???uv?????????????????????????????????,
                        dist???????????????
                        */
                        randomp = fixed2(i, j) + p;
						dist = length(randomp - pos);

						if (dist < d.x)
						{
							d.y = d.x;
							d.x = dist;
						}
						else
						{
							d.y = min(dist, d.y);
						}
					}
                }
				return d;
			}


            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv.xy = TRANSFORM_TEX(v.uv, _MainTex);
                o.uv.zw = v.uv;

                //BumpMap????????????
                TANGENT_SPACE_ROTATION;
                o.lDirTS = mul(rotation, ObjSpaceLightDir(v.vertex)).xyz;
                o.vDirTS = mul(rotation, ObjSpaceViewDir(v.vertex)).xyz;

                //CubeMap????????????
                float3 posWS = mul(unity_ObjectToWorld, v.vertex);
                float3 nDirWS = UnityObjectToWorldNormal(v.normal);
                float3 vDirWS = normalize(_WorldSpaceCameraPos.xyz - posWS);
                o.vrDirWS = reflect(-vDirWS,nDirWS);
                o.nDirOS = v.normal;
                o.vDirOS = ObjSpaceViewDir(v.vertex);

                //????????????????????????
                o.scrPos = ComputeGrabScreenPos(o.vertex);

                COMPUTE_EYEDEPTH(o.scrPos.z);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {


                //??????????????????????????????????????????????????????
                float2 d = worleyNoise(i.uv.zw * float2(2,1) * _IceAmount * 20);
                float edge = step(d.y-d.x, _EdgeThickness) ? 0 : 1;

                //????????????
                //xy????????????????????????????????????????????????
                fixed4 albedo = tex2D(_MainTex, i.uv.xy)* _IceColor;
                fixed4 packednormal = tex2D(_BumpMap, i.uv.xy);
                fixed4 depth =  tex2D(_DepthTex, i.uv.xy);
                fixed4 dirttex = tex2D(_DirtTex, i.uv.xy); 
                fixed2 warptex = tex2D(_WarpTex, i.uv.zw);

                //zw??????????????????
                fixed4 ramp = tex2D(_RampTex, i.uv.zw);
                 
                
                fixed3 cubemap = texCUBElod(_CubeMap, float4(i.vrDirWS, _CubemapMip));

                
                i.scrPos.xy +=  mul(UNITY_MATRIX_MV,normalize(i.vDirOS)).xy * _WarpInt + warptex.xy * _WarpInt2 ;
                fixed3 grabtex = tex2Dproj(_Background, i.scrPos).rgb;

                /*????????????*/

                //???????????????
                fixed3 lDirTS = normalize(i.lDirTS);
                fixed3 vDirTS = normalize(i.vDirTS);
                fixed3 nDirOS = normalize(i.nDirOS);
                fixed3 vDirOS = normalize(i.vDirOS);
                //????????????
                fixed3 nDirTS = UnpackNormal(packednormal);
                nDirTS.xy *= _BumpScale;
                nDirTS.z = sqrt(1 - saturate(dot(nDirTS.xy, nDirTS.xy)));
                //????????????
                fixed vdotn = max(0,dot(vDirOS, nDirOS));

                /*????????????*/
                //????????????
                fixed depthInt = lerp(depth.r, 0, _DepthFace);
                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)));
                float vertexDepth = i.scrPos.z;
                float dis = sceneDepth - vertexDepth;
                float depthValue = saturate((1 - dis) + _DepthInt); //???????????????????????????????????????????????????
                float clip = smoothstep(0.0,1,depthValue);
                //????????????????????????
                
                

                //??????????????????
                //fixed3 grabColor =  min((1 - clip) *  grabtex * depthInt,clip) * _Brightness;
                fixed3 grabColor =  grabtex * depthInt * clip * _Brightness;
                //fixed3 grabColor =  grabtex * depthInt;
                //????????????
                fixed3 edgeColor = lerp(step(_EdgeScale,dirttex.r) * max(0, (1 - edge)), 0,_EdgeInt) * _EdgeColor;
                //???????????????
                fixed3 diffuse = (1 - edgeColor.rgb) * albedo.rgb ;
                //????????????
                fixed3 halfdir = normalize(vDirTS + lDirTS);
                fixed3 specular = _LightColor0 * pow(max(0,dot(halfdir,nDirTS)), _SpecGloss) * _SpecInt * _SpecCol;
                //??????????????? 
                float fresnel = pow(max(0.0, 1 - vdotn), _EnvGloss);
                float3 envSpecLighting = fresnel * cubemap * _EnvSpecInt;
                
                //????????????
                fixed3 finalColor;
                finalColor = (diffuse + specular + envSpecLighting + grabColor) * lerp(_OutColor,_InColor, ramp.rgb);
                
                //return clip;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
}
