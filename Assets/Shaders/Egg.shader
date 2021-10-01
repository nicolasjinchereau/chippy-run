
Shader "Custom/Egg" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
    _ExtrusionColor ("Extrusion Color", Color) = (0, 0, 0, 0)
    _Extrusion ("Normal Extrusion", Range(0.1, 10.0)) = 2.0
}

SubShader {
	Tags {"Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque" }
	LOD 200

    Cull Front
    ZTest Less
    ZWrite On
    Blend SrcAlpha OneMinusSrcAlpha

    CGPROGRAM
    #pragma surface surf Unlit nolightmap nofog vertex:vert keepalpha

    struct Input {
        float2 uv_MainTex;
    };

    float _Extrusion;
    float4 _ExtrusionColor;

    float4 LightingUnlit(SurfaceOutput s, half3 lightDir, half atten)
    {
        float4 c;
        c.rgb = s.Albedo;
        c.a = s.Alpha;
        return c;
    }

    void vert (inout appdata_base v) {
        v.vertex.xyz += v.normal * _Extrusion;
    }

    void surf(Input IN, inout SurfaceOutput o)
    {
        o.Albedo = _ExtrusionColor.rgb;
        o.Alpha = _ExtrusionColor.a;
    }
    ENDCG

    Cull Back
    ZTest Less
    ZWrite On
    Blend One Zero

	CGPROGRAM
    #pragma surface surf Lambert nolightmap nofog keepalpha

	sampler2D _MainTex;

	struct Input {
		float2 uv_MainTex;
	};

	void surf (Input IN, inout SurfaceOutput o)
    {
		float4 c = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = c.rgb;
		o.Alpha = c.a;
	}
	ENDCG
}

Fallback "Legacy Shaders/Diffuse"
}
