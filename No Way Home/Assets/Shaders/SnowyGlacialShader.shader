Shader "Custom/SnowyGlacialShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SnowColor ("Snow Color", Color) = (0.9608, 0.9686, 0.9804, 1) // #f5f7fa
        _IceColor ("Ice Color", Color) = (0.8549, 0.9020, 0.9686, 1) // #dae6f7
        _NoiseScale ("Noise Scale", Float) = 5
        _Seed ("Seed", Float) = 0
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 100

        CGPROGRAM
        #pragma surface surf Lambert alpha

        sampler2D _MainTex;
        fixed4 _SnowColor;
        fixed4 _IceColor;
        float _NoiseScale;
        float _Seed;

        struct Input
        {
            float2 uv_MainTex;
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
            return res * res; // Smooth the noise
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);

            float2 noiseUV = IN.uv_MainTex * _NoiseScale + _Seed;

            float n = noise(noiseUV);
            n += 0.5 * noise(noiseUV * 2.0);
            n += 0.25 * noise(noiseUV * 4.0);
            n /= 1.75; // Normalize between 0 and 1

            fixed3 color = lerp(_IceColor.rgb, _SnowColor.rgb, n);

            o.Albedo = color;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
