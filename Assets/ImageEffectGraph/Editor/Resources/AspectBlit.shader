Shader "Hidden/AspectBlit"
{
    Properties
    {
        _MainTex ("Texture", 2D) = "white" {}
        _AspectRatio ("Aspect Ratio", float) = 1
        _BackgroundColor ("Background Color", Color) = (0, 0, 0, 0)
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
            
            sampler2D _MainTex;
            float _AspectRatio;
            fixed4 _BackgroundColor;

            v2f vert (appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                
                float aspectXScale = max(1/_AspectRatio, 1);
                float aspectYScale = max(_AspectRatio, 1);
                
                float aspectXOffset = -(aspectXScale-1) / 2;
                float aspectYOffset = -(aspectYScale-1) / 2;
                
                o.uv = float2(v.uv.x * aspectXScale + aspectXOffset, v.uv.y * aspectYScale + aspectYOffset);
                return o;
            }


            fixed4 frag (v2f i) : SV_Target
            {
                if(i.uv.x < 0 || i.uv.x > 1 || i.uv.y < 0 || i.uv.y > 1)
                    return _BackgroundColor;
                return tex2D(_MainTex, i.uv);
            }
            ENDCG
        }
    }
}
