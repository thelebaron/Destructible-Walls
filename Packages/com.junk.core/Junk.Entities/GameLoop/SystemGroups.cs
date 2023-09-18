using Unity.Entities;
using UnityEditor;

namespace Junk.Entities
{
    // A group that runs right at the very beginning of SimulationSystemGroup  
    /*[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    [UpdateBefore(typeof(BeginSimulationEntityCommandBufferSystem))]
    public partial class PreSimulationStructuralChangeSystemGroup : ComponentSystemGroup { }*/
  
    // A group that runs right at the very end of SimulationSystemGroup  
    
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderLast = true)]
    [UpdateAfter(typeof(EndSimulationEntityCommandBufferSystem))]
    public partial class EndSimulationStructuralChangeSystemGroup : ComponentSystemGroup { }
    
    
    [DisableAutoCreation]
    [WorldSystemFilter(WorldSystemFilterFlags.Default | WorldSystemFilterFlags.Editor)]
    [UpdateInGroup(typeof(EndSimulationStructuralChangeSystemGroup), OrderLast = true)]
    //[UpdateAfter(typeof(EndSimulationStructuralChangeSystemGroup))]
    public partial class EndStructuralChangeEntityCommandBufferSystem : EntityCommandBufferSystem
    {
    }
    
    
    // A group that runs right at the very end of SimulationSystemGroup  
    [UpdateInGroup(typeof(EndSimulationStructuralChangeSystemGroup))]
    public partial class MenuSystemGroup : ComponentSystemGroup { }

    [UpdateInGroup(typeof(EndSimulationStructuralChangeSystemGroup))]
    public abstract partial class CopyECSToManagedDataSystem : SystemBase
    {
        // Copy processed data from components back into managed objects  
        protected override void OnUpdate()
        {
        }
    }
}