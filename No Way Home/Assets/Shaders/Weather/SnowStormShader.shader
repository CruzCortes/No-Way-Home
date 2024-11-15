Shader "Custom/SnowStormShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _SnowTexture ("Snow Texture", 2D) = "white" {}
        _NoiseMask ("Noise Mask", 2D) = "white" {}
        _SnowIntensity ("Snow Intensity", Range(0, 1)) = 0.5
        _Speed ("Speed", Float) = 1.0
        _OffsetDirection ("Offset Direction", Vector) = (1, 1, 0, 0)
        _ScrollOffset ("Scroll Offset", Vector) = (0, 0, 0, 0)
    }
    
    SubShader
    {
        // No culling or depth
        Cull Off 
        ZWrite Off 
        ZTest Always

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
                float2 snowUV : TEXCOORD1;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            sampler2D _SnowTexture;
            sampler2D _NoiseMask;
            float4 _MainTex_ST;
            float _SnowIntensity;
            float _Speed;
            float4 _OffsetDirection;
            float4 _ScrollOffset;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = UnityStereoScreenSpaceUVAdjust(v.uv, _MainTex_ST);
                
                // Calculate snow movement
                float2 windOffset = _OffsetDirection.xy * _Speed * _Time.y;
                o.snowUV = v.uv + _ScrollOffset.xy + windOffset;
                
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Sample snow with offset
                fixed4 snow = tex2D(_SnowTexture, i.snowUV);
                fixed4 noise = tex2D(_NoiseMask, i.snowUV * 0.5);
                
                // Calculate snow mask
                float snowMask = snow.r * noise.r * _SnowIntensity;
                
                // Simple blend without grab pass
                fixed4 finalColor = col;
                finalColor.rgb = lerp(col.rgb, fixed3(1,1,1), snowMask);
                
                return finalColor;
            }
            ENDCG
        }
    }
}