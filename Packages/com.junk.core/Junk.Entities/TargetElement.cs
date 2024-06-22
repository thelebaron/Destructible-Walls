using System;
using Unity.Entities;
using Unity.Mathematics;

namespace Junk.Entities
{
    // This describes the number of buffer elements that should be reserved
    // in chunk data for each instance of a buffer. In this case, 8 integers
    // will be reserved (32 bytes) along with the size of the buffer header
    // (currently 16 bytes on 64-bit targets)
    /// <summary>
    /// Stores a list of possible targets
    /// </summary>
    /*[InternalBufferCapacity(0)]
    public struct TargetElement : IBufferElementData, IEnableableComponent
    {
        public TargetInfo Value;

        // These implicit conversions are optional, but can help reduce typing.
        public static implicit operator Entity(TargetElement e)
        {
            return e.Value.Entity;
        }
    }*/

    /// <summary>
    /// Target entity data
    /// </summary>
    public struct TargetInfo : IEquatable<Entity>, IComparable<Entity>
    {
        public Entity Entity;
        public float3 Position;
        public float  Distance;
        public int    LastSeen;
        public bool   IdealAngle;
        public int    HiddenTime; // The uninterrupted time a target was hidden for
        public bool   Visible;
        
        public static TargetInfo NullTarget = new TargetInfo();
        public        bool       Equals(Entity    other)
        {
            return other.Index == Entity.Index && other.Version == Entity.Version;
        }

        public        int        CompareTo(Entity other)
        {
            throw new NotImplementedException();
        }
    }
}