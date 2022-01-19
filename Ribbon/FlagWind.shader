Shader "Q/Scene/FlagWind" {
	Properties {
		_MainTex ("Base (RGB) Gloss (A)", 2D) = "white" {} 
		_Wind("XYZ各个轴偏移 W偏移权重",Vector) = (1,1,1,1)
		_WindEdgeFlutter("幅度", float) = 0.5
		_WindEdgeFlutterFreqScale("频率",float) = 0.5
		_CutOut("裁剪" , Range(0 , 1)) = 0.5
		[Toggle]SHOW_VERTEX_COLOR("显示顶点颜色",float) = 0
	}

	SubShader {
		Tags {"Queue"="Geometry" "RenderType"="Transparent"  "RenderPipeline" = "UniversalPipeline"  }
		Cull Off

		Pass {
			Tags{"LightMode" = "UniversalForward"}
			HLSLPROGRAM
			#pragma target 3.0
			#pragma shader_feature __ SHOW_VERTEX_COLOR_ON
			#pragma vertex vert
			#pragma fragment frag
			#include "Assets/ArtAssets/Shader/GArt/Library/SceneLitInput.hlsl"
			#include "Assets/ArtAssets/Shader/GArt/Library/SceneLitForwardPass.hlsl"
			
			sampler2D _MainTex;
			half4 _MainTex_ST;
		

			struct appdata_full
			{
				half4 vertex : POSITION;
				half4 normal : NORMAL;
				half2 texcoord : TEXCOORD;
				half4 color : COLOR;
			};

			struct v2f {
				half4 pos : SV_POSITION;
				half2 uv : TEXCOORD0;
				half3 color : TEXCOORD2;
			};

			float4 SmoothCurve( float4 x ) {  
				return x * x *( 3.0 - 2.0 * x );  
			}

			float4 TriangleWave( float4 x ) {  
				return abs( frac( x + 0.5 ) * 2.0 - 1.0 );  
			}

			float4 SmoothTriangleWave( float4 x ) {  
				return SmoothCurve( TriangleWave( x ) );  
			}  

			inline float4 AnimateVertex2(half4 pos, half3 normal, half4 animParams,half4 wind,half2 time)
			{	

				half fDetailAmp = 0.3f;
				half fBranchAmp = 0.3f;
			
				// Phases (object, vertex, branch)
				half fObjPhase = dot(unity_ObjectToWorld[3].xyz, 1);
				half fBranchPhase = fObjPhase + animParams.x;
			
				half fVtxPhase = dot(pos.xyz, animParams.y + fBranchPhase);
			
				// x is used for edges; y is used for branches
				//half2 vWavesIn = time  + half2(fVtxPhase, fBranchPhase );
			
				// 1.975, 0.793, 0.375, 0.193 are good frequencies
				//half4 vWaves = (frac( vWavesIn.xxyy * half4(1.975, 0.793, 0.375, 0.193) ) * 2.0 - 1.0);
				half4 vWaves = frac((((_Time.yyyy) * float4(_WindEdgeFlutterFreqScale,_WindEdgeFlutterFreqScale,1,1))+ float4(fVtxPhase,fVtxPhase,fBranchPhase,fBranchPhase)) * float4(1.975,0.793,0.375,0.193))*2.0-1.0;
			
				vWaves = SmoothTriangleWave( vWaves );
				half2 vWavesSum = vWaves.xz + vWaves.yw;

				// Edge (xz) and branch bending (y)
				half3 bend = animParams.y * fDetailAmp * normal.xyz;
				bend.y = animParams.w * fBranchAmp;
				pos.xyz += ((vWavesSum.xyx * bend) + (wind.xyz * vWavesSum.y * animParams.w)) * wind.w; 

				pos.xyz += animParams.z * wind.xyz;
			
				return pos;
			}


		
			v2f vert (appdata_full v)
			{
				v2f o;
				half4	wind;
				half	bendingFact	= v.color.r;
			
				wind.xyz = mul((float3x3)unity_WorldToObject,_Wind.xyz);
				wind.w = _Wind.w  * bendingFact;
			
				half4	windParams	= half4(0,_WindEdgeFlutter,bendingFact.xx);
				half2 	windTime = 0;
				half4	mdlPos = AnimateVertex2(v.vertex,v.normal,windParams,wind,windTime);
			
				o.pos = mul(UNITY_MATRIX_MVP, mdlPos);
				o.uv = TRANSFORM_TEX(v.texcoord, _MainTex);
				o.color = v.color;

				half3 worldPos = mul(UNITY_MATRIX_M, v.vertex).xyz;

				return o;
			}

			half4 frag (v2f i) : COLOR
			{
				half4 tex = tex2D (_MainTex, half2(i.uv.x,i.uv.y));//修改了UV，与模型的UV有关。
				
				half4 c;
				c.rgb = tex.rgb;
				c.a = tex.a;			
				clip(c.a - _CutOut);
				#if SHOW_VERTEX_COLOR_ON
					c.rgb = i.color;
				#endif
				return c;
			}
			
			ENDHLSL
		}	

		Pass
        {
            Name "DepthOnly"
            Tags{"LightMode" = "DepthOnly"}

            ZWrite On
            ColorMask 0
            Cull[_Cull]

            HLSLPROGRAM
            // Required to compile gles 2.0 with standard srp library
            #pragma prefer_hlslcc gles
            #pragma exclude_renderers d3d11_9x
            #pragma target 2.0

            #pragma vertex DepthOnlyVertex
            #pragma fragment DepthOnlyFragment

            // -------------------------------------
            // Material Keywords
            #pragma shader_feature _ALPHATEST_ON
            // #pragma shader_feature _SMOOTHNESS_TEXTURE_ALBEDO_CHANNEL_A

            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing

            #include "Assets/ArtAssets/Shader/GArt/Library/SceneLitInput.hlsl"
            #include "Packages/com.unity.render-pipelines.universal/Shaders/DepthOnlyPass.hlsl"
            ENDHLSL
        }
	}
	//CustomEditor "FlagWindShaderGUI"
	//CustomEditor "CommonShaderGUI"
}


