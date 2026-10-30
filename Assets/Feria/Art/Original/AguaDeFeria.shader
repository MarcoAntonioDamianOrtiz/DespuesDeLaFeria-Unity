Shader "Feria/Agua pintada"
{
    Properties { _BaseColor("Turquesa", Color)=(.16,.56,.60,1) _Shallow("Brillo", Color)=(.42,.76,.73,1) }
    SubShader
    {
        Tags { "RenderPipeline"="UniversalPipeline" "RenderType"="Opaque" }
        Pass
        {
            HLSLPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
            struct Attributes {float4 positionOS:POSITION;};
            struct Varyings {float4 positionCS:SV_POSITION;float3 world:TEXCOORD0;};
            CBUFFER_START(UnityPerMaterial)
            half4 _BaseColor,_Shallow;
            CBUFFER_END
            Varyings vert(Attributes i){Varyings o;o.world=TransformObjectToWorld(i.positionOS.xyz);o.positionCS=TransformWorldToHClip(o.world);return o;}
            half4 frag(Varyings i):SV_Target
            {
                float wave=sin(i.world.x*14+i.world.z*20+_Time.y*.7)*.5+.5;
                float ribbon=smoothstep(.83,.96,wave)*.28;
                float calm=.10+.10*sin(i.world.x*3-i.world.z*4+_Time.y*.4);
                return half4(lerp(_BaseColor.rgb,_Shallow.rgb,ribbon+calm),1);
            }
            ENDHLSL
        }
    }
}
