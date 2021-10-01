Shader "Custom/Unlit-Overlay" {
Properties {
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}

SubShader {
    Tags {"Queue"="Overlay" "IgnoreProjector"="True" "RenderType"="Overlay"}
    LOD 100

    ZTest Always
    ZWrite Off
    Blend One Zero

    Pass
    {
        CGPROGRAM
        #pragma vertex vert
        #pragma fragment frag
        #pragma target 2.0
        
        #include "UnityCG.cginc"

        struct appdata_t {
            float4 vertex : POSITION;
            float2 texcoord : TEXCOORD0;
            float3 normal : NORMAL;
        };

        struct v2f {
            float4 vertex : SV_POSITION;
            float2 texcoord : TEXCOORD0;
            float3 normal : TEXCOORD1;
            float3 worldPos : TEXCOORD2;
        };

        sampler2D _MainTex;
        float4 _MainTex_ST;
        
        v2f vert (appdata_t v)
        {
            v2f o;
            o.vertex = UnityObjectToClipPos(v.vertex);
            o.texcoord = TRANSFORM_TEX(v.texcoord, _MainTex);
            o.normal = UnityObjectToWorldNormal(v.normal);
            o.worldPos = mul(unity_ObjectToWorld, float4(v.vertex.xyz, 1)).xyz;
            return o;
        }
        
        float4 frag (v2f i) : SV_Target
        {
            float3 view = normalize(_WorldSpaceCameraPos - i.worldPos);
            float4 c = tex2D(_MainTex, i.texcoord);
            float diff = saturate(dot(i.normal, view));
            return lerp(float4(c.rgb * diff, c.a), c, 0.3);
        }
        ENDCG
    }
}

}
