Shader "Unlit/PixelSprite"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _BorderColor("Border Color", Color) = (1,1,1,1)
        [MaterialToggle]_Blink("Use Blink?", Float) = 0.0
        _BlinkSpeed("Blink Speed", Float) = 2.0
    }
    SubShader
    {
        Tags { "RenderType"="TransparentCutout" "Queue"="Transparent" }
        LOD 100
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
            float4 _MainTex_TexelSize;
            fixed4 _BorderColor;
            float _Blink;
            float _BlinkSpeed;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = TRANSFORM_TEX(v.uv, _MainTex);
                return o;
            }

            fixed4 frag (v2f i) : SV_Target
            {
                // sample the texture
                fixed4 col = tex2D(_MainTex, i.uv);
                fixed4 up = tex2D(_MainTex, i.uv + float2(0.0,_MainTex_TexelSize.y));
                fixed4 down = tex2D(_MainTex, i.uv - float2(0.0,_MainTex_TexelSize.y));
                fixed4 right = tex2D(_MainTex, i.uv + float2(_MainTex_TexelSize.x, 0.0));
                fixed4 left = tex2D(_MainTex, i.uv - float2(_MainTex_TexelSize.x, 0.0));
                if (col.a < 0.5)
                {
                    float border = up.a + down.a + right.a + left.a;
                    if (border > 0.5)
                    {
                        col = _BorderColor;
                        if (_Blink > 0.5)
                            col.a *= sin(_Time.y * _BlinkSpeed) * 0.5 + 0.5;
                    }
                }
                
                clip(col.a - 0.001);
                return col;
            }
            ENDCG
        }
    }
}
