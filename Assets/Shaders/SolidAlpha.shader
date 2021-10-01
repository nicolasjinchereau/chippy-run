Shader "Custom/SolidAlpha"
{
	Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _Glow ("Glow", Float) = 0.0
	}

	SubShader
	{
		Tags { "RenderType"="Overlay" "Queue"="Overlay" }
		LOD 100

        CGPROGRAM
        #pragma surface surf Glow nolightmap nofog alpha:blend

        sampler2D _MainTex;
        float4 _Color;
        float _Glow;

        float4 LightingGlow(SurfaceOutput s, half3 lightDir, half atten)
        {
            float4 c;
            c.rgb = s.Albedo + float3(_Glow, _Glow, _Glow);
            c.a = s.Alpha;
            return c;
        }

        struct Input {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutput o) {
            o.Albedo = _Color.rgb;
            o.Alpha = _Color.a;
        }
        ENDCG
	}
}
