Shader "Custom/SnowDeform"
{
    Properties {
        _MainTex ("Base Color", 2D) = "white" {}
        _DeformTex ("Deformation Map", 2D) = "black" {}
        _DeformStrength ("Deform Strength", Float) = 0.5
    }

    SubShader {
        Tags { "RenderType"="Opaque" }
        LOD 200

        CGPROGRAM
        #pragma surface surf Standard vertex:vert
        sampler2D _MainTex;
        sampler2D _DeformTex;
        float _DeformStrength;

        struct Input {
            float2 uv_MainTex;
        };

        void vert(inout appdata_full v) {
            float height = tex2Dlod(_DeformTex, float4(v.texcoord.xy, 0, 0)).r;
            v.vertex.y -= height * _DeformStrength;
        }

        void surf(Input IN, inout SurfaceOutputStandard o) {
            o.Albedo = tex2D(_MainTex, IN.uv_MainTex).rgb;
        }
        ENDCG
    }
    FallBack "Diffuse"
}
