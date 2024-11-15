Shader "Custom/SnowStormShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _NoiseTex ("Noise Texture", 2D) = "white" {}
        _Intensity ("Intensity", Range(0, 1)) = 0.5
        _Tiling ("Tiling", Float) = 1
        _ScrollOffset ("Scroll Offset", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        Tags { "RenderType"="Transparent" "Queue"="Transparent" }
        LOD 100
        
        ZWrite Off
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
                float2 uvNoise : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };
            
            sampler2D _MainTex;
            sampler2D _NoiseTex;
            float _Intensity;
            float _Tiling;
            float2 _ScrollOffset;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                o.uvNoise = v.uv * _Tiling + _ScrollOffset;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainCol = tex2D(_MainTex, i.uv);
                
                // Sample noise texture twice with different offsets for more variation
                fixed snow1 = tex2D(_NoiseTex, i.uvNoise).r;
                fixed snow2 = tex2D(_NoiseTex, i.uvNoise * 1.4 + float2(0.5, 0.5)).r;
                
                float snowMask = (snow1 * 0.7 + snow2 * 0.3) * _Intensity;
                
                // Create final color
                float3 snowColor = float3(1, 1, 1);
                return lerp(mainCol, fixed4(snowColor, snowMask), snowMask * _Intensity);
            }
            ENDCG
        }
    }
}