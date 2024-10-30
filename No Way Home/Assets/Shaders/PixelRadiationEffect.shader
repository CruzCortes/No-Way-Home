Shader "Custom/PixelRadiationEffect"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _RadiationColor ("Radiation Color", Color) = (0,0,0,1)
        _PulseSpeed ("Pulse Speed", Range(0.1, 5.0)) = 1.0
        _RadiationIntensity ("Radiation Intensity", Range(0.0, 1.0)) = 0.5
        _PixelSize ("Pixel Size", Range(1, 16)) = 2
    }
    
    SubShader
    {
        Tags { "Queue"="Transparent" "RenderType"="Transparent" }
        Blend SrcAlpha OneMinusSrcAlpha
        
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            struct appdata
            {
                float4 vertex : POSITION;
                float2 uv : TEXCOORD0;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _RadiationColor;
            float _PulseSpeed;
            float _RadiationIntensity;
            float _PixelSize;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // Pixelate UV coordinates
                float2 pixelatedUV = floor(i.uv * _PixelSize) / _PixelSize;
                
                // Sample original texture
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Create pulsing effect
                float pulse = (sin(_Time.y * _PulseSpeed) + 1.0) * 0.5;
                
                // Generate radiation effect
                float dist = length(i.uv - 0.5) * 2.0;
                float noise = random(pixelatedUV + _Time.y);
                float radiation = smoothstep(0.8 - pulse * 0.3, 1.2, noise) * 
                                smoothstep(1.0, 0.0, dist) * 
                                _RadiationIntensity;
                
                // Apply radiation only to non-transparent pixels
                float alpha = col.a;
                if (alpha > 0.1) {
                    col.rgb = lerp(col.rgb, _RadiationColor.rgb, radiation);
                }
                
                return col;
            }
            ENDCG
        }
    }
}