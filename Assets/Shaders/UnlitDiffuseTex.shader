Shader "Custom/UnlitDiffuseTex"
{
	Properties {
        _Color ("Color", Color) = (1, 1, 1, 1)
        _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	}

	SubShader
	{
		Tags { "RenderType"="Overlay" "Queue"="Overlay" }
		LOD 100
        ZTest Always
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha

        CGPROGRAM
        #pragma surface surf Unlit nolightmap nofog keepalpha

        float4 _Color;
        sampler2D _MainTex;

        float4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten) {
            float4 c;
            c.rgb = s.Albedo;
            c.a = s.Alpha;
            return c;
        }

        struct Input {
            float2 uv_MainTex;
        };

        void surf(Input IN, inout SurfaceOutput o)
        {
            float4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
            o.Albedo = c.rgb;
            o.Alpha = c.a;
        }
        ENDCG
	}
}
