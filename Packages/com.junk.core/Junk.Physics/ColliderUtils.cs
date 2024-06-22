using Unity.Assertions;
using Unity.Burst;
using Unity.Entities;
using Unity.Physics;

namespace Junk.Physics
{
    public class ColliderUtils
    {
        [BurstCompile]
        public static bool IsTrigger(BlobAssetReference<Collider> collider, ColliderKey key)
        {
            bool bIsTrigger = false;
            unsafe
            {
                var c = (Collider*)collider.GetUnsafePtr();
                {
                    var cc = ((ConvexCollider*)c);
                    if (cc->CollisionType != CollisionType.Convex)
                    {
                        c->GetLeaf(key, out ChildCollider child);
                        cc = (ConvexCollider*)child.Collider;
                        Assert.IsTrue(cc->CollisionType == CollisionType.Convex);
                    }
                    bIsTrigger = cc -> Material.CollisionResponse == CollisionResponsePolicy.RaiseTriggerEvents;
                }
            }
            return bIsTrigger;
        }
    }
}