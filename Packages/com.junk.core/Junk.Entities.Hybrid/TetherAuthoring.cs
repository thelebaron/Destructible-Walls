using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Gizmos = UnityEngine.Gizmos;

namespace Junk.Entities.Hybrid
{
    public class TetherAuthoring : MonoBehaviour
    {
        public float Radius     = 5;
        public float UpdateRate = 0.1f;
        
        private void OnDrawGizmosSelected()
        {
            // Draw a yellow sphere at the transform's position
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(transform.position, Radius);
            // Create 8 positions, north, south, east, west, northeast, northwest, southeast, southwest, along the radius of the circle
            var center = transform.position + new Vector3(0, 0, 0);
            var position0 = transform.position + new Vector3(0, 0, Radius);
            var position1 = transform.position + new Vector3(0, 0, -Radius);
            var position2 = transform.position + new Vector3(Radius, 0, 0);
            var position3 = transform.position + new Vector3(-Radius, 0, 0);
            var position4 = transform.position + new Vector3(Radius/1.5f, 0, Radius/1.5f);
            var position5 = transform.position + new Vector3(-Radius/1.5f, 0, Radius/1.5f);
            var position6 = transform.position + new Vector3(Radius/1.5f, 0, -Radius/1.5f);
            var position7 = transform.position + new Vector3(-Radius/1.5f, 0, -Radius/1.5f);
            
            Gizmos.color = Color.yellow;
            var gizmoSize = 0.25f;
            Gizmos.DrawWireSphere(center, gizmoSize);
            Gizmos.DrawWireSphere(position0, gizmoSize);
            Gizmos.DrawWireSphere(position1, gizmoSize);
            Gizmos.DrawWireSphere(position2, gizmoSize);
            Gizmos.DrawWireSphere(position3, gizmoSize);
            Gizmos.DrawWireSphere(position4, gizmoSize);
            Gizmos.DrawWireSphere(position5, gizmoSize);
            Gizmos.DrawWireSphere(position6, gizmoSize);
            Gizmos.DrawWireSphere(position7, gizmoSize);
        }

        public class TetherAuthoringBaker : Baker<TetherAuthoring>
        {
            public override void Bake(TetherAuthoring authoring)
            {
                var entity = GetEntity(TransformUsageFlags.WorldSpace);
                
                // Create 8 positions, north, south, east, west, northeast, northwest, southeast, southwest, along the radius of the circle
                var position0 = authoring.transform.position + new Vector3(0, 0, authoring.Radius);
                var position1 = authoring.transform.position + new Vector3(0, 0, -authoring.Radius);
                var position2 = authoring.transform.position + new Vector3(authoring.Radius, 0, 0);
                var position3 = authoring.transform.position + new Vector3(-authoring.Radius, 0, 0);
                var position4 = authoring.transform.position + new Vector3(authoring.Radius, 0, authoring.Radius);
                var position5 = authoring.transform.position + new Vector3(-authoring.Radius, 0, authoring.Radius);
                var position6 = authoring.transform.position + new Vector3(authoring.Radius, 0, -authoring.Radius);
                var position7 = authoring.transform.position + new Vector3(-authoring.Radius, 0, -authoring.Radius);
                
                AddComponent(entity, new TetherData
                {
                    Rate = authoring.UpdateRate,
                });
                var heightOffset = 1.25f;
                
                var buffer = AddBuffer<TetherSamplePoint>(entity);
                buffer.Add(new TetherSamplePoint
                {
                    Point        = authoring.transform.position + heightOffset * Vector3.up,
                    Valid        = false,
                    CanSeePlayer = false
                });
                buffer.Add(new TetherSamplePoint
                {
                    Point = position0 + heightOffset * Vector3.up,
                    Valid = false,
                    CanSeePlayer = false
                });
                buffer.Add(new TetherSamplePoint
                {
                    Point        = position1 + heightOffset * Vector3.up,
                    Valid        = false,
                    CanSeePlayer = false
                });
                buffer.Add(new TetherSamplePoint
                {
                    Point        = position2 + heightOffset * Vector3.up,
                    Valid        = false,
                    CanSeePlayer = false
                });
                buffer.Add(new TetherSamplePoint
                {
                    Point        = position3 + heightOffset * Vector3.up,
                    Valid        = false,
                    CanSeePlayer = false
                });
                buffer.Add(new TetherSamplePoint
                {
                    Point        = position4 + heightOffset * Vector3.up,
                    Valid        = false,
                    CanSeePlayer = false
                });
                buffer.Add(new TetherSamplePoint
                {
                    Point        = position5 + heightOffset * Vector3.up,
                    Valid        = false,
                    CanSeePlayer = false
                });
                buffer.Add(new TetherSamplePoint
                {
                    Point        = position6 + heightOffset * Vector3.up,
                    Valid        = false,
                    CanSeePlayer = false
                });
                buffer.Add(new TetherSamplePoint
                {
                    Point        = position7 + heightOffset * Vector3.up,
                    Valid        = false,
                    CanSeePlayer = false
                });
            }
        }
    }


}