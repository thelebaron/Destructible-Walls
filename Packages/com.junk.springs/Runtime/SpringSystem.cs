using Unity.Burst;
using Unity.Collections;
using Unity.Entities;

namespace Junk.Springs
{
    [BurstCompile]
    [UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
    public partial struct SpringSystem : ISystem
    {
        private EntityQuery query;

        [BurstCompile]
        public void OnCreate(ref  SystemState state)
        {
            query = new EntityQueryBuilder(Allocator.Temp).WithAllRW<Spring>().WithAllRW<SoftForce>().Build(ref state);
        }

        [BurstCompile]
        public void OnUpdate(ref  SystemState state)
        {
            state.Dependency = new UpdateSpringsJob
            {
                DeltaTime    = (float)SystemAPI.Time.DeltaTime,
                ElapsedTime   = (float)SystemAPI.Time.ElapsedTime,
                SpringType    = SystemAPI.GetComponentTypeHandle<Spring>(),
                SoftForceType = SystemAPI.GetBufferTypeHandle<SoftForce>()
            }.Schedule(query, state.Dependency);
        }
    }
    

}