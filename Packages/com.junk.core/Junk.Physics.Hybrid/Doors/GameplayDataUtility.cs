using Junk.Entities;
using UnityEngine;

namespace Junk.Gameplay.Hybrid
{
    public static class GameplayDataUtility
    {
        public static void FitTriggerShape(this GameplayAuthoring authoring)
        {
            authoring.IsTrigger = true;
            
            if(authoring.TriggerCenter.Equals(Vector3.zero)) authoring.TriggerCenter   = GetCenter(authoring)        - authoring.transform.position;
            if(authoring.TriggerExtents.Equals(Vector3.zero)) authoring.TriggerExtents = GetBoundsExtents(authoring) + Vector3.one;
        }
        
        public static void FitColliderShape(this GameplayAuthoring authoring)
        {
            authoring.IsCollider = true;
            
            if(authoring.ColliderCenter.Equals(Vector3.zero)) authoring.ColliderCenter   = GetCenter(authoring)        - authoring.transform.position;
            if(authoring.ColliderExtents.Equals(Vector3.zero)) authoring.ColliderExtents = GetBoundsExtents(authoring) * 2; // + Vector3.one
        }
        
        public static Vector3 GetCenter(this GameplayAuthoring authoring)
        {
            var component = authoring as Component;
            var renderers = component.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return component.transform.position;
            
            var combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                combinedBounds.Encapsulate(renderers[i].bounds);

            return combinedBounds.center;
        }

        public static Vector3 GetBoundsExtents(this Component authoring)
        {
            var component = authoring as Component;
            var renderers = component.GetComponentsInChildren<Renderer>();
            if (renderers.Length == 0)
                return Vector3.zero;
            
            var combinedBounds = renderers[0].bounds;
            for (int i = 1; i < renderers.Length; i++)
                combinedBounds.Encapsulate(renderers[i].bounds);
            
            return combinedBounds.extents;
        }
    }
}