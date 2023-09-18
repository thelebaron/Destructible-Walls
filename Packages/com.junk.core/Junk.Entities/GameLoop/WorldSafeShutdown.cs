using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.LowLevel;

namespace Junk.Entities
{
    public static class WorldSafeShutdown
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Init()
        {
            Application.quitting += OnQuit;
        }
        private static void OnQuit()
        {
            Application.quitting -= OnQuit;

            for (var index = 0; index < World.All.Count; index++)
            {
                var world = World.All[index];
                if (world.IsCreated && (world.Flags & WorldFlags.Live) != 0)
                {
                    TryDisableUpdateSystemGroup<InitializationSystemGroup>(world);
                    TryDisableUpdateSystemGroup<SimulationSystemGroup>(world);
                    TryDisableUpdateSystemGroup<PresentationSystemGroup>(world);
                }
            }
        }

        private static void TryDisableUpdateSystemGroup<T>(World world)
            where T : ComponentSystemBase
        {
            var system = world.GetExistingSystemManaged<T>();
            if (system != null)
            {
                system.Enabled = false;
                system.Update();
            }
        }
        /*private static void OnQuit()
        {
            Application.quitting -= OnQuit;

            var playerLoop = PlayerLoop.GetCurrentPlayerLoop();

            var worlds = new List<World>();

            for (var index = 0; index < World.All.Count; index++)
            {
                if ((World.All[index].Flags & WorldFlags.Live) != 0)
                {
                    worlds.Add(World.All[index]);
                }
            }

            foreach (var world in worlds)
            {
                ScriptBehaviourUpdateOrder.RemoveWorldFromPlayerLoop(world, ref playerLoop);
            }

            PlayerLoop.SetPlayerLoop(playerLoop);

            foreach (var world in worlds)
            {
                if (world.IsCreated)
                {
                    world.Dispose();
                }
            }

            World.DefaultGameObjectInjectionWorld = null;
        }*/
    }
}