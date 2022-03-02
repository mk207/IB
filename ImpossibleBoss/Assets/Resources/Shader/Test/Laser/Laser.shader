Shader "Unlit/Laser"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _LaserSpeed("Laser Speed", Range(0, 10)) = 1
        [HDR] _Color("Color", Color) = (1,0,0,1)
        [HDR] _GlowColor("GlowColor", Color) = (1,0,0,1)
        _Cutoff("Alpha cutoff", Range(0,1)) = 0
    }
    SubShader
    {
        Tags { "RenderType"="Opaque" }
        LOD 100

        
            CGPROGRAM
            #pragma surface surf Lambert noambient alphatest:_Cutoff

            //#include "UnityCG.cginc"

            sampler2D _MainTex;
            //float4 _MainTex_ST;
            float4 _Color;
            float4 _GlowColor;
            float _LaserSpeed;

            struct Input
            {
                float2 uv_MainTex;
                float4 vertexColor:COLOR;
                float3 viewDir;
            };

            void surf(Input IN, inout SurfaceOutput o)
            {
                float2 uv = IN.uv_MainTex;
                uv.x -= _Time.y * _LaserSpeed;
                fixed4 c = tex2D(_MainTex, uv);               
                float rim = dot(o.Normal, IN.viewDir);
                o.Emission = _GlowColor * pow(1-rim, 1);
                o.Albedo = c.rgb * _Color * IN.vertexColor.rgb;
                o.Alpha = c.a * IN.vertexColor.a;
            }
            ENDCG
        
    }
            Fallback "Diffuse"
}
