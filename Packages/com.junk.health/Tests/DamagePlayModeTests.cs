
using System.Collections;
using NUnit.Framework;
using Unity.Entities;
using Unity.Jobs.LowLevel.Unsafe;
using UnityEngine.TestTools;

namespace Junk.Hitpoints.Tests
{
    public class DamagePlayModeTests
    {
        protected World                    world;
        protected EntityManager            entityManager;
        private   EntityQuery              healthQuery;

        [SetUp]
        public void Setup()
        {
            world         = World.DefaultGameObjectInjectionWorld = new World("Test World");
            entityManager = world.EntityManager;
            healthQuery   = entityManager.CreateEntityQuery(typeof(HealthData));

            //var handlehealthSystem = World.DefaultGameObjectInjectionWorld.Unmanaged.GetExistingUnmanagedSystem<HealthSystem>();
            
                
            // Don't know if you can get the server/client worlds like this, but you need a reference to them
            // for the steps below
            //World serverWorld = ClientServerBootstrap.CreateServerWorld(defaultWorld, "ServerWorld");
            //World clientWorld = ClientServerBootstrap.CreateClientWorld(defaultWorld, "ClientWorld");
     
            // I'm not sure if your ClientServerBootstrap includes the scene systems in the target worlds,
            // but they're required to load SubScenes into worlds.
            //SceneSystem serverSceneSystem = serverWorld.GetExistingSystem<SceneSystem>();
            //var clientSceneSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<SceneSystem>();
     
            // If you need to unload the scene, these functions return an entity that acts as a scene instance reference.
            // This function also contains an optional parameter to control how the scene is loaded (synchronous loading etc)
            //Entity serverScene = serverSceneSystem.LoadSceneAsync(MySubScene.SceneGUID);
            //Entity clientScene = clientSceneSystem.LoadSceneAsync(MySubScene.SceneGUID);
     
            // If you need to unload scenes, you do this.
            //serverSceneSystem.UnloadScene(serverScene);
            //clientSceneSystem.UnloadScene(clientScene);
                
            var e1 = entityManager.CreateEntity();
            HealthBakingUtility.AddComponents(e1, entityManager);
            
            // Many ECS tests will only pass if the Jobs Debugger enabled;
            // force it enabled for all tests, and restore the original value at teardown.
            JobsUtility.JobDebuggerEnabled = true;
            
            //Assert.IsTrue(healthSystem.ShouldRunSystem());
        }

        [TearDown]
        protected void TearDown()
        {
            //world.Dispose();
        }

        [Test]
        public void WorldTest()
        {
            Assert.IsNotNull(world, "world != null");
        }

        [Test]
        public void EntityManagerTest()
        {
            Assert.IsNotNull(entityManager, "entityManager != null");
        }

        //[Test]
        //public void HealthSystemTest()
        //{
            //Assert.IsNotNull(healthSystem, "healthSystem != null");
        //}


        [Test]
        public void PlayModeTestsSimplePasses()
        {
            Assert.IsTrue(healthQuery.CalculateChunkCount() > 0, "healthQuery.CalculateChunkCount()>0");
        }

        // A UnityTest behaves like a coroutine in Play Mode. In Edit Mode you can use
        // `yield return null;` to skip a frame.
        [UnityTest]
        public IEnumerator PlayModeTestsWithEnumeratorPasses()
        {
            // Use the Assert class to test conditions.
            // Use yield to skip a frame.
            yield return null;
        }
    }
}