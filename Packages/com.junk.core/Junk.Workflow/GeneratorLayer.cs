using System;
using System.Collections.Generic;
using Junk.Procedural;
using UnityEngine;

namespace Junk.Workflow
{
    [CreateAssetMenu(fileName = "GeneratorLayer", menuName = "ProcGen/Generator Layer", order = 0)]
    public class GeneratorLayer : ScriptableObject
    {
        public List<PrefabData> Models;

        public void Refresh()
        {
            if(Models==null)
                return;
            
            for (int i = 0; i < Models.Count; i++)
            {
                var go = Models[i].Prefab;
                var data = go.GetComponent<ProceduralEntityData>();

                if (data == null)
                {
                    Debug.Log("Missing ProceduralEntityData on "+ go.name);
                    continue;
                }
                
                Models[i].ChanceToSpawn   = data.ChanceToSpawn;
                Models[i].DisableSpawn = data.DisableSpawn;
            }
        }
    }

    [Serializable]
    public class PrefabData
    {
        public GameObject Prefab;
        public float      ChanceToSpawn;
        public bool       DisableSpawn;
    }
}
