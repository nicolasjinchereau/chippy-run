
Shader "Custom/Leaves1X" {
Properties {
	_MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
	_Cutoff ("Alpha cutoff", Range(0, 1)) = 0.5

    _Opacity ("Opacity", Float) = 1
    [Enum(One, 1, SrcAlpha, 5)] _SrcBlend ("Source Blend", Float) = 1.0
    [Enum(Zero, 0, OneMinusSrcAlpha, 10)] _DstBlend ("Dest Blend", Float) = 0.0
}

SubShader {
	Tags {"Queue"="AlphaTest" "IgnoreProjector"="True" "RenderType"="TransparentCutout"}
	LOD 200

    Cull Back
    Blend [_SrcBlend] [_DstBlend]

	CGPROGRAM
	#pragma surface surf Lambert alphatest:_Cutoff nolightmap nofog keepalpha

	sampler2D _MainTex;
    float _Opacity;

	struct Input {
		float2 uv_MainTex;
	};

	void surf (Input IN, inout SurfaceOutput o) {
		fixed4 c = tex2D(_MainTex, IN.uv_MainTex);
		o.Albedo = c.rgb;
		o.Alpha = c.a * _Opacity;
	}
	ENDCG
}

Fallback "Legacy Shaders/Transparent/Cutout/VertexLit"
}
