Shader "Custom/MyFogShader" {
Properties {
    _Color ("Main Color", Color) = (0,0,0,1)
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}
 
SubShader {
    Tags {"Queue"="Transparent" "IgnoreProjector"="True" "RenderType"="Transparent"}
    LOD 200
 
    Stencil
{
Ref 1
Comp Greater
Pass IncrSat
}
 
CGPROGRAM
#pragma surface surf Lambert alpha
 
sampler2D _MainTex;
fixed4 _Color;
 
struct Input {
    float2 uv_MainTex;
};
 
void surf (Input IN, inout SurfaceOutput o) {
    fixed4 c = tex2D(_MainTex, IN.uv_MainTex) * _Color;
    o.Albedo = c.rgb;
    o.Alpha = c.a;
}
ENDCG
}
 
Fallback "Transparent/VertexLit"
}