Shader "Custom/WallConnection"
{
    Properties
    {
        _MainTex ("Wall Texture", 2D) = "white" {}
        _Color ("Connection Color", Color) = (0.8,0.6,0.4,1)
        _ConnectionBits ("Connection Bits", Float) = 0
        _ConnectionWidth ("Connection Width", Range(0.0, 0.2)) = 0.1
    }
    
    SubShader
    {
        Tags 
        { 
            "RenderType"="Transparent" 
            "Queue"="Transparent-1"
        }
        
        Blend SrcAlpha OneMinusSrcAlpha
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
            };

            struct v2f
            {
                float2 uv : TEXCOORD0;
                float4 vertex : SV_POSITION;
            };

            sampler2D _MainTex;
            float4 _Color;
            float _ConnectionBits;
            float _ConnectionWidth;
            
            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }
            
            fixed4 frag (v2f i) : SV_Target
            {
                fixed4 mainTex = tex2D(_MainTex, i.uv);
                fixed4 col = fixed4(0,0,0,0);
                
                bool shouldFill = false;
                int connections = (int)_ConnectionBits;
                
                if ((connections & 1) && i.uv.x > (1 - _ConnectionWidth)) shouldFill = true;
                if ((connections & 2) && i.uv.x < _ConnectionWidth) shouldFill = true;
                if ((connections & 4) && i.uv.y > (1 - _ConnectionWidth)) shouldFill = true;
                if ((connections & 8) && i.uv.y < _ConnectionWidth) shouldFill = true;
                
                float diagWidth = _ConnectionWidth * 1.4142;
                
                if ((connections & 16) && i.uv.x > (1 - diagWidth) && i.uv.y > (1 - diagWidth))
                    shouldFill = true;
                if ((connections & 32) && i.uv.x < diagWidth && i.uv.y > (1 - diagWidth))
                    shouldFill = true;
                if ((connections & 64) && i.uv.x > (1 - diagWidth) && i.uv.y < diagWidth)
                    shouldFill = true;
                if ((connections & 128) && i.uv.x < diagWidth && i.uv.y < diagWidth)
                    shouldFill = true;
                
                if (shouldFill)
                {
                    col = mainTex * _Color;
                }
                
                return col;
            }
            ENDCG
        }
    }
}