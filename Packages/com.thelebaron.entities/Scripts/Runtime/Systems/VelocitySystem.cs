using thelebaron.mathematics;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

namespace thelebaron.bee
{
    [UpdateInGroup(typeof(SimulationSystemGroup))]
    public class VelocitySystem : SystemBase
    {
        private EntityQuery velocityQuery;
        protected override void OnCreate()
        {
            velocityQuery = GetEntityQuery(//typeof(PhysicsGravityFactor), 
                typeof(Velocity), typeof(Translation), typeof(Rotation));
        }
        
        [BurstCompile]
        private struct VelocityMove : IJobChunk
        {
            [ReadOnly] public float FixedDeltaTime;
            public ComponentTypeHandle<Translation> TranslationType;
            public ComponentTypeHandle<Rotation> RotationType;
            public ComponentTypeHandle<Velocity> VelocityType;
            public ComponentTypeHandle<BallisticMotion> BallisticMotionType;

            public void Execute(ArchetypeChunk chunk, int chunkIndex, int firstEntityIndex)
            {
                const float actualGravity = 9.807f;
                var chunkTranslations = chunk.GetNativeArray(TranslationType);
                var chunkRotations = chunk.GetNativeArray(RotationType);
                var chunkVelocities = chunk.GetNativeArray(VelocityType);
                var chunkBallisticMotions = chunk.GetNativeArray(BallisticMotionType);
                var hasBallisticMotion = chunk.Has(BallisticMotionType);

                if (hasBallisticMotion)
                {
                    for (var i = 0; i < chunk.Count; i++)
                    {
                        var ballisticMotion = chunkBallisticMotions[i];
                        var velocity    = chunkVelocities[i];
                        
                        // Gravity is factored into the velocity, can be ignored by setting the gravityfactor to 0
                        velocity.Value.y -= actualGravity * FixedDeltaTime * ballisticMotion.GravityFalloff;
                        
                        // Also apply a small amount of drag over time
                        if (math.length(velocity.Value) > 0)
                            velocity.Value -= ballisticMotion.Drag * FixedDeltaTime * maths.one;
                        
                        chunkVelocities[i] = velocity;
                    }
                }
                
                for (int i = 0; i < chunk.Count; i++)
                {
                    var translation = chunkTranslations[i];
                    var rotation = chunkRotations[i];
                    var velocity = chunkVelocities[i];
                    
                    // We align to velocity so that the trajectory changes with distance. Can be disabled.
                    rotation.Value = quaternion.LookRotation(math.normalize(velocity.Value), maths.up);
                    
                    // Finally move
                    translation.Value += velocity.Value * FixedDeltaTime;
                    
                    chunkTranslations[i] = translation;
                    chunkRotations[i] = rotation;
                    chunkVelocities[i] = velocity;
                }
                //Marker.End();
            }
        }
        
        protected override void OnUpdate()
        {
            Dependency = new VelocityMove
            {
                FixedDeltaTime           = Time.fixedDeltaTime,
                TranslationType          = GetComponentTypeHandle<Translation>(),
                RotationType             = GetComponentTypeHandle<Rotation>(),
                VelocityType             = GetComponentTypeHandle<Velocity>(),
                BallisticMotionType     = GetComponentTypeHandle<BallisticMotion>()
            }.Schedule(velocityQuery, Dependency);
        } 
    }
}
