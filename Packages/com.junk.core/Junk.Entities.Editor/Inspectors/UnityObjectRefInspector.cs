using JetBrains.Annotations;
using Unity.Entities;
using Unity.Entities.UI;
using UnityEditor.Search;
using UnityEngine;
using UnityEngine.UIElements;

namespace Junk.Entities.Editor.PropertyInspectors
{
    // thanks eizenhorn https://discussions.unity.com/t/where-can-i-see-the-materials-in-rendermesharray/1503804/3
    internal class UnityObjectRefInspector<T> : PropertyInspector<UnityObjectRef<T>> where T : Object
    {
        public override VisualElement Build()
        {
            var value = Target.Value;
            var name  = value == null ? DisplayName : $"{value.name}";
            var foldout = new Foldout
            {
                text  = name, 
                value = false
            };
            var id = new IntegerField("Instance Id")
            {
                value = Target.Id.instanceId
            };
            var of = new ObjectField
            {
                value = Target.Value
            };

            foldout.Add(id);
            foldout.Add(of);

            return foldout;
        }
    }

    [UsedImplicitly]
    internal class MaterialUnityObjectRefInspector : UnityObjectRefInspector<Material>
    {
    }

    [UsedImplicitly]
    internal class MeshUnityObjectRefInspector : UnityObjectRefInspector<Mesh>
    {
    }
    
    [UsedImplicitly]
    internal class ComputeShaderUnityObjectRefInspector : UnityObjectRefInspector<ComputeShader>
    {
    }
}