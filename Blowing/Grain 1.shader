Shader "Unlit/Grain1"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Length("Length",float) = 1.0
		_Color("Color",Color)=(1,1,1,1)
	}
		SubShader
		{
			Tags { "RenderType" = "Opaque" }
			LOD 100

			Pass
			{
				CGPROGRAM

				#pragma vertex vert
				#pragma fragment frag
				//������ɫ��
				#pragma geometry geom
				#include "UnityCG.cginc"

				float _Length;
				sampler2D _MainTex;
				float4 _MainTex_ST;
				float4 _Color;

				struct a2v {
					float4 vertex:POSITION;
					float2 uv:TEXCOORD0;
				};

				struct v2g {
					float4 vertex:POSITION;
					float2 uv:TEXCOORD0;
				};

				struct g2f {
					float4 vertex:SV_POSITION;
					float2 uv:TEXCOORD0;
					float4 col:COLOR;
				};

				v2g vert(a2v v) {
					v2g o;
					o.vertex = v.vertex;
					o.uv = TRANSFORM_TEX(v.uv, _MainTex);
					return o;
				}

			    float4 RandomPos(float4 position)
				{
					float3 offset;
					offset = float3(dot(position, float3(124.52,487.12,645.32)), dot(position, float3(763.52,173.12,752.32)), dot(position, float3(876.52,192.12,108.32)));
					position.xyz += frac(sin(54632.123) * offset); 
					return position;

				}

				//һ�����������ɵ���ඥ����
				[maxvertexcount(9)]
				void geom(triangle v2g IN[3], inout TriangleStream<g2f> tristream) {
					g2f o;
					//��������������߷���
					float3 edgeA = IN[1].vertex - IN[0].vertex;
					float3 edgeB = IN[2].vertex - IN[0].vertex;
					float3 normalFace = normalize(cross(edgeA, edgeB));
					//���������ĵ�
					float3 centerPos = (IN[0].vertex + IN[1].vertex + IN[2].vertex) / 3;
					//���ĵ�uvλ��
					float2 centerTex = (IN[0].uv + IN[1].uv + IN[2].uv) / 3;
					//���صĶ������
					centerPos += float4(normalFace, 0)*_Length;
					for (uint i = 0; i < 3; i++)
					{
						o.vertex = UnityObjectToClipPos(RandomPos(IN[i].vertex));
						o.uv = IN[i].uv;
						o.col = fixed4(0., 0., 0., 1.);
						//��Ӷ���
						tristream.Append(o);

						uint index = (i + 1) % 3;
						o.vertex = UnityObjectToClipPos(RandomPos(IN[index].vertex));
						o.uv = IN[index].uv;
						o.col = fixed4(0., 0., 0., 1.);

						tristream.Append(o);

						//�ⲿ��ɫ��
						o.vertex = UnityObjectToClipPos(RandomPos(float4(centerPos + float3(i,i,i), 1)));
						o.uv = centerTex;
						o.col = fixed4(1.0, 1.0, 1.0, 1.);

						tristream.Append(o);
						//���������
						tristream.RestartStrip();
					}
				}

				float4 frag(g2f i) :SV_Target{
					float4 col = tex2D(_MainTex,i.uv)*i.col*_Color;
					return col;
				}
			ENDCG

		}
	}
}

