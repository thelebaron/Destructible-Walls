using Unity.Collections;
using Unity.Entities;
using UnityEngine;

namespace Junk.Entities.Hybrid
{
    #if UNITY_EDITOR
    public abstract class BakedScriptableObject<T> : ScriptableObject where T : unmanaged
    {
        public BlobAssetReference<T> BakeToBlob(IBaker baker)
        {
            BlobBuilder builder    = new BlobBuilder(Allocator.Temp);
            ref T       definition = ref builder.ConstructRoot<T>();
    
            BakeToBlobData(ref definition, ref builder);
        
            BlobAssetReference<T> blobReference = builder.CreateBlobAssetReference<T>(Allocator.Persistent);
            baker.AddBlobAsset(ref blobReference, out var hash);
            builder.Dispose();

            baker.DependsOn(this);
        
            return blobReference;
        }
    
        protected abstract void BakeToBlobData(ref T data, ref BlobBuilder blobBuilder);
    }
    #endif
    /*
    // Example usage
    [Serializable]
    public struct MyTestData
    {
        [Header("General")]
        public float Value;

        public static MyTestData Default()
        {
            return new MyTestData
            {
                Value = 1f,
            };
        }
    }

    [CreateAssetMenu(fileName = "NewShipData", menuName = "Game/ShipData")]
    public class MyDataObject : BakedScriptableObject<MyTestData>
    {
        public MyTestData TestData = MyTestData.Default();

        protected override void BakeToBlobData(ref MyTestData testData, ref BlobBuilder blobBuilder)
        {
            testData = TestData;
        }
    }
    */
}