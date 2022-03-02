Shader "Custom/LiquidObject"
{
    Properties
    {
        [HDR] _Color("Color", Color) = (1,1,1,1)
        _BumpMap("Bump Map", 2D) = "bump" {}
        _SPColor("Specular Color", Color) = (1,1,1,1)
        _SPPower("Specular Power", Range(50,300)) = 150
        _SPMulti("Specular Multi", Range(1,10)) = 3
        _WaveH("Wave Height", Range(0,0.5)) = 0.1
        _WaveL("Wave Length", Range(5,20)) = 12
        _WaveT("Wave Timeing", Range(0,10)) = 1
        _RimPower("Rim Power", Range(0,15)) = 1
        _RimIntensity("Rim Intensity", Range(0,1)) = 0.5
    }
        SubShader
        {
            Tags { "RenderType" = "Opaque" }
            LOD 200

            CGPROGRAM
            #pragma surface surf WaterSpcular vertex:vert
            //#pragma target 3.0

        sampler2D _BumpMap;
        fixed4 _Color;
        fixed4 _SPColor;
        float _SPPower;
        float _SPMulti;
        float _WaveH;
        float _WaveL;
        float _WaveT;
        float _RimPower;
        float _RimIntensity;

        struct Input
        {
            float2 uv_HeightMap;
            float2 uv_BumpMap;
            float3 worldRefl;
            float3 viewDir;
            float4 screenPos;
            INTERNAL_DATA
        };

        void vert(inout appdata_tan v)
        {
            #if !defined(SHADER_API_OPENGL)
            fixed3 n = tex2Dlod(_BumpMap, v.texcoord);
            float Wave = sin(abs((v.texcoord.x * 2 - 1) * _WaveL) + _Time.y * _WaveT) * _WaveH;
            Wave += sin(abs((v.texcoord.x * 2 - 1) * _WaveL) + _Time.y * _WaveT) * _WaveH;
            v.vertex.y += Wave / 4;
            #endif
        }



        void surf(Input IN, inout SurfaceOutput o)
        {
            float3 Normal1 = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap + _Time.x * 0.1));
            float3 Normal2 = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap - _Time.x * 0.1));

            o.Normal = (Normal1 + Normal2) / 2;

            //refraction
            float3 screenUV = IN.screenPos.rgb / IN.screenPos.a;

            float rim = saturate(dot(o.Normal, IN.viewDir));
            rim = pow(1 - rim, _RimPower);

            o.Albedo = _Color;
            o.Emission = rim * _RimIntensity;
            o.Alpha = 1;
        }

        float4 LightingWaterSpcular(SurfaceOutput s, float3 lightDir, float3 viewDir, float atten)
        {
            float3 H = normalize(lightDir + viewDir);
            float spec = saturate(dot(H,s.Normal));
            spec = pow(spec, _SPPower);

            float4 finalColor;
            finalColor.rgb = spec * _SPColor.rgb * _SPMulti;
            finalColor.a = s.Alpha;

            return finalColor;
        }
        ENDCG
        }
            FallBack "Legacy Shaders/Transparent/Vertexlit"
}
