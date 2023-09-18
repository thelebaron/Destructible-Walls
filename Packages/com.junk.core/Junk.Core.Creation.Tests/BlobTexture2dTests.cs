
using System.IO;
using NUnit.Framework;
using Junk.Math;
using Unity.Entities.Serialization;
using UnityEditor;
using UnityEngine;
using File = UnityEngine.Windows.File;

namespace Junk.Core.Tests
{
    public class BlobTexture2dTests
    {
        public Texture2D texture;
        
        [SetUp]
        public void Setup()
        {
            texture = new Texture2D(256,256);
            
            for (int y = 0; y < texture.height; y++)
            {
                for (int x = 0; x < texture.width; x++)
                {
                    Color color = ((x & y) != 0 ? Color.white : Color.gray);
                    texture.SetPixel(x, y, RandomMath.RandomColor());
                }
            }
            texture.Apply();
        }

        [TearDown]
        protected void TearDown()
        {
            UnityEngine.Object.DestroyImmediate(texture);
        }
        
        [Test]
        public void TestSerializeTextureToTga()
        {
            //Sprite    itemBGSprite = Resources.Load<Sprite>( "_Defaults/Item Images/_Background" );
            //Texture2D itemBGTex    = itemBGSprite.texture;
            //byte[]    itemBGBytes  = itemBGTex.EncodeToPNG();
            //File.WriteAllBytes( formattedCampaignPath + "/Item Images/Background.png" , itemBGBytes );
            
            // Encode the bytes in TGA format
            byte[] bytes = texture.EncodeToTGA();
            
            string path = "Packages/com.junk.core/Junk.Core.Creation.Tests//DataAssets/SavedTexture.tga";
            
            //if (File.Exists(path))
            //    File.Delete(path);

            path = Application.persistentDataPath + "/SavedTexture.tga";
            //var path2 = Application.streamingAssetsPath + "/SavedTexture2.tga";
            //AssetDatabase.CreateAsset(texture, path2);
            // Write the returned byte array to a file in the project folder
            UnityEngine.Windows.File.WriteAllBytes(path, bytes);
            Assert.IsTrue(File.Exists(path));
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