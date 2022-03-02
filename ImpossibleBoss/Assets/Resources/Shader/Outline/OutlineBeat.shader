Shader "Custom/OutlineBeat"
{
    
        Properties
        {
            [HDR]_Color("Main Color", Color) = (0.5,0.5,0.5,1)
            _MainTex("Albedo (RGB)", 2D) = "white" {}
            _OutlineColor("Outline Color", Color) = (0,0,0,1)
            _OutlineAlpha("Outline Alpha", Range(0,1)) = 1
            _OutlineWidth("Outline Width", Range(1,5)) = 1
            _InnerlineColor("Innerline Color", Color) = (0,0,0,0)
            
        }

            CGINCLUDE

#include "UnityCG.cginc"
#include "Lighting.cginc"

            
            float4 _OutlineColor;
            float _OutlineAlpha;
            float _OutlineWidth;
            float4 _InnerlineColor;
            float4 _MainTex_ST;
            sampler2D _MainTex;                     

            ENDCG

                Subshader
            {

                Tags { "Queue" = "Transparent"  "RenderType" = "Transparent"}
                LOD 3000
                Zwrite Off
                cull back

                GrabPass{}
                
                //Rendering Outline
                CGPROGRAM
                #pragma surface surf Lambert alpha:fade vertex:vert2
                #pragma target 3.0
                
                struct Input
                {
                    float4 color:COLOR;
                };
                    
                void vert2(inout appdata_base v)
                {
                    v.vertex.xyz *= _OutlineWidth;
                }

                void surf(Input IN, inout SurfaceOutput o) 
                {                  
                    o.Emission = _OutlineColor;
                    o.Alpha = _OutlineAlpha;
                }                
                ENDCG

                //Rendering Innerlines 
                CGPROGRAM
                #pragma surface surf nolight noambient alpha:fade vertex:vert3
                #pragma target 3.0

                sampler2D _GrabTexture;

                struct Input 
                {
                    float4 color:COLOR;
                    float4 screenPos;
                    float2 uv_MainTex;
                };

                void vert3(inout appdata_full v)
                {
                    v.vertex.xyz *= (_OutlineWidth - 0.1);
                }

                void surf(Input IN, inout SurfaceOutput o) 
                {
                    float2 uv_screen = IN.screenPos.rgb / IN.screenPos.a;
                    fixed3 mappingScreenColor = tex2D(_GrabTexture, uv_screen);
                    o.Emission = mappingScreenColor;
                }

                float4 Lightingnolight(SurfaceOutput s, float3 lightDir, float atten) 
                {
                    return float4(0,0,0,1);
                }
                ENDCG

                Pass
                {
                    Blend SrcAlpha OneMinusSrcAlpha

                    CGPROGRAM

                    #pragma vertex vert
                    #pragma fragment frag

                    half4 _Color;
                    //sampler2D _MainTex;
                    //float4 _MainTex_ST;

                    struct vertexInput
                    {
                        float4 vertex: POSITION;
                        float4 texcoord: TEXCOORD0;
                    };

                    struct vertexOutput
                    {
                        float4 pos: SV_POSITION;
                        float4 texcoord: TEXCOORD0;
                    };

                    vertexOutput vert(vertexInput v)
                    {
                        vertexOutput o;
                        o.pos = UnityObjectToClipPos(v.vertex);
                        o.texcoord.xy = (v.texcoord.xy * _MainTex_ST.xy + _MainTex_ST.zw);
                        return o;
                    }

                    half4 frag(vertexOutput i) : COLOR
                    {
                        return tex2D(_MainTex, i.texcoord) * _Color;
                    }

                    ENDCG
                }
            }
}
