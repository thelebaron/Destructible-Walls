using UnityEditor;
using UnityEngine;

namespace Junk.Entities
{
    [ExecuteAlways]
    public class ScenePrefabAuthoring : MonoBehaviour
    {
        public GameObject SelfPrefab;

#if UNITY_EDITOR
        
        [ContextMenu("Set Prefab")]
        private void OnEnable()
        {
            if (!PrefabUtility.IsPartOfPrefabInstance(gameObject))
            {
                Debug.Log("Selected GameObject is not a Prefab Instance");
                //return;
            } 
            var thing  = PrefabUtility.GetCorrespondingObjectFromSource(gameObject);

            if (SelfPrefab == null)
            {
                SelfPrefab = thing;
            }
        }
#endif
    }
}