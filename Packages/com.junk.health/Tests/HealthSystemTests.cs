using NUnit.Framework;
using Unity.Entities;
using UnityEngine;

namespace Junk.Hitpoints.Tests
{
    public class HealthSystemTests
    {
        private   Entity        entity;
        
        protected World         world;
        protected EntityManager entityManager;
        private   EntityQuery   healthQuery;
        
        [SetUp]
        public void Setup()
        {
            world = World.DefaultGameObjectInjectionWorld = new World("Test World");
            entityManager = world.EntityManager;
            healthQuery   = entityManager.CreateEntityQuery(typeof(HealthData));
            
            entity = entityManager.CreateEntity(
                typeof(HealthData),
                typeof(HealthState),
                typeof(HealthParent),
                typeof(HealthMultiplier),
                typeof(HealthDamageBuffer),
                typeof(HealthPhysicsDeath),
                typeof(HealthFeedback)
            );
        }

        [TearDown]
        public void TearDown()
        {
            entityManager.DestroyEntity(entity);
            entityManager.CompleteAllTrackedJobs();
            world.Dispose();
        }

        [Test]
        public void HealthSystem_EntityHasHealthComponent()
        {
            Assert.IsTrue(entityManager.HasComponent<HealthData>(entity));
        }

        [Test]
        public void HealthSystem_EntityHasDeadComponent()
        {
            Assert.IsFalse(entityManager.HasComponent<Dead>(entity));
        }
        
        [Test]
        public void HealthSystem_DoesNotAddDeadComponent_WhenHealthGreaterThanZero()
        {
            entityManager.SetComponentData(entity, new HealthData { Value = 10 });
            //world.GetOrCreateSystem<HealthSystem>();
            //world.Update();
            //world.Unmanaged.GetExistingUnmanagedSystem<HealthSystem>().Update(world.Unmanaged);
            
            Assert.IsFalse(entityManager.HasComponent<Dead>(entity));
        }

        /*
        [Test]
        public void HealthSystem_AddsDeadComponent_WhenHealthIsZero()
        {
            entityManager.SetComponentData(entity, new Health { Value = 0 });
            entityManager.SetComponentData(entity, new HealthState { Value = 0 });

            var system = new HealthSystem();
            system.OnCreate(default);

            system.Update();

            Assert.IsTrue(entityManager.HasComponent<Dead>(entity));
        }

        [Test]
        public void HealthSystem_AddsDeadComponent_WhenHealthIsNegative()
        {
            entityManager.SetComponentData(entity, new Health { Value = -10 });
            entityManager.SetComponentData(entity, new HealthState { Value = 0 });

            var system = new HealthSystem();
            system.OnCreate(default);

            system.Update();

            Assert.IsTrue(entityManager.HasComponent<Dead>(entity));
        }

        [Test]
        public void HealthSystem_DestroysEntity_WhenHealthIsZeroAndHasDestroyOnZeroHealthComponent()
        {
            entityManager.SetComponentData(entity, new Health { Value = 0 });
            entityManager.SetComponentData(entity, new HealthState { Value = 0 });
            entityManager.AddComponentData(entity, new DestroyOnZeroHealth());

            var system = new HealthSystem();
            system.OnCreate(default);

            system.Update();

            Assert.IsFalse(entityManager.Exists(entity));
        }

        [Test]
        public void HealthSystem_DoesNotDestroyEntity_WhenHealthIsZeroAndDoesNotHaveDestroyOnZeroHealthComponent()
        {
            entityManager.SetComponentData(entity, new Health { Value = 0 });
            entityManager.SetComponentData(entity, new HealthState { Value = 0 });

            var system = new HealthSystem();
            system.OnCreate(default);

            system.Update();

            Assert.IsTrue(entityManager.Exists(entity));
        }

        [Test]
        public void HealthSystem_DoesNotAddDeadComponent_WhenEntityHasHealthMultiplierComponent()
        {
            entityManager.SetComponentData(entity, new Health { Value = 0 });
            entityManager.SetComponentData(entity, new HealthState { Value = 0 });
            entityManager.AddComponentData(entity, new HealthMultiplier());

            var system = new HealthSystem();
            system.OnCreate(default);

            system.Update();

            Assert.IsFalse(entityManager.HasComponent<Dead>(entity));
        }*/
    }
}