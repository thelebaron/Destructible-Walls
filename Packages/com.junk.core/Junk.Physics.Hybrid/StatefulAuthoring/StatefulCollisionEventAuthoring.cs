using Unity.Entities;
using UnityEngine;

namespace Junk.Physics.Stateful
{
    public class StatefulCollisionEventAuthoring : MonoBehaviour
    {
        [Tooltip("If selected, the details will be calculated in collision event dynamic buffer of this entity")]
        public bool CalculateDetails = false;

        class Baker : Baker<StatefulCollisionEventAuthoring>
        {
            public override void Bake(StatefulCollisionEventAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.Dynamic);
                if (authoring.CalculateDetails)
                {
                    AddComponent(entity, new StatefulCollisionEventDetails
                    {
                        CalculateDetails = authoring.CalculateDetails
                    });
                }
                AddBuffer<StatefulCollisionEvent>(entity);
            }
        }
    }

    // Collision Event that can be stored inside a DynamicBuffer
}
