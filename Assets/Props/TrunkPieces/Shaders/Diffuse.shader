
Shader "Custom/Diffuse" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}

     _Opacity ("Opacity", Float) = 1
    [Enum(One, 1, SrcAlpha, 5)] _SrcBlend ("Source Blend", Float) = 1.0
    [Enum(Zero, 0, OneMinusSrcAlpha, 10)] _DstBlend ("Dest Blend", Float) = 0.0
}

SubShader {
	Tags {"Queue"="Geometry" "IgnoreProjector"="True" "RenderType"="Opaque"}
	LOD 200

    Cull Back
    Blend [_SrcBlend] [_DstBlend]

	CGPROGRAM
    #pragma shader_feature OPACITY_ON
    #pragma surface surf Lambert nolightmap nofog keepalpha

	sampler2D _MainTex;
    float _Opacity;

	struct Input {
		float2 uv_MainTex;
	};

	void surf (Input IN, inout SurfaceOutput o)
    {
		float4 c = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = c.rgb;
		o.Alpha = c.a * _Opacity;
	}
	ENDCG
}

Fallback "Legacy Shaders/Diffuse"
}
