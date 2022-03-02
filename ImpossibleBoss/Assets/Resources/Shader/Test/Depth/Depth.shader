Shader "Custom/Depth"
{    
    SubShader
    {
        Tags { "RenderType"="Opaque" }       

        CGPROGRAM       
        #pragma surface surf Standard fullforwardshadows
        #pragma target 3.0

        sampler2D _CameraDepthTexture;

        struct Input
        {
            float4 screenPos;
        };

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            float2 sPos = float2(IN.screenPos.x, IN.screenPos.y) / IN.screenPos.w;
            float4 Depth = tex2D(_CameraDepthTexture, sPos);
            o.Emission = Depth.r;
            o.Alpha = 1;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
