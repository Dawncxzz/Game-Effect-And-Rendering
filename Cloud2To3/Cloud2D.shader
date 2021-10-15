Shader "Unlit/Cloud2D"
{
    Properties
    {
        _MainTex        ("Texture", 2D)                     = "white" {}
        _Mask1          ("X:Right Y:Top Z:Front",2D)        = "black" {}
        _Mask2          ("X:Left Y:Bottom Z:Back",2D)       = "gray"  {}
        _SSS            ("Subsurface Scater",Range(0,3))    = 0.0
        _SSSScale       ("SSS Scaele",Range(0,1))           = 0.0
        _SSSRim         ("SSS Rim",Range(0,1))              = 0.0

    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}
        LOD 100

        Pass
        {

            Blend SrcAlpha OneMinusSrcAlpha
            
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            // make fog work
            #pragma multi_compile_fog

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
                float2 uv : TEXCOORD0;
                float3 oldir : TEXCOORD1;
                float3 ovdir : TEXCOORD2;
                float4 vertex : SV_POSITION;
            };

            uniform sampler2D _MainTex;
            uniform float4 _MainTex_ST;
            uniform sampler2D _Mask1;
            uniform sampler2D _Mask2;
            float _SSS;
            float _SSSScale;
            float _SSSRim;

            

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;

                o.oldir = mul(unity_WorldToObject, float4(_WorldSpaceLightPos0.xyz,0)).xyz;
                o.ovdir = mul(unity_WorldToObject, float4(_WorldSpaceCameraPos.xyz,1)).xyz - float3(0,0,0);
                
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 dirttex = tex2D(_MainTex, i.uv);
                //获取mask图
                fixed3 mask1 = tex2D(_Mask1, i.uv).rgb; //right,top,front
                fixed3 mask2 = tex2D(_Mask2, i.uv).rgb; //left,bottom,back

                //向量归一化
                fixed3 oldir = normalize(i.oldir);
                fixed3 ovdir = normalize(i.ovdir);

                //计算方向权重
                //fixed right = smoothstep(-1,1,oldir.x);
                //fixed left = 1 - right;
                //fixed top = smoothstep(-1,1,oldir.z);
                //fixed bottom = 1 - top;
                //fixed back = smoothstep(-1,1,oldir.y);
                //fixed front = 1 - back;

                //计算方向权重
                fixed right = oldir.x < 0 ? abs(oldir.x) : 0;
                fixed left = oldir.x >= 0 ? abs(oldir.x) : 0;
                fixed top = oldir.z < 0 ? abs(oldir.z) : 0;
                fixed bottom = oldir.z >= 0 ? abs(oldir.z) : 0;
                fixed back = oldir.y < 0 ? abs(oldir.y): 0;
                fixed front = oldir.y >= 0 ? abs(oldir.y) : 0;


                //输出通道信息（r为左右，g为前后，b为前，a为后）
                fixed4 channel;
                channel.r = mask1.r * right + mask2.r * left;
                channel.g = mask1.g * top   + mask2.g * bottom;
                channel.b = mask1.b * front;


                //背面次表面反射
                fixed lrmask = max(max(mask1.r, mask2.r) - _SSSScale,0);
                fixed bmask = 1 - mask2.b;
                fixed SSSarea = bmask * (1 - lrmask);

                //计算透射部分
                fixed transmission = SSSarea * back * _SSS;

                //计算背面部分的非透射颜色
                channel.a = (1 - SSSarea) * _SSSRim * back * max(0.5,1 -_SSS);

                //SSS颜色（这里直接使用b通道，因为朝向非前即后）
                fixed3 SSSColor = channel.a * dirttex.rgb + transmission * _LightColor0.rgb * dirttex.rgb;

                //漫反射颜色
                fixed3 diffuse = (channel.r + channel.g + channel.b) * dirttex.rgb * _LightColor0.rgb;

                //根据通道信息输出颜色
                fixed3 finalColor;
                finalColor.rgb = diffuse + SSSColor;

                //return fixed4(channel.a, 0 ,transmission,dirttex.a);
                return fixed4(finalColor,dirttex.a);
            }
            ENDCG
        }
    }
}
