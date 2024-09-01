using System.Collections.Generic;
using Junk.Entities.Hybrid;
using Unity.Entities;
using UnityEditor;
using UnityEngine;

namespace Junk.Fracture.Hybrid
{
    #if UNITY_EDITOR
    /// <summary>
    /// An asset that stores a list of fractures
    /// </summary>
    [CreateAssetMenu(fileName = "Fracture Cache", menuName = "Fracturing", order = 0)]
    public class FractureCache : BakedScriptableObject<FractureData>
    {
        // ECS data
        public bool UseAnchors;
        
        // Unity data
        public                   Mesh                Mesh;
        
        public                   Material            InsideMaterial;
        public                   Material            OutsideMaterial;
        [HideInInspector] public FractureCache       Root;
        [HideInInspector] public FractureCache       Parent;
        [HideInInspector] public List<FractureCache> Children = new();
        public                   ulong               StableHash;
        
        protected override void BakeToBlobData(ref FractureData data, ref BlobBuilder blobBuilder)
        {
            data = new FractureData
            {
                UseAnchors = UseAnchors,
            };
        }
        
        public void Add(Mesh mesh, Material insideMaterial, Material outsideMaterial)
        {
            var fracture = CreateInstance<FractureCache>();
            fracture.Root            = Root;
            fracture.Mesh            = mesh;
            fracture.InsideMaterial  = insideMaterial;
            fracture.OutsideMaterial = outsideMaterial;
            fracture.Parent          = this;
            fracture.name            = mesh.name;
            AssetDatabase.AddObjectToAsset(fracture, this);
            Children.Add(fracture);
        }

        public void Clear()
        {
            for (var index = 0; index < Children.Count; index++)
            {
                var fracture = Children[index];
                fracture.Clear();
                
                DestroyImmediate(fracture.Mesh, true);
                DestroyImmediate(fracture, true);
                //AssetDatabase.RemoveObjectFromAsset(fracture.Mesh);
                //AssetDatabase.RemoveObjectFromAsset(fracture);
            }

            Children.Clear();
            AssetDatabase.SaveAssets();
        }
        
        public FractureCache GetRoot()
        {
            return Root ?? this;
        }
        
    }
    #endif
}