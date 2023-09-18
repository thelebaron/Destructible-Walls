using System.Collections;
using System.Collections.Generic;
using Junk.Math;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using CyberJunk.Procedural;
using Random = UnityEngine.Random;

/// <summary>
/// In editor script for testing procgen algorithms. Not for use at runtime.
/// </summary>
[ExecuteAlways]
public class RandomSeedTester : MonoBehaviour
{
    public int    seed;
    public float3 grid0;
    public float3 grid1;
    
    // debugging int value
    public int   seedA;
    public int   seedB;
    // debugging float value
    public float fseedA;
    public float fseedB;


    public GameObject       prefab;
    public List<GameObject> spawned = new List<GameObject>();
    
    public bool spawn;
    public bool randomSeed;
    public bool clear;

    public float threshold = 0.0155f;

    void Spawn()
    {
        if (randomSeed)
            seed = UnityEngine.Random.Range(1, 12345);
        
        uint seedvalue = (uint)seed;
        seed = (int)seedvalue;
        if(prefab==null)
            return;
        int limit = 10;
        Clear();

        
        for (int x = 0; x < limit; x++)
        {
            for (int y = 0; y < limit; y++)
            {
                for (int z = 0; z < limit; z++)
                {
                    var num  = Rules.FloatRange(new float3(x,y,z), seed);
                    var rand = Random.Range(0, 1f);

                    if (num > threshold && rand>0.9f)
                    {
                        var go = Object.Instantiate(prefab);
                        go.transform.position = new float3(x, y, z);
                        spawned.Add(go);
                    }
                }
            }
        }
    }
    
    void Clear()
    {
        for (int i = 0; i < spawned.Count; i++)
        {
            Object.DestroyImmediate(spawned[i]);
        }
        spawned.Clear();
    }
    
    void Update()
    {
        seedA = SeedPosition(grid0,seed);
        seedB = SeedPosition(grid1,seed);
        
        
        fseedA = Rules.FloatRange(grid0,seed);
        fseedB = Rules.FloatRange(grid1,seed);

        if (spawn)
            Spawn();
        spawn = false;
        if (clear)
            Clear();
        clear = false;
    }
    
            
    private static int SeedPosition(float3 position, int seed)
    {
        var x       = position * seed;
        int hashPos = (int) math.hash(new int3(math.floor(x)));

        return hashPos;
    }
    
    /// <summary>
    /// Gets a float number usually between 0.001f and 0.019f
    /// </summary>
    /// <param name="position">position to use</param>
    /// <param name="seed">unique seed</param>
    /// <returns></returns>
    private static float FSeedPosition(float3 position, int seed)
    {
        var x       = position * seed + new float3(0.01f,0.01f,0.01f); // if the value is a whole number, offset slightly as whole numbers cause the result to be zero(not useful)
        int hashPos = (int) math.hash(new int3(math.floor(x)));

        var y = noise.cnoise(x);
        //var nansafe = maths.notnan(y);
        //y = math.abs(y);
        return y;
    }
}
