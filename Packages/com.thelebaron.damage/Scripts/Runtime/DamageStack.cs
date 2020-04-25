using Unity.Entities;

namespace thelebaron.damage
{
    // This describes the number of buffer elements that should be reserved
    // in chunk data for each instance of a buffer. In this case, 8 integers
    // will be reserved (32 bytes) along with the size of the buffer header
    // (currently 16 bytes on 64-bit targets)
    [GenerateAuthoringComponent]
    [InternalBufferCapacity(16)]
    public struct DamageStack : IBufferElementData
    {
        // These implicit conversions are optional, but can help reduce typing.
        public static implicit operator DamageEvent(DamageStack e)
        {
            return e.Value;
        }

        public static implicit operator DamageStack(DamageEvent e)
        {
            return new DamageStack {Value = e};
        }

        // Actual value each buffer element will store.
        public DamageEvent Value;
    }
}