using System.Collections.Generic;
using Junk.Core.Creation;
using UnityEditor;
using UnityEngine;

namespace Junk.Destroy.Hybrid
{
    /// <summary>
    /// An asset that stores a list of fractures
    /// </summary>
    [CreateAssetMenu(fileName = "Fracture Cache", menuName = "Fracturing", order = 0)]
    public class FractureCache : ScriptableObject
    {
        [HideInInspector] public FractureChild       Parent;
        public                   Mesh                Mesh;
        public                   List<FractureChild> Fractures = new();
        public void Add(Shape shape, Material insideMaterial, Material outsideMaterial)
        {
            var fracture = ScriptableObject.CreateInstance<FractureChild>();
            fracture.Shape           = shape;
            fracture.InsideMaterial  = insideMaterial;
            fracture.OutsideMaterial = outsideMaterial;
            fracture.Parent     = this;
            fracture.name = shape.name;
            AssetDatabase.AddObjectToAsset(fracture, this);
            Fractures.Add(fracture);
        }

        public void Clear()
        {
            for (var index = 0; index < Fractures.Count; index++)
            {
                var fracture = Fractures[index];
                AssetDatabase.RemoveObjectFromAsset(fracture);
                DestroyImmediate(fracture, true);
            }

            Fractures.Clear();
            AssetDatabase.SaveAssets();
        }
    }
    
    /// <summary>
    /// An asset that stores a list of fractures
    /// </summary>
    public class FractureNode : ScriptableObject
    {
        public Mesh               Mesh;
        public Material           InsideMaterial;
        public Material           OutsideMaterial;
        public FractureNode       Parent;
        public List<FractureNode> Children = new();
    }
}