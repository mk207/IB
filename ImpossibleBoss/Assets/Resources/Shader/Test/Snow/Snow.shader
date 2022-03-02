Shader "Custom/Snow"
{
    Properties
    {
        _MainTex ("Height Map", 2D) = "white" {}        
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        
        LOD 200

        pass 
        {
             CGPROGRAM
            #pragma target 3.0
            #pragma vertex vert
            #pragma fragment frag 

            sampler2D _MainTex;

            struct appdata
            {
                float4 vertex : POSITION;
                float4 uv : TEXCOORD;
                float3 normal : NORMAL;
            };

            struct v2f
            {
                float4 pos : SV_POSITION;
                float4 uv : TEXCOORD;
                float3 normal : NORMAL;
            };

            float _OutlineWidth;
            float4 _OutlineColor;

            v2f vert(appdata v)
            {
                v.vertex.y += tex2Dlod(_MainTex, v.uv).r;

                v2f o;
                o.pos = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.normal = v.normal;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                fixed c = saturate(dot(i.normal, fixed3(1,1,1)));
                return fixed4(c,c,c,1);
            }
            ENDCG
        }
       
    }
    FallBack "Diffuse"
}
