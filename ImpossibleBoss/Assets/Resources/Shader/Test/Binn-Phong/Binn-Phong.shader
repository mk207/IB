Shader "Custom/Binn-Phong"
{
    Properties
    {       
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _BumpMap ("Bump Map", 2D) = "bump" {}
        _SpecCol ("Specular Color", Color) = (1,1,1,1)
        _SpecPow ("Specular Power", Range(10, 200)) = 100
        _FakeSpecCol("Fake Specular Color", Color) = (1,1,1,1)
        _FakeSpecPow("Fake Specular Power", Range(10, 200)) = 100
        _RimCol("Rim Color", Color) = (1,1,1,1)
        _RimPow("Rim Power", Range(1, 10)) = 5
        _GlossTex ("Gloss Tex", 2D) = "white" {}
    } 
    SubShader
    {
        Tags { "RenderType"="Opaque" }
       

        CGPROGRAM       
        #pragma surface surf Test

        sampler2D _MainTex;
        sampler2D _BumpMap;
        sampler2D _GlossTex;
        float4 _SpecCol;
        float _SpecPow;
        float4 _FakeSpecCol;
        float _FakeSpecPow;
        float4 _RimCol;
        float _RimPow;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_BumpMap;
            float2 uv_GlossTex;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            float4 m = tex2D(_GlossTex, IN.uv_GlossTex);
            o.Albedo = c.rgb;
            o.Normal = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap));
            o.Gloss = m.a;
            o.Alpha = c.a;
        }

        float4 LightingTest(SurfaceOutput s, float3 lightDir, float3 viewDir, float atten) 
        {
            float3 SpecColor;
            float3 DiffColor;
            //Lambert
            float ndotl = saturate(dot(s.Normal, lightDir));
            DiffColor = ndotl * s.Albedo * _LightColor0.rgb * atten;

            //Specular
            float3 H = normalize(lightDir + viewDir);
            float spec = saturate(dot(H, s.Normal));
            spec = pow(spec, _SpecPow);
            SpecColor = spec * _SpecCol.rgb * s.Gloss;

            //Rim
            float3 rimColor;
            float rim = abs(dot(viewDir, s.Normal));
            float invrim = 1 - rim;
            rimColor = pow(invrim, _RimPow) * _RimCol.rgb;

            //fake Spec
            float3 SpecColor2;
            SpecColor2 = pow(rim, _FakeSpecPow) * _FakeSpecCol * s.Gloss;

            //Combining
            float4 final;
            final.rgb = DiffColor.rgb + SpecColor.rgb + rimColor.rgb + SpecColor2.rgb;
            final.a = s.Alpha;
            return final;            
        }
        ENDCG
    }
    FallBack "Diffuse"
}
