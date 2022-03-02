Shader "Custom/Water"
{
    Properties
    {
        [HDR] _Color ("Color", Color) = (1,1,1,1)
        _HeightMap ("Height Map", 2D) = "white" {}
        _BumpMap ("Bump Map", 2D) = "bump" {}
        _Cube ("Cube", Cube) = ""{}
        _LerpDrain("Drain", Range(0,1)) = 0
        _SPColor("Specular Color", Color) = (1,1,1,1)
        _SPPower("Specular Power", Range(50,300)) = 150
        _SPMulti("Specular Multi", Range(1,10)) = 3
        _WaveH ("Wave Height", Range(0,0.5)) = 0.1
        _WaveL ("Wave Length", Range(5,20)) = 12
        _WaveT ("Wave Timeing", Range(0,10)) = 1
        _Refract ("Refract Strength", Range(0,0.2)) = 0.1
    }
        SubShader
        {
            //Tags { "RenderType" = "Transparent" "Queue" = "Transparent"}
            Tags { "RenderType" = "Opaque" }
            LOD 200

            GrabPass{}

            CGPROGRAM
            #pragma surface surf WaterSpcular vertex:vert
            //#pragma target 3.0

        samplerCUBE _Cube;
        sampler2D _HeightMap;
        sampler2D _BumpMap;
        sampler2D _GrabTexture;
        fixed4 _Color;
        fixed4 _SPColor;
        float _SPPower;
        float _SPMulti;
        float _WaveH;
        float _WaveL;
        float _WaveT;
        float _Refract;
        float _LerpDrain;

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
            fixed4 c = tex2Dlod(_HeightMap, v.texcoord);
            fixed3 n = tex2Dlod(_BumpMap, v.texcoord);
            float Wave = sin(abs((v.texcoord.x * 2 - 1) * _WaveL) + _Time.y * _WaveT) * _WaveH;
            Wave += sin(abs((v.texcoord.x * 2 - 1) * _WaveL) + _Time.y * _WaveT) * _WaveH;
            v.vertex.y += lerp(c.g + Wave / 2, pow(c.r, 1.5) * 5, _LerpDrain);
            #endif
        }

        

        void surf (Input IN, inout SurfaceOutput o)
        {
            float3 Normal1 = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap + _Time.x * 0.1));
            float3 Normal2 = UnpackNormal(tex2D(_BumpMap, IN.uv_BumpMap - _Time.x * 0.1));

            o.Normal = (Normal1 + Normal2)/2;

            float3 refcolor = texCUBE(_Cube, WorldReflectionVector(IN,o.Normal));

            //refraction
            float3 screenUV = IN.screenPos.rgb / IN.screenPos.a;
            float3 refraction = tex2D(_GrabTexture, (screenUV.xy + o.Normal.xy * _Refract));

            float rim = saturate(dot(o.Normal, IN.viewDir));
            rim = pow(1-rim, 1.5);

            o.Albedo = _Color;
            o.Emission = (refcolor * rim * refraction) * 0.5;
            o.Alpha = saturate(rim /*+ 0.5 */+ _LerpDrain);
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
