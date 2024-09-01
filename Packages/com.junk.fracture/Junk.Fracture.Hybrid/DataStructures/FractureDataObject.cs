using Junk.Entities.Hybrid;
using Unity.Entities;

namespace Junk.Fracture.Hybrid
{
    #if UNITY_EDITOR
    public class FractureDataObject : BakedScriptableObject<FractureData>
    {
        public bool UseAnchors;
        
        protected override void BakeToBlobData(ref FractureData data, ref BlobBuilder blobBuilder)
        {
            data = new FractureData
            {
                UseAnchors = UseAnchors,
            };
        }
    }
    #endif
}