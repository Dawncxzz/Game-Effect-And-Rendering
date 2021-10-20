Shader "Unlit/Decal"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" "Queue"="Geometry+1"}
        LOD 100

        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag

            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
            };

            struct v2f
            {
                float4 screenUV : TEXCOORD1;
                float3 ray : TEXCOORD2;
                float4 pos : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            sampler2D _CameraDepthTexture;
            

            v2f vert (appdata_base v)
            {
	            v2f o;
	            o.pos = UnityObjectToClipPos (v.vertex);
	            o.screenUV = ComputeScreenPos (o.pos);
	            //��ȡ���������ռ�λ�ã�ͬʱ����ռ�Ϊ��������ϵ��zȡ��
	            o.ray = UnityObjectToViewPos(v.vertex).xyz * float3(1, 1, -1);
	            return o;
            }

            
            float4 frag(v2f i) : SV_Target
            {
	            //�������߷�������ӳ�䵽Զ��ƽ��
	            i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
	            //��Ļ��0��w����ӳ�䵽0��1
	            float2 uv = i.screenUV.xy / i.screenUV.w;

	            //��ȡ��������ͼ��������uv����
	            float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);

	            // Ҫת�������Ե����ֵ
	            depth = Linear01Depth (depth);
	
	            //����ռ�����ӳ�䵽Ҫ�����������
	            float4 vpos = float4(i.ray * depth,1);

	            //�ռ�ת��
	            float3 wpos = mul (unity_CameraToWorld, vpos).xyz;
	            float3 opos = mul (unity_WorldToObject, float4(wpos,1)).xyz;

	            //���������������������Ϊ������ģ�ͣ�����Ҫ��ģ�Ϳռ������������repeat�����ü���
	            clip (float3(0.5,0.5,0.5) - abs(opos.xyz));

	            // ת���� [0,1] ���� 
	            float2 texUV = opos.xz + 0.5;

	            float4 col = tex2D (_MainTex, texUV);
	            return col;
            }
            ENDCG
        }
    }
}
