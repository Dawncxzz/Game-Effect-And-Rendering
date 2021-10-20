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
	            //获取顶点的相机空间位置，同时相机空间为右手坐标系，z取反
	            o.ray = UnityObjectToViewPos(v.vertex).xyz * float3(1, 1, -1);
	            return o;
            }

            
            float4 frag(v2f i) : SV_Target
            {
	            //根据射线方向重新映射到远裁平面
	            i.ray = i.ray * (_ProjectionParams.z / i.ray.z);
	            //屏幕从0到w重新映射到0到1
	            float2 uv = i.screenUV.xy / i.screenUV.w;

	            //获取相机的深度图，并根据uv采样
	            float depth = SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture, uv);

	            // 要转换成线性的深度值
	            depth = Linear01Depth (depth);
	
	            //相机空间重新映射到要贴的物体表面
	            float4 vpos = float4(i.ray * depth,1);

	            //空间转化
	            float3 wpos = mul (unity_CameraToWorld, vpos).xyz;
	            float3 opos = mul (unity_WorldToObject, float4(wpos,1)).xyz;

	            //这个例子是用立方体来作为贴花的模型，所以要将模型空间下立方体外的repeat贴花裁剪掉
	            clip (float3(0.5,0.5,0.5) - abs(opos.xyz));

	            // 转换到 [0,1] 区间 
	            float2 texUV = opos.xz + 0.5;

	            float4 col = tex2D (_MainTex, texUV);
	            return col;
            }
            ENDCG
        }
    }
}
