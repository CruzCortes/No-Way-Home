Shader "Custom/PixelFireShader"
{
    Properties
    {
        _MainTex ("Sprite Texture", 2D) = "white" {}
        _GlowColor ("Glow Color", Color) = (1, 0.5, 0, 1)
        _GlowIntensity ("Glow Intensity", Range(0, 3)) = 1.5
        _PixelSize ("Pixel Size", Range(1, 32)) = 1
        _FlickerSpeed ("Flicker Speed", Range(0, 10)) = 2
        _FlickerIntensity ("Flicker Intensity", Range(0, 1)) = 0.2
        _GlowRadius ("Glow Radius", Range(1, 10)) = 2
    }

    SubShader
    {
        Tags
        {
            "Queue"="Transparent"
            "RenderType"="Transparent"
            "PreviewType"="Plane"
        }

        Blend SrcAlpha OneMinusSrcAlpha
        Cull Off
        ZWrite Off

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
                fixed4 color : COLOR;
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
                fixed4 color : COLOR;
            };

            sampler2D _MainTex;
            float4 _MainTex_ST;
            float4 _MainTex_TexelSize;
            fixed4 _GlowColor;
            float _GlowIntensity;
            float _PixelSize;
            float _FlickerSpeed;
            float _FlickerIntensity;
            float _GlowRadius;

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                o.color = v.color;
                return o;
            }

            float random(float2 st)
            {
                return frac(sin(dot(st.xy, float2(12.9898,78.233))) * 43758.5453123);
            }

            // Sample texture with pixel art preservation
            fixed4 samplePixelated(float2 uv)
            {
                float2 pixelatedUV = floor(uv * _PixelSize) / _PixelSize;
                return tex2D(_MainTex, pixelatedUV);
            }

            // Calculate glow based on neighboring pixels
            float calculateGlow(float2 uv)
            {
                float glow = 0;
                float totalWeight = 0;
                
                // Sample in a circular pattern
                for(float i = -_GlowRadius; i <= _GlowRadius; i++)
                {
                    for(float j = -_GlowRadius; j <= _GlowRadius; j++)
                    {
                        float2 offset = float2(i, j) * _MainTex_TexelSize.xy;
                        float weight = 1.0 - length(offset) / (_GlowRadius * _MainTex_TexelSize.xy);
                        
                        if(weight > 0)
                        {
                            float2 sampleUV = uv + offset;
                            fixed4 sample = samplePixelated(sampleUV);
                            glow += sample.a * weight;
                            totalWeight += weight;
                        }
                    }
                }
                
                return glow / totalWeight;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                // Get base color with pixelation
                fixed4 col = samplePixelated(i.uv);
                
                // Calculate flicker effect
                float flicker = random(float2(_Time.y * _FlickerSpeed, i.uv.y));
                flicker = lerp(1, flicker, _FlickerIntensity);
                
                // Calculate glow
                float glowMask = calculateGlow(i.uv);
                fixed4 glowEffect = _GlowColor * glowMask * _GlowIntensity * flicker;
                
                // Blend original color with glow
                fixed4 finalColor = col + glowEffect * (1 - col.a);
                finalColor.rgb *= i.color.rgb;
                
                // Ensure alpha doesn't go below original sprite's alpha
                finalColor.a = max(col.a, glowEffect.a * 0.5);
                
                return finalColor;
            }
            ENDCG
        }
    }
}