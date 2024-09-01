using Junk.Entities;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.AI;

namespace Junk.Navigation.Hybrid
{
    public partial struct NavmeshLinkSystem : ISystem
    {
        private EntityQuery query;
        private EntityQuery cleanupQuery;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            query = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<NavLink>()
                .WithNone<NavLinkManagedState>()
                .WithAll<LocalToWorld>()
                .Build(ref state);
            
            cleanupQuery = new EntityQueryBuilder(Allocator.Temp)
                .WithAllRW<NavLinkManagedState>()
                .Build(ref state);
        }

        
        public void OnUpdate(ref SystemState state)
        {
            var entities = query.ToEntityArray(Allocator.Temp);
            var links = query.ToComponentDataArray<NavLink>(Allocator.Temp);
            var localToWorlds = query.ToComponentDataArray<LocalToWorld>(Allocator.Temp);
            
            for (int i = 0; i < entities.Length; ++i)
            {
                var entity       = entities[i];
                var link         = links[i];
                var localToWorld = localToWorlds[i];
                
                var handle       = new ObjectHandle(entity);
                var managed = new NavLinkManagedState
                {
                    Instance = NavMesh.AddLink(link.Data, localToWorld.Position, localToWorld.Rotation),
                    ObjectHandle   = handle
                };
                
                state.EntityManager.AddComponentObject(entity, managed);
                
                if (NavMesh.IsLinkValid(managed.Instance))
                {
                    NavMesh.SetLinkOwner(managed.Instance, managed.ObjectHandle);
                    NavMesh.SetLinkActive(managed.Instance, true);
                    Debug.Log($"Added link from entity " + entity.Index);
                }
            }
            /*
            foreach (var (unmanagedNavmeshLink, localToWorld) in SystemAPI.Query<RefRW<NavLink>, RefRO<LocalToWorld>>().WithNone<NavLinkManagedState>())
            {

            }*/
        }
        
        //[BurstCompile]
        public void OnDestroy(ref SystemState state)
        {
            var entities = query.ToEntityArray(Allocator.Temp);

            for (int i = 0; i < entities.Length; ++i)
            {
                var entity       = entities[i];
                var managed = state.EntityManager.GetComponentObject<NavLinkManagedState>(entity);
                
                NavMesh.RemoveLink(managed.Instance);
                state.EntityManager.RemoveComponent<NavLinkManagedState>(entity);
            }
        }
    }
}