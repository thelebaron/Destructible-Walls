using System.Collections.Generic;
using Junk.Workflow;
using Unity.Mathematics;
using UnityEngine;
using Random = UnityEngine.Random;

namespace CyberJunk.Procedural
{
    [CreateAssetMenu(fileName = "ActivatePhysics", menuName = "ProcGen/Generator", order = 0)]
    public class Generator : ScriptableObject
    {
        public int        Seed = 12345;
        public bool       RandomSeed = true;
        public GameObject Prefab;

        public GeneratorLayer Layer;
        
        
        public List<GameObject> spawned   = new List<GameObject>();
        public float            threshold = 0.9f;
        
        public void Spawn()
        {
            if(Prefab==null)
                return;
            
            if (RandomSeed)
                Seed = UnityEngine.Random.Range(1, 12345);
        
            uint seedvalue = (uint)Seed;
            Seed = (int)seedvalue;
            int limit = 10;
            
            //Clear();

        
            for (int x = 0; x < limit; x++)
            {
                for (int y = 0; y < limit; y++)
                {
                    for (int z = 0; z < limit; z++)
                    {
                        var num  = Rules.FloatRange(new float3(x,y,z), Seed);
                        var rand = Random.Range(0, 1f);

                        //new 
                        if (Layer != null)
                        {
                            for (int i = 0; i < Layer.Models.Count; i++)
                            {
                                rand = Random.Range(0, 1f);
                                if (num > threshold && rand < Layer.Models[i].ChanceToSpawn)
                                {
                                    var go = Object.Instantiate(Layer.Models[i].Prefab);
                                    go.AddComponent<ProceduralInstance>();
                                    go.transform.position = new float3(x, y, z);
                                    //spawned.Add(go);
                                }
                            }
                        }
                        
                        
                        /*
                         // old
                        if (num > threshold && rand>0.9f)
                        {
                            var go = Object.Instantiate(Prefab);
                            go.AddComponent<ProceduralInstance>();
                            go.transform.position = new float3(x, y, z);
                            //spawned.Add(go);
                        }*/
                    }
                }
            }
        }
        
        public void Clear()
        {

            ProceduralInstance[] gameObjects = FindObjectsOfType<ProceduralInstance>();
            
            // Reverse loop to remove items safely from the buffer
            for (var j = gameObjects.Length - 1; j >= 0; j--)
            {
                var go = gameObjects[j].gameObject;
                Object.DestroyImmediate(go);
            }

            spawned.Clear();
        }
    }
    

}