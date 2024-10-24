Shader "Custom/DayNightShader"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _TimeOfDay ("Time of Day", Range(0,1)) = 0.5
        _NightColor ("Night Color", Color) = (0.1,0.1,0.2,0.5)
        _NightIntensity ("Night Intensity", Range(0,5)) = 1
    }
    SubShader
    {
        // No culling or depth
        Cull Off ZWrite Off ZTest Always

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

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            sampler2D _MainTex;
            float _TimeOfDay;
            float4 _NightColor;
            float _NightIntensity;

            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 col = tex2D(_MainTex, i.uv);
                
                // Simplified night calculation
                float nightStrength = 0;
                if (_TimeOfDay > 0.75 || _TimeOfDay < 0.25)
                {
                    nightStrength = _TimeOfDay > 0.75 ? 
                        (_TimeOfDay - 0.75) * 4 : // Evening
                        (0.25 - _TimeOfDay) * 4;  // Night
                }
                
                // Apply night overlay
                return col * (1 - nightStrength * _NightIntensity * _NightColor.a) + 
                       _NightColor * nightStrength * _NightIntensity;
            }
            ENDCG
        }
    }
}