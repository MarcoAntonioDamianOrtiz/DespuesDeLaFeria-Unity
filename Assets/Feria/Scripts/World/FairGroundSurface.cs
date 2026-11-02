using UnityEngine;
namespace Feria
{
    public enum WalkSurface { Stone,Wood,Earth,Grass }
    /// <summary>Static surface metadata. No per-frame work.</summary>
    public sealed class FairGroundSurface : MonoBehaviour { public WalkSurface surface; }
}
