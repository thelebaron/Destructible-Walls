using Junk.Entities;
using Unity.Entities;
using UnityEngine;

public class TimescaleBehaviour : MonoBehaviour
{
    [Range(0.0f, 1.0f)] 
    public float TimeScale = 1.0f;
    public  float                          defaultTimeStep = 0.02f;
    private FixedStepSimulationSystemGroup simulationGroup;
    private EntityQuery                    timeScaleQuery;
    private World                          world;

    private void GetWorld()
    {
        world = World.DefaultGameObjectInjectionWorld;
        simulationGroup = world.GetExistingSystemManaged<FixedStepSimulationSystemGroup>();
        defaultTimeStep = simulationGroup.Timestep;
        timeScaleQuery = World.DefaultGameObjectInjectionWorld.EntityManager.CreateEntityQuery(typeof(TimeScale));
    }

    private void Update()
    {
        if(World.All.Count<1)
            return;
        
        if(world == null)
            GetWorld();
        
        simulationGroup.Timestep = defaultTimeStep * TimeScale;
        Time.timeScale = TimeScale;
        
        timeScaleQuery.SetSingleton(new TimeScale {Value = TimeScale});
    }
}
