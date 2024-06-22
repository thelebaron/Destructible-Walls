using Junk.Entities;
using Unity.Entities;
using UnityEngine;

namespace Junk.Cameras
{
    
    public class CameraTargetAuthoring : MonoBehaviour
    {

    }

    public class CameraTargetBaker : Baker<CameraTargetAuthoring>
    {
        public override void Bake(CameraTargetAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, new SetCameraTargetTag());
        }
    }

    /// <summary>
    /// Tag: add this to an entity to set the camera target
    /// </summary>
    public struct SetCameraTargetTag : IComponentData
    {
    
    }

    [UpdateInGroup(typeof(EndSimulationStructuralChangeSystemGroup))]
    public partial class CameraTargetSystem : SystemBase
    {
        protected override void OnCreate()
        {
            RequireForUpdate<SetCameraTargetTag>();
            RequireForUpdate<CameraTarget>();
        }

        protected override void OnUpdate()
        {
            
            var target       = SystemAPI.GetSingletonEntity<SetCameraTargetTag>();
            var cameraTarget = SystemAPI.GetSingleton<CameraTarget>();
            cameraTarget.Target = target;
            SystemAPI.SetSingleton(cameraTarget);
            EntityManager.RemoveComponent<SetCameraTargetTag>(target);
        }
    }
}