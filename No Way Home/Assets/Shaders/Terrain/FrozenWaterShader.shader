Shader "Custom/FrozenWaterShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _IceColor ("Ice Color", Color) = (0.8549, 0.9020, 0.9686, 1)
        _DeepIceColor ("Deep Ice Color", Color) = (0.7, 0.85, 0.95, 1)
        _CracksColor ("Cracks Color", Color) = (0.95, 0.97, 1.0, 1)
        _NoiseScale ("Noise Scale", Float) = 10
        _CracksScale ("Cracks Scale", Float) = 15
        _Glossiness ("Smoothness", Range(0,1)) = 0.9
        _Metallic ("Metallic", Range(0,1)) = 0.0
        _Transparency ("Transparency", Range(0,1)) = 0.7
        _FlowSpeed ("Flow Speed", Float) = 0.5
        _DisplacementStrength ("Displacement", Range(0,1)) = 0.1
    }
    SubShader
    {
        Tags {"Queue"="Transparent" "RenderType"="Transparent"}
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard fullforwardshadows alpha:fade
        #pragma target 3.0

        sampler2D _MainTex;
        fixed4 _IceColor;
        fixed4 _DeepIceColor;
        fixed4 _CracksColor;
        float _NoiseScale;
        float _CracksScale;
        float _Glossiness;
        float _Metallic;
        float _Transparency;
        float _FlowSpeed;
        float _DisplacementStrength;

        struct Input
        {
            float2 uv_MainTex;
            float3 worldPos;
            float3 viewDir;
        };

        // Basic hash function
        float hash(float2 p)
        {
            return frac(sin(dot(p, float2(12.9898, 78.233))) * 43758.5453);
        }

        // 2D hash function
        float2 hash2(float2 p)
        {
            return float2(hash(p), hash(p + float2(123.45, 678.90)));
        }

        // Noise function for natural patterns
        float noise(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            f = f * f * (3.0 - 2.0 * f);
    
            float a = hash(i);
            float b = hash(i + float2(1.0, 0.0));
            float c = hash(i + float2(0.0, 1.0));
            float d = hash(i + float2(1.0, 1.0));
    
            return lerp(lerp(a, b, f.x), lerp(c, d, f.x), f.y);
        }

        // Voronoi noise for crack patterns
        float voronoi(float2 p)
        {
            float2 i = floor(p);
            float2 f = frac(p);
            float minDist = 1.0;
    
            for(int y = -1; y <= 1; y++)
            {
                for(int x = -1; x <= 1; x++)
                {
                    float2 neighbor = float2(x, y);
                    float2 hashValue = hash2(i + neighbor);
                    hashValue = 0.5 + 0.5 * sin(_Time.y * _FlowSpeed + 6.2831 * hashValue);
                    float2 diff = neighbor + hashValue - f;
                    float dist = length(diff);
                    minDist = min(minDist, dist);
                }
            }
            return minDist;
        }

        void surf (Input IN, inout SurfaceOutputStandard o)
        {
            // Base noise for ice texture
            float2 noiseUV = IN.worldPos.xz * _NoiseScale;
            float n = noise(noiseUV);
            n += 0.5 * noise(noiseUV * 2.0);
            n += 0.25 * noise(noiseUV * 4.0);
            n = n / 1.75;

            // Flow animation
            float2 flowUV = IN.worldPos.xz * 0.1 + _Time.y * _FlowSpeed;
            float flow = noise(flowUV);

            // Cracks pattern
            float2 cracksUV = IN.worldPos.xz * _CracksScale;
            float cracks = voronoi(cracksUV + flow * 0.1);
            cracks = smoothstep(0.0, 0.1, cracks);

            // Fresnel effect for edge highlighting
            float fresnel = pow(1.0 - saturate(dot(normalize(IN.viewDir), float3(0, 1, 0))), 5.0);

            // Combine colors
            fixed3 baseColor = lerp(_DeepIceColor.rgb, _IceColor.rgb, n);
            baseColor = lerp(baseColor, _CracksColor.rgb, (1 - cracks) * 0.3);
            
            // Add depth variation
            float depth = noise(noiseUV * 0.5 + flow * 0.05);
            baseColor = lerp(baseColor, _DeepIceColor.rgb, depth * 0.4);
            
            // Apply fresnel effect
            baseColor += fresnel * _IceColor.rgb * 0.2;
            
            // Surface output
            o.Albedo = baseColor;
            o.Metallic = _Metallic;
            o.Smoothness = _Glossiness * (cracks * 0.3 + 0.7);
            
            // Displacement for surface detail
            float displacement = (n * 0.5 + (1 - cracks) * 0.5) * _DisplacementStrength;
            o.Normal = normalize(float3(0, 1, 0) + float3(displacement, 0, displacement));
            
            // Transparency
            float alpha = lerp(_Transparency, 1.0, fresnel * 0.5);
            alpha *= 1.0 - ((1 - cracks) * 0.2);
            o.Alpha = alpha;
        }
        ENDCG
    }
    FallBack "Transparent/VertexLit"
}