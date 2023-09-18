using System.IO;
using Junk.Core.Creation;
using NUnit.Framework;
using Unity.Entities;
using Unity.Entities.Serialization;
using UnityEngine;
using UnityEngine.Rendering;

namespace Junk.Core.Tests
{
    
    public class DiskEntityTests
    {
        private GameObject                   primitive;
        private Mesh                         referenceMesh;
        private BlobAssetReference<BlobMesh> meshBlobData;

        [SetUp]
        public void Setup()
        {
            
            
        }

        [TearDown]
        protected void TearDown()
        {
            
        }
        
        
        [Test]
        public void DiskEntityValidation()
        {
            var entity = new Entity();
            entity.Index = Random.Range(0, int.MaxValue);
            entity.Version = Random.Range(0, int.MaxValue);
            
            var diskEntity1 = (DiskEntity)entity;
            Assert.AreEqual(entity.Index, diskEntity1.Index);
            Assert.AreEqual(entity.Version, diskEntity1.Version);
            
            var entity1 = (Entity)diskEntity1;
            Assert.AreEqual(entity.Index, entity1.Index);
            Assert.AreEqual(entity.Version, entity1.Version);
            
        }

        const int kVersion          = 51;
        const int kIncorrectVersion = 13;
        
 
        [Test]
        public void DiskEntitySerialization()
        {
            string path = "Packages/com.junk.core/Junk.Core.Creation.Tests/TestData/entity.diskentity";
            if (File.Exists(path))
                File.Delete(path);

            if(!Directory.Exists("Packages/com.junk.core/Junk.Core.Creation.Tests/TestData/"))
                Directory.CreateDirectory("Packages/com.junk.core/Junk.Core.Creation.Tests/TestData/");
            var entity = new Entity();
            entity.Index   = 67;
            entity.Version = 1;
            
            var diskEntity = (DiskEntity)entity;
            
            try
            {
                var diskEntityJson = JsonUtility.ToJson(diskEntity);
                using (var stream = File.Open(path, FileMode.Create))
                {
                    var writer = new StreamWriter(stream);
                    writer.Write(diskEntityJson);
                    writer.Flush();
                }
                
                using (var stream = File.Open(path, FileMode.Open))
                {
                    var reader = new StreamReader(stream);
                    var diskEntityJson2 = reader.ReadToEnd();
                    Assert.AreEqual(diskEntityJson, diskEntityJson2);
                }
                
            }
            finally
            {
                //if (File.Exists(path))
                    //File.Delete(path);
            }
        }
    }
}