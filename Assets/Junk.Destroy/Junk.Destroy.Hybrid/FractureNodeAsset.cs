using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Junk.Destroy.Hybrid
{
    /// <summary>
    /// An asset that stores a list of fractures
    /// </summary>
    [CreateAssetMenu(fileName = "Fracture Cache", menuName = "Fracturing", order = 0)]
    public class FractureNodeAsset : ScriptableObject
    {
        public                   Mesh                    Mesh;
        public                   Material                InsideMaterial;
        public                   Material                OutsideMaterial;
        [HideInInspector] public FractureNodeAsset       Root;
        [HideInInspector] public FractureNodeAsset       Parent;
        [HideInInspector] public List<FractureNodeAsset> Children = new();
        
        public void Add(Mesh mesh, Material insideMaterial, Material outsideMaterial)
        {
            var fracture = CreateInstance<FractureNodeAsset>();
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
        

        
    }
}