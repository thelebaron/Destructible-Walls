using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

namespace Junk.LineRenderer
{
    /// <summary>
    /// For use with Game Object Conversion Workflow / Subscene.
    /// 
    /// Generates an Entity with <see cref="LineSegment"/> and <see cref="LineStyle"/>.
    /// Then <see cref="LineSegmentRegisterSystem"/> will pick that up. 
    /// </summary>
    public class LineAuthoring : MonoBehaviour
    {
        public LineSegment lineSegment;
        public LineStyle   lineStyle;
        public Mesh        mesh;
        
        void OnValidate()
        {
            lineSegment.lineWidth = math.max(0, lineSegment.lineWidth);
        }
    }
    
    public class LineBaker : Baker<LineAuthoring>
    {
        public override void Bake(LineAuthoring authoring)
        {
            var entity = GetEntity(TransformUsageFlags.Dynamic);
            AddComponent(entity, authoring.lineSegment);
            AddSharedComponentManaged(entity, authoring.lineStyle);
            //AddComponent<NonUniformScale>();
            
            var compositeScale = float4x4.Scale(1);
            AddComponent(entity, new PostTransformMatrix { Value = compositeScale });
            //AddComponent<PropagateLocalToWorld>();
        }
    }
    
}