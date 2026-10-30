Shader "Feria/Amanecer artesanal"
{
    Properties
    {
        _Zenith("Cielo",Color)=(.32,.57,.69,1)
        _Horizon("Horizonte",Color)=(1,.77,.59,1)
        _Ground("Suelo lejano",Color)=(.64,.64,.59,1)
        _SunDirection("Dirección del sol",Vector)=(.5,.2,.7,0)
    }
    SubShader
    {
        Tags {"Queue"="Background" "RenderType"="Background" "PreviewType"="Skybox"}
        Cull Off ZWrite Off
        Pass
        {
            CGPROGRAM
            #pragma vertex vert
            #pragma fragment frag
            #include "UnityCG.cginc"
            struct appdata {float4 vertex:POSITION;};
            struct v2f {float4 vertex:SV_POSITION;float3 direction:TEXCOORD0;};
            float4 _Zenith,_Horizon,_Ground,_SunDirection;
            v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.direction=mul((float3x3)unity_ObjectToWorld,v.vertex.xyz);return o;}
            fixed4 frag(v2f i):SV_Target
            {
                float3 d=normalize(i.direction);
                float h=saturate(d.y*3.2);
                float3 sky=lerp(_Horizon.rgb,_Zenith.rgb,pow(h,.65));
                sky=lerp(_Ground.rgb,sky,smoothstep(-.12,.04,d.y));
                float sun=dot(d,normalize(_SunDirection.xyz));
                sky+=float3(1,.57,.22)*pow(saturate(sun),22)*.13;
                sky=lerp(sky,float3(1,.93,.72),smoothstep(.9981,.9987,sun));
                return fixed4(sky,1);
            }
            ENDCG
        }
    }
}
