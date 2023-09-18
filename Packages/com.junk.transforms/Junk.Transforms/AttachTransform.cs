using Unity.Entities;

namespace Junk.Transforms
{
    /// <summary>
    /// Used to "attach" one entity's local transform(translation & rotation) to another via transformations. Ie - pure entity parent to child relationships involving physics,
    /// say trigger to follow a rigidbody (which are not supported). While similar, this is NOT a replacement for CopyTransformTo/FromGameObject.
    /// </summary>
    //
    public struct AttachTransform : IComponentData
    {
        // The entity whos transform we will mimic by "attaching" to.
        public Entity Value;
    }
}