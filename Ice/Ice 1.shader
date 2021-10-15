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
                float4 uv : TEXCOORD0; //xy存储脏迹uv，zw为原始uv
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

            //沃利噪声程序纹理(细胞纹理)
            float2 worleyNoise(float2 uv)
			{
                //当前点的向下取整坐标
				fixed2 index = floor(uv);
                //当前点的小数坐标
				float2 pos = frac(uv);
				//其中x分量存储最短距离，y分量存储第二近的距离
				float2 d = float4(1.5, 1.5, 1.5, 1.5);

                fixed2 p;
                fixed2 randomp;
                float dist;

				//周围8点加上自己共9个点范围为（-1，1）,实际使用时需加上index
				for (int i = -1; i < 2; i++)
                {
					for (int j = -1; j < 2; j++)
					{
						//判断该点属于哪个Voronoi块内，进行距离判断的时候是在局部坐标进行的

						//先根据当前点的全局坐标位置生成随机向量
						p = randVec(index + fixed2(i, j));
						/*
                        然后获取该随机点到该uv点的距离，
                        randomp为局部坐标系中当前点的随机位置，
                        pos为uv坐标在局部坐标系的位置,
                        dist为两者距离
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

                //屏幕扰动向量准备
                o.scrPos = ComputeGrabScreenPos(o.vertex);

                COMPUTE_EYEDEPTH(o.scrPos.z);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {


                //噪声边缘获取（黑色为边缘，白色为块）
                float2 d = worleyNoise(i.uv.zw * float2(2,1) * _IceAmount * 20);
                float edge = step(d.y-d.x, _EdgeThickness) ? 0 : 1;

                //纹理采样
                //xy用于反射率、法线贴图、深度、脏迹
                fixed4 albedo = tex2D(_MainTex, i.uv.xy)* _IceColor;
                fixed4 packednormal = tex2D(_BumpMap, i.uv.xy);
                fixed4 depth =  tex2D(_DepthTex, i.uv.xy);
                fixed4 dirttex = tex2D(_DirtTex, i.uv.xy); 
                fixed2 warptex = tex2D(_WarpTex, i.uv.zw);

                //zw用于渐变遮罩
                fixed4 ramp = tex2D(_RampTex, i.uv.zw);
                 
                
                fixed3 cubemap = texCUBElod(_CubeMap, float4(i.vrDirWS, _CubemapMip));

                
                i.scrPos.xy +=  mul(UNITY_MATRIX_MV,normalize(i.vDirOS)).xy * _WarpInt + warptex.xy * _WarpInt2 ;
                fixed3 grabtex = tex2Dproj(_Background, i.scrPos).rgb;

                /*向量处理*/

                //向量归一化
                fixed3 lDirTS = normalize(i.lDirTS);
                fixed3 vDirTS = normalize(i.vDirTS);
                fixed3 nDirOS = normalize(i.nDirOS);
                fixed3 vDirOS = normalize(i.vDirOS);
                //法线处理
                fixed3 nDirTS = UnpackNormal(packednormal);
                nDirTS.xy *= _BumpScale;
                nDirTS.z = sqrt(1 - saturate(dot(nDirTS.xy, nDirTS.xy)));
                //向量计算
                fixed vdotn = max(0,dot(vDirOS, nDirOS));

                /*光照计算*/
                //深度获取
                fixed depthInt = lerp(depth.r, 0, _DepthFace);
                float sceneDepth = LinearEyeDepth(SAMPLE_DEPTH_TEXTURE_PROJ(_CameraDepthTexture, UNITY_PROJ_COORD(i.scrPos)));
                float vertexDepth = i.scrPos.z;
                float dis = sceneDepth - vertexDepth;
                float depthValue = saturate((1 - dis) + _DepthInt); //摄像机视角的深度减去当前物体的深度
                float clip = smoothstep(0.0,1,depthValue);
                //计算当前像素深度
                
                

                //屏幕抓取扰动
                //fixed3 grabColor =  min((1 - clip) *  grabtex * depthInt,clip) * _Brightness;
                fixed3 grabColor =  grabtex * depthInt * clip * _Brightness;
                //fixed3 grabColor =  grabtex * depthInt;
                //边缘计算
                fixed3 edgeColor = lerp(step(_EdgeScale,dirttex.r) * max(0, (1 - edge)), 0,_EdgeInt) * _EdgeColor;
                //漫反射计算
                fixed3 diffuse = (1 - edgeColor.rgb) * albedo.rgb ;
                //高光计算
                fixed3 halfdir = normalize(vDirTS + lDirTS);
                fixed3 specular = _LightColor0 * pow(max(0,dot(halfdir,nDirTS)), _SpecGloss) * _SpecInt * _SpecCol;
                //环境光计算 
                float fresnel = pow(max(0.0, 1 - vdotn), _EnvGloss);
                float3 envSpecLighting = fresnel * cubemap * _EnvSpecInt;
                
                //光照混合
                fixed3 finalColor;
                finalColor = (diffuse + specular + envSpecLighting + grabColor) * lerp(_OutColor,_InColor, ramp.rgb);
                
                //return clip;
                return fixed4(finalColor,1);
            }
            ENDCG
        }
    }
}
