Shader "Custom/Tornado"
{
    Properties
    {
        [HDR]_Color ("Color", Color) = (1,1,1,1)
        _MainTex ("Albedo (RGB)", 2D) = "white" {}
        _Noise ("Noise", 2D) = "bumb"{}
        _RotationSpeed("Rot Speed", Range (-100, 100)) = 1
        _Cutoff("Cutoff", Range(0,1)) = 0
        _RimPower("Rim Power", Range(0, 10)) = 1
        _WaveSpeed("Wave Speed", Range(0,1)) = 1
    }
    SubShader
    {
        Cull Off
        Tags { "RenderType"="Transparent" "Queue" = "Transparent"}
        LOD 200

        CGPROGRAM
        #pragma surface surf Lambert vertex:vert alphatest:_Cutoff
        #pragma target 3.0
            
        sampler2D _MainTex;
        sampler2D _Noise;

        struct Input
        {
            float2 uv_MainTex;
            float2 uv_Noise;
            float3 viewDir;
        };
        
        fixed4 _Color;
        float _RotationSpeed;
        float _RimPower;
        float _WaveSpeed;

        void vert(inout appdata_full v)
        {
            //v.vertex.y += (_Time.y * _WaveSpeed * 6) ;

            /*float sinX = sin(_RotationSpeed * _Time);
            float cosX = cos(_RotationSpeed * _Time);
            float sinY = sin(_RotationSpeed * _Time);
            float2x2 rotationMatrix = float2x2(cosX, -sinX, sinY, cosX);
            v.texcoord.xy = mul(v.texcoord.xy, rotationMatrix);*/
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            //fixed4 c = tex2D (_MainTex, IN.uv_MainTex) * _Color;
            /*float2 uv = IN.uv_Noise;
            uv += _Time.y;*/
            float rim = saturate(dot(o.Normal, IN.viewDir));
            rim = pow(1 - rim, _RimPower);
            fixed4 n = tex2D(_Noise, IN.uv_Noise + _Time.y) * _Color;
            o.Albedo = n.rgb;
            o.Emission = rim;
            o.Alpha = n.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
