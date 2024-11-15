Shader "Custom/SnowyGlacialShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SnowColor ("Snow Color", Color) = (0.9608, 0.9686, 0.9804, 1)
        _IceColor ("Ice Color", Color) = (0.8549, 0.9020, 0.9686, 1)
        _ShadowColor ("Shadow Color", Color) = (0.282, 0.329, 0.580, 0.7)
        _NoiseScale ("Noise Scale", Float) = 5
        _Seed ("Seed", Float) = 0
        _ShadowDirection ("Shadow Direction", Vector) = (1, 1, 0, 0)
        _ShadowIntensity ("Shadow Intensity", Range(0, 1)) = 0.7
    }
    SubShader
    {
        Tags {"Queue"="Transparent+100" "RenderType"="Transparent"}
        LOD 100
        
        Blend SrcAlpha OneMinusSrcAlpha
        ZWrite Off
        ZTest LEqual
        
        CGPROGRAM
        #pragma surface surf Lambert alpha
        #pragma target 3.0
        
        sampler2D _MainTex;
        fixed4 _SnowColor;
        fixed4 _IceColor;
        fixed4 _ShadowColor;
        float _NoiseScale;
        float _Seed;
        float2 _ShadowDirection;
        float _ShadowIntensity;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
        };

        float rand(float2 n)
        {
            return frac(sin(dot(n, float2(12.9898, 4.1414))) * 43758.5453);
        }

        float noise(float2 p)
        {
            float2 ip = floor(p);
            float2 u = frac(p);
            u = u * u * (3.0 - 2.0 * u);
            float res = lerp(
                lerp(rand(ip), rand(ip + float2(1.0, 0.0)), u.x),
                lerp(rand(ip + float2(0.0, 1.0)), rand(ip + float2(1.0, 1.0)), u.x),
                u.y);
            return res * res;
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
            float2 noiseUV = IN.uv_MainTex * _NoiseScale + _Seed;
            float n = noise(noiseUV);
            n += 0.5 * noise(noiseUV * 2.0);
            n += 0.25 * noise(noiseUV * 4.0);
            n /= 1.75;

            fixed3 baseColor = lerp(_IceColor.rgb, _SnowColor.rgb, n);
            
            float2 shadowPos = IN.worldPos.xy * 0.5;
            shadowPos += _ShadowDirection * _Seed;
            float largeScaleNoise = noise(shadowPos * 0.2);
            float mediumScaleNoise = noise(shadowPos * 0.4 + _Seed);
            float smallScaleNoise = noise(shadowPos * 0.8 - _Seed * 0.5);
            
            float shadowNoise = largeScaleNoise * 0.5 + mediumScaleNoise * 0.3 + smallScaleNoise * 0.2;
            float shadowMask = smoothstep(0.3, 0.7, shadowNoise) * _ShadowIntensity;
            
            if (_ShadowIntensity > 0.05f)
            {
                o.Albedo = lerp(baseColor, _ShadowColor.rgb, shadowMask * _ShadowColor.a);
            }
            else
            {
                o.Albedo = baseColor;
            }
            
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Transparent/Diffuse"
}