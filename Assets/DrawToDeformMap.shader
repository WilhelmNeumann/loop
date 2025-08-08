Shader "Custom/SnowBrush"
{
    Properties
    {
        _MainTex ("Base (unused)", 2D) = "white" {}
        _Coords ("Coords", Vector) = (0.5, 0.5, 0.1, 0)
        _Strength ("Strength", Float) = 0.5
    }

    SubShader
    {
        Pass
        {
            ZTest Always Cull Off ZWrite Off
            Blend OneMinusDstColor One // additive darkening

            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"

            sampler2D _MainTex;
            float4 _Coords;
            float _Strength;

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

            v2f vert(appdata v)
            {
                v2f o;
                o.vertex = UnityObjectToClipPos(v.vertex);
                o.uv = v.uv;
                return o;
            }

            fixed4 frag(v2f i) : SV_Target
            {
                float2 diff = i.uv - _Coords.xy;
                float dist = length(diff);
                float mask = smoothstep(_Coords.z, 0.0, dist);
                return fixed4(mask * _Strength, 0, 0, 1);
            }
            ENDCG
        }
    }
}