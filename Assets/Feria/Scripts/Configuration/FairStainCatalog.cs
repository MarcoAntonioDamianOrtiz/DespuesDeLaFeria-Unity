using System;
using UnityEngine;
namespace Feria
{
    [Serializable] public struct FairStainProfile
    {
        public string displayName;public Color color;public float opacity,roughness;public int seed;
        public FairStainProfile(string n,Color c,float a,float r,int s){displayName=n;color=c;opacity=a;roughness=r;seed=s;}
    }
    public sealed class FairStainCatalog:ScriptableObject
    {
        public FairStainProfile[] profiles={
            new FairStainProfile("Refresco de cola",new Color(.19f,.075f,.025f),.82f,.18f,101),
            new FairStainProfile("Refresco de naranja",new Color(.95f,.34f,.035f),.64f,.22f,213),
            new FairStainProfile("Gaseosa de fresa",new Color(.74f,.16f,.24f),.61f,.25f,327),
            new FairStainProfile("Jugo derramado",new Color(.91f,.61f,.08f),.65f,.33f,439),
            new FairStainProfile("Bebida de uva",new Color(.31f,.12f,.40f),.68f,.20f,541),
            new FairStainProfile("Grasa de comida",new Color(.40f,.27f,.075f),.53f,.10f,653),
            new FairStainProfile("Polvo de la feria",new Color(.29f,.25f,.21f),.72f,.95f,769)};
        public FairStainProfile Get(int stableIndex)=>profiles[Mathf.Abs(stableIndex)%profiles.Length];
    }
}
