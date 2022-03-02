Shader "Custom/Hologram"
{
    Properties
    {
        _Color("Main Color", Color) = (0.5,0.5,0.5,1)
        _BumpMap ("NormalMap", 2D) = "bump" {}
        _RimPower("Rim Power", Range(1, 10)) = 3
        _NoiseInterval("Noise Interval", Range(1, 10)) = 3
        _NoiseThickness("Noise Thickness", Range(1, 10)) = 3
        _BlinkInterval("Blink Interval", Range(1, 10)) = 3
        _NoiseTransparency("Noise Transparency", Range(0, 1)) = 0.5
    }
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent"}

        CGPROGRAM
        #pragma surface surf nolight noambient alpha:fade


        sampler2D _BumpMap;
        float4 _Color;
        float _RimPower;
        float _NoiseInterval;
        float _NoiseThickness;
        float _BlinkInterval;
        float _NoiseTransparency;


        struct Input
        {
            float2 uv_BumpMap;
            float3 viewDir;
            float3 worldPos;
        };

        void surf (Input IN, inout SurfaceOutput o)
        {
            o.Normal = UnpackNormal(tex2D (_BumpMap, IN.uv_BumpMap));
            o.Emission = _Color; 
            float rim = saturate(dot(o.Normal, IN.viewDir));
            rim = saturate(pow(1 - rim, _RimPower) + pow(frac(IN.worldPos.g * _NoiseInterval - _Time.y), _NoiseThickness) * _NoiseTransparency);
            
            o.Alpha = rim * abs(sin(_Time.y * _BlinkInterval));
        }

        float4 Lightingnolight(SurfaceOutput s, float3 lightDir, float atten) 
        {
            return float4(_Color.rgb, s.Alpha);
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}
