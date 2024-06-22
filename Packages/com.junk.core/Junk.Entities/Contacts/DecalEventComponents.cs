using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;

namespace Junk.Entities
{
    /// <summary>
    /// Determines the resulting decal from a collision.
    /// </summary>
    /// todo rename ContactImpactType
    public enum ImpactType
    {
        Normal, // handgun caliber
        Large // large caliber ie revolver
    }
}