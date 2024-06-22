using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using UnityEngine.UIElements;

namespace Junk.Entities.Hybrid
{
    
    [UpdateInGroup(typeof(InitializationSystemGroup))]
    public partial struct GameSystem : ISystem
    {
        private float inputBlockTime;
        
        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<UnmanagedMouse>();
            state.RequireForUpdate<UnmanagedKeyboard>();

            // Self initialize
            var entity = state.EntityManager.CreateEntity();
            state.EntityManager.AddComponent<Game>(entity);
            state.EntityManager.AddComponent<GameMenu>(entity);
            state.EntityManager.AddComponent<GameSave>(entity);
            state.EntityManager.SetComponentEnabled<GameMenu>(entity, true);
        }
        
        public void OnUpdate(ref SystemState state)
        {
            // show or hide the menu screen
            var keyboard = SystemAPI.GetSingleton<UnmanagedKeyboard>();
            foreach (var (game, entity) in SystemAPI.Query<RefRW<Game>>().WithPresent<GameMenu>().WithOptions(EntityQueryOptions.IgnoreComponentEnabledState).WithEntityAccess())
            {
                game.ValueRW.LimitInputTime -= SystemAPI.Time.DeltaTime;
                
                var escapeKeyIsPressed = keyboard.escapeKey.isPressed;

                if (!escapeKeyIsPressed || !(game.ValueRW.LimitInputTime <= 0)) 
                    continue;
                
                game.ValueRW.LimitInputTime = 0.2f;
                    
                var menuComponentData = state.EntityManager.GetComponentData<GameMenu>(entity);
                var menuEnabled       = state.EntityManager.IsComponentEnabled<GameMenu>(entity);

                if (menuEnabled && !menuComponentData.PlayableSceneIsLoaded)
                    continue;

                GameScreenBase.SetMenuEnabled(!menuEnabled);
                state.EntityManager.SetComponentEnabled<GameMenu>(entity, !menuEnabled);
                

                game.ValueRW.LimitInputTime = 0.2f;
            }
        }
        
        public static void SetSceneLoadedStatusAsync(WorldUnmanaged world, bool enabledState = false)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp).WithAllRW<GameMenu>();
            
            // should only ever be one but eh
            using var q       = world.EntityManager.CreateEntityQuery(builder);
            foreach (var e in q.ToEntityArray(Allocator.Temp))
            {
                var r = world.EntityManager.GetComponentData<GameMenu>(e);
                r.PlayableSceneIsLoaded = enabledState;
                world.EntityManager.SetComponentData(e, r);
            }
        }
        
        public static void SetMenuEnabledAsync(WorldUnmanaged world, bool enabledState = false)
        {
            var builder = new EntityQueryBuilder(Allocator.Temp).WithAllRW<GameMenu>();
            
            // should only ever be one but eh
            using var q       = world.EntityManager.CreateEntityQuery(builder);
            foreach (var e in q.ToEntityArray(Allocator.Temp))
            {
                world.EntityManager.SetComponentEnabled<GameMenu>(e, enabledState);
            }
        }
    }
}