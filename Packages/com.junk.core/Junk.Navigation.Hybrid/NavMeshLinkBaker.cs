using System.Reflection;
using Junk.Entities;
using Unity.AI.Navigation;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.AI;

namespace Junk.Navigation.Hybrid
{
    public class NavMeshLinkBaker : Baker<NavMeshLink>
    {
        Matrix4x4 LocalToWorldUnscaled(NavMeshLink link)
        {
            return Matrix4x4.TRS(link.transform.position, link.transform.rotation, Vector3.one);
        }
        
        void GetLocalPositions(NavMeshLink link,
            out Vector3 localStartPosition,
            out Vector3 localEndPosition)
        {
            var startIsLocal = link.startTransform == null;
            var endIsLocal   = link.endTransform == null;
            var toLocal      = startIsLocal && endIsLocal ? Matrix4x4.identity : LocalToWorldUnscaled(link).inverse;
            
            // Use reflection to access the private fields m_StartPoint and m_EndPointvar
            var type = typeof(NavMeshLink);
            var startPointField = type.GetField("m_StartPoint", BindingFlags.NonPublic | BindingFlags.Instance);
            var endPointField   = type.GetField("m_EndPoint", BindingFlags.NonPublic   | BindingFlags.Instance);

            var m_StartPoint = (Vector3)startPointField.GetValue(link);
            var m_EndPoint   = (Vector3)endPointField.GetValue(link);

            localStartPosition = startIsLocal ? m_StartPoint : toLocal.MultiplyPoint3x4(link.startTransform.position);
            localEndPosition   = endIsLocal ? m_EndPoint : toLocal.MultiplyPoint3x4(link.endTransform.position);
        }
        
        public override void Bake(NavMeshLink authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            
            GetLocalPositions(authoring, out var localStartPosition, out var localEndPosition);
            
            var data = new NavMeshLinkData
            {
                startPosition = localStartPosition,
                endPosition   = localEndPosition,
                width         = authoring.width,
                costModifier  =  authoring.costModifier,
                bidirectional =  authoring.bidirectional,
                area          =  authoring.area,
                agentTypeID   =  authoring.agentTypeID,
            };
            AddComponent(entity, new NavLink
            {
                Data = data,
            });
        }
    }
    
    

    public struct NavLink : IComponentData
    {
        public NavMeshLinkData     Data;
    }

    public class NavLinkManagedState : ICleanupComponentData
    {
        public NavMeshLinkInstance Instance;
        public ObjectHandle  ObjectHandle;
    }
}