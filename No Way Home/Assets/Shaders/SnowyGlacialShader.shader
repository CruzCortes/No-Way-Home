Shader "Custom/SnowyGlacialShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SnowColor ("Snow Color", Color) = (0.95, 0.97, 1, 1)
        _IceColor ("Ice Color", Color) = (0.8, 0.9, 1, 1)
        _NoiseScale ("Noise Scale", Float) = 1
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

        float2 random2(float2 st, float seed)
        {
            st += seed;
            return frac(sin(float2(dot(st,float2(127.1,311.7)),dot(st,float2(269.5,183.3))))*43758.5453);
        }

        float noise(float2 st, float seed)
        {
            float2 i = floor(st);
            float2 f = frac(st);
    
            float2 u = f*f*(3.0-2.0*f);

            return lerp(lerp(dot(random2(i + float2(0.0,0.0), seed), f - float2(0.0,0.0)), 
                             dot(random2(i + float2(1.0,0.0), seed), f - float2(1.0,0.0)), u.x),
                        lerp(dot(random2(i + float2(0.0,1.0), seed), f - float2(0.0,1.0)), 
                             dot(random2(i + float2(1.0,1.0), seed), f - float2(1.0,1.0)), u.x), u.y);
        }

        void surf (Input IN, inout SurfaceOutput o)
        {
            fixed4 c = tex2D (_MainTex, IN.uv_MainTex);
            
            float2 noiseUV = IN.uv_MainTex * _NoiseScale;
            float noiseValue = noise(noiseUV, _Seed);
            
            float organicNoise = noise(noiseUV * 2, _Seed + 1000) * 0.5 + 0.5;
            
            float combinedNoise = (noiseValue * 0.7 + organicNoise * 0.3);
            
            fixed3 snowColor = lerp(_IceColor.rgb, _SnowColor.rgb, pow(combinedNoise, 0.5));
            
            snowColor = lerp(snowColor, fixed3(1,1,1), 0.2);
            
            o.Albedo = snowColor;
            o.Alpha = c.a;
        }
        ENDCG
    }
    FallBack "Diffuse"
}