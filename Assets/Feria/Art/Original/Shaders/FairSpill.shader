Shader "Feria/Spill"
{
 Properties { _BaseColor("Color",Color)=(.3,.1,.02,.7) _Roughness("Rugosidad",Range(0,1))=.2 _Clean("Limpieza",Range(0,1))=0 _Seed("Variacion",Float)=1 }
 SubShader
 {
  Tags {"RenderPipeline"="UniversalPipeline" "Queue"="Transparent" "RenderType"="Transparent"}
  Pass
  {
   Tags {"LightMode"="UniversalForward"}
   Blend SrcAlpha OneMinusSrcAlpha
   ZWrite Off
   Cull Off
   HLSLPROGRAM
   #pragma vertex Vert
   #pragma fragment Frag
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
   #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Lighting.hlsl"
   CBUFFER_START(UnityPerMaterial)
   half4 _BaseColor;float _Roughness,_Clean,_Seed;
   CBUFFER_END
   struct A {float4 p:POSITION;float3 n:NORMAL;float2 uv:TEXCOORD0;};
   struct V {float4 p:SV_POSITION;float3 world:TEXCOORD0;float3 normal:TEXCOORD1;float2 uv:TEXCOORD2;};
   V Vert(A i){V o;o.p=TransformObjectToHClip(i.p.xyz);o.world=TransformObjectToWorld(i.p.xyz);o.normal=TransformObjectToWorldNormal(i.n);o.uv=i.uv;return o;}
   half4 Frag(V i):SV_Target
   {
    Light l=GetMainLight();float3 n=normalize(i.normal);float diffuse=saturate(dot(n,l.direction));
    float noise=sin(i.uv.x*25+_Seed)*sin(i.uv.y*19+_Seed*.7);
    float spec=pow(saturate(dot(n,normalize(l.direction+GetWorldSpaceNormalizeViewDir(i.world)))),lerp(96,8,_Roughness))*(1-_Roughness)*.28;
    float rim=smoothstep(1,.72,length(i.uv*2-1));
    return half4(_BaseColor.rgb*(.72+diffuse*.28+noise*.045)+spec*l.color,_BaseColor.a*(1-_Clean)*lerp(.60,1,rim));
   }
   ENDHLSL
  }
 }
}
