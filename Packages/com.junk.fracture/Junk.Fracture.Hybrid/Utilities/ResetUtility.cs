using UnityEngine;

namespace Junk.Fracture.Hybrid
{
    public static class ResetUtility 
    {
        public static void Reset(GameObject gameObject)
        {
            var m_Children = gameObject.GetComponentsInChildren<Transform>();
                
            for (int i = 0; i < m_Children.Length; i++)
            {
                if(i==0)
                    continue;
                UnityEngine.Object.DestroyImmediate(m_Children[i].gameObject);
            }

            m_Children = null;
        }
    }
}