
using System.IO;
using Junk.Core.Creation;
using NUnit.Framework;
using Junk.Math;
using Unity.Entities.Serialization;
using UnityEditor;
using UnityEngine;
using File = UnityEngine.Windows.File;

namespace Junk.Core.Tests
{
    public class ColourTests
    {
        
        [SetUp]
        public void Setup()
        {
            
        }

        [TearDown]
        protected void TearDown()
        {
            
        }
        
        [Test]
        public void ColorDataIntegrity()
        {
            var color = new Color();
            color.r = Random.value;
            color.g = Random.value;
            color.b = Random.value;
            color.a = Random.value;

            var col = (colour) color;
            
            Assert.AreEqual(color.r, col.r);
            Assert.AreEqual(color.g, col.g);
            Assert.AreEqual(color.b, col.b);
            Assert.AreEqual(color.a, col.a);
            
            
            //Assert.AreEqual(color, col);


            //File.WriteAllBytes(path, bytes);
        }
        
        /*private void Start()
        {
            Setup();
            var filter = gameObject.AddComponent<MeshFilter>();
            var meshRenderer = gameObject.AddComponent<MeshRenderer>();
            
            meshRenderer.sharedMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit"));
            meshRenderer.sharedMaterial.mainTexture = texture;
            
            var meshobject = GameObject.CreatePrimitive(PrimitiveType.Cube);
            var mesh = meshobject.GetComponent<MeshFilter>().sharedMesh;
            filter.mesh = mesh;
            
            // cleanup
            DestroyImmediate(meshobject);
        }*/
        
        
    }
}