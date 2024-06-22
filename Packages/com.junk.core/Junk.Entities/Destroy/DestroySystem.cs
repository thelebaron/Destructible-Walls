using Junk.Entities;
using Unity.Burst;
using Unity.Entities;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;

// Note this is 1:1 copied from tertle's excellent Core library
// Copyright (c) BovineLabs. All rights reserved.

namespace Junk.Entities
{
    [UpdateInGroup(typeof(DestroySystemGroup), OrderLast = true)]
    [WorldSystemFilter(WorldSystemFilterFlags.Default)]// | Worlds.Service)]
    public partial class DestroyEntityCommandBufferSystem : EntityCommandBufferSystem
    {
        /// <inheritdoc cref="EntityCommandBufferSystem.OnCreate"/>
        protected override void OnCreate()
        {
            base.OnCreate();

            this.RegisterSingleton<Singleton>(ref this.PendingBuffers, this.World.Unmanaged);
        }

        /// <summary>
        /// Call <see cref="SystemAPI.GetSingleton{T}"/> to get this component for this system, and then call
        /// <see cref="CreateCommandBuffer"/> on this singleton to create an ECB to be played back by this system.
        /// </summary>
        /// <remarks>
        /// Useful if you want to record entity commands now, but play them back at a later point in
        /// the frame, or early in the next frame.
        /// </remarks>
        public unsafe struct Singleton : IComponentData, IECBSingleton
        {
            internal UnsafeList<EntityCommandBuffer>* pendingBuffers;
            internal AllocatorManager.AllocatorHandle allocator;

            /// <summary>
            /// Create a command buffer for the parent system to play back.
            /// </summary>
            /// <remarks>The command buffers created by this method are automatically added to the system's list of
            /// pending buffers.</remarks>
            /// <param name="world">The world in which to play it back.</param>
            /// <returns>The command buffer to record to.</returns>
            public EntityCommandBuffer CreateCommandBuffer(WorldUnmanaged world)
            {
                return EntityCommandBufferSystem.CreateCommandBuffer(ref *this.pendingBuffers, this.allocator, world);
            }

            /// <summary>
            /// Sets the list of command buffers to play back when this system updates.
            /// </summary>
            /// <remarks>This method is only intended for internal use, but must be in the public API due to language
            /// restrictions. Command buffers created with <see cref="CreateCommandBuffer"/> are automatically added to
            /// the system's list of pending buffers to play back.</remarks>
            /// <param name="buffers">The list of buffers to play back. This list replaces any existing pending command buffers on this system.</param>
            public void SetPendingBufferList(ref UnsafeList<EntityCommandBuffer> buffers)
            {
                this.pendingBuffers = (UnsafeList<EntityCommandBuffer>*)UnsafeUtility.AddressOf(ref buffers);
            }

            /// <summary>
            /// Set the allocator that command buffers created with this singleton should be allocated with.
            /// </summary>
            /// <param name="allocatorIn">The allocator to use</param>
            public void SetAllocator(Allocator allocatorIn)
            {
                this.allocator = allocatorIn;
            }

            /// <summary>
            /// Set the allocator that command buffers created with this singleton should be allocated with.
            /// </summary>
            /// <param name="allocatorIn">The allocator to use</param>
            public void SetAllocator(AllocatorManager.AllocatorHandle allocatorIn)
            {
                this.allocator = allocatorIn;
            }
        }
    }
    
    [UpdateInGroup(typeof(DestroySystemGroup), OrderLast = true)]
    public partial struct DestroySystem : ISystem
    {
        private EntityQuery query;

        [BurstCompile]
        public void OnCreate(ref SystemState state)
        {
            state.RequireForUpdate<DestroyEntityCommandBufferSystem.Singleton>();
#if UNITY_NETCODE
            // Client doesn't destroy ghosts, instead we'll disable them in
            this.query = Unity.NetCode.ClientServerWorldExtensions.IsClient(state.WorldUnmanaged)
                ? SystemAPI.QueryBuilder().WithAll<Destroy>().WithNone<Unity.NetCode.GhostInstance>().Build()
                : SystemAPI.QueryBuilder().WithAll<Destroy>().Build();
#else
            this.query = SystemAPI.QueryBuilder().WithAll<Destroy>().Build();
#endif
            this.query.SetChangedVersionFilter(ComponentType.ReadOnly<Destroy>());
        }

        [BurstCompile]
        public void OnUpdate(ref SystemState state)
        {
            var bufferSingleton = SystemAPI.GetSingleton<DestroyEntityCommandBufferSystem.Singleton>();
            new DestroyJob { CommandBuffer = bufferSingleton.CreateCommandBuffer(state.WorldUnmanaged).AsParallelWriter() }.ScheduleParallel(this.query);
        }

        [WithChangeFilter(typeof(Destroy))]
        [WithAll(typeof(Destroy))]
        [BurstCompile]
        private partial struct DestroyJob : IJobEntity
        {
            public EntityCommandBuffer.ParallelWriter CommandBuffer;

            private void Execute([ChunkIndexInQuery] int chunkIndexInQuery, Entity entity)
            {
                this.CommandBuffer.DestroyEntity(chunkIndexInQuery, entity);
            }
        }
    }
}