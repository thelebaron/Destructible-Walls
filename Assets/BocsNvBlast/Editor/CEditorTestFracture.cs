using UnityEditor;
using UnityEngine;

public class CEditorTestFracture : EditorWindow
{
    private enum FractureTypes
    {
        Voronoi,
        Clustered,
        Slicing,
        Skinned,
        Plane,
        Cutout
    }

    [MenuItem("Test/Fracture")]
    public static void OpenEditor()
    {
        EditorWindow.GetWindow<CEditorTestFracture>("Fracture");
    }

    private FractureTypes fractureType = FractureTypes.Voronoi;

    public GameObject point;
    public GameObject source;
    public Material insideMaterial;
    public bool islands = false;
    public bool previewColliders = false;
    public float previewDistance = 0.5f;
    public int totalChunks = 5;
    public int seed = 0;

    //TODO: serialize
    //public SlicingConfiguration sliceConf;

    Vector3Int slices = Vector3Int.one;
    float offset_variations = 0;
    float angle_variations = 0;
    float amplitude = 0;
    float frequency = 1;
    int octaveNumber = 1;
    int surfaceResolution = 2;

    public int clusters = 5;
    public int sitesPerCluster = 5;
    public float clusterRadius = 1;

    private void OnEnable()
    {
        point = (GameObject)Resources.Load("Point");
    }

    private void OnSelectionChange()
    {
        Repaint();
    }

    protected void OnGUI()
    {
        if (Application.isPlaying)
        {
            GUILayout.Label("PLAY MODE ACTIVE", GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinHeight(32));
            return;
        }

        GUILayout.Label("OPTIONS", GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinHeight(32));
        if (GUILayout.Button("Clean Up Objects")) CleanUp();

        GUILayout.Space(20);
        source = EditorGUILayout.ObjectField("Source", source, typeof(GameObject), true) as GameObject;

        if (Selection.activeGameObject != null)
        {
            //hack to not select preview chunks OR Points OR Destructible :)
            if (Selection.activeGameObject.GetComponent<ChunkInfo>() == null && Selection.activeGameObject.hideFlags != HideFlags.NotEditable && Selection.activeGameObject.GetComponent<Destructible>() == null)
            {
                if (Selection.activeGameObject.GetComponent<MeshFilter>() != null) source = Selection.activeGameObject;
                if (Selection.activeGameObject.GetComponentInChildren<SkinnedMeshRenderer>() != null)
                {
                    source = Selection.activeGameObject.GetComponentInChildren<SkinnedMeshRenderer>().gameObject;
                }
            }
        }

        if (!source) return;

        insideMaterial = (Material)EditorGUILayout.ObjectField("Inside Material", insideMaterial, typeof(Material), false);

        if (!insideMaterial) return;

        fractureType = (FractureTypes)EditorGUILayout.EnumPopup("Fracture Type", fractureType);

        EditorGUILayout.BeginHorizontal();
        islands = EditorGUILayout.Toggle("Islands", islands);
        previewColliders = EditorGUILayout.Toggle("Preview Colliders", previewColliders);
        EditorGUILayout.EndHorizontal();

        seed = EditorGUILayout.IntSlider("Seed", seed, 0, 25);

        EditorGUI.BeginChangeCheck();
        previewDistance = EditorGUILayout.Slider("Preview", previewDistance, 0, 5);
        if (EditorGUI.EndChangeCheck())
        {
            UpdatePreview();
        }

        bool canCreate = false;

        if (fractureType == FractureTypes.Voronoi) canCreate = GUI_Voronoi();
        if (fractureType == FractureTypes.Clustered) canCreate = GUI_Clustered();
        if (fractureType == FractureTypes.Slicing) canCreate = GUI_Slicing();
        if (fractureType == FractureTypes.Skinned) canCreate = GUI_Skinned();
        if (fractureType == FractureTypes.Plane) canCreate = GUI_Plane();
        if (fractureType == FractureTypes.Cutout) canCreate = GUI_Cutout();

        if (canCreate)
        {
            GUILayout.BeginHorizontal();
            if (GUILayout.Button("Preview Chunks"))
            {
                _createPreview(false);
            }

            if (GUILayout.Button("Create Prefab"))
            {
                _createPreview(true);
            }
            GUILayout.EndHorizontal();
        }
    }

    private void _createPreview(bool makePrefab)
    {
        NvBlastExtUnity.setSeed(seed);

        CleanUp();

        GameObject cs = new GameObject("CHUNKS");
        cs.transform.position = Vector3.zero;
        cs.transform.rotation = Quaternion.identity;
        cs.transform.localScale = Vector3.one;

        Mesh ms = null;

        Material[] mats = new Material[2];
        mats[1] = insideMaterial;

        MeshFilter mf = source.GetComponent<MeshFilter>();
        SkinnedMeshRenderer smr = source.GetComponent<SkinnedMeshRenderer>();

        if (mf != null)
        {
            mats[0] = source.GetComponent<MeshRenderer>().sharedMaterial;
            ms = source.GetComponent<MeshFilter>().sharedMesh;
        }
        if (smr != null)
        {
            mats[0] = smr.sharedMaterial;
            smr.gameObject.transform.position = Vector3.zero;
            smr.gameObject.transform.rotation = Quaternion.identity;
            smr.gameObject.transform.localScale = Vector3.one;
            ms = new Mesh();
            smr.BakeMesh(ms);
            //ms = smr.sharedMesh;
        }

        if (ms == null) return;

        NvMesh mymesh = new NvMesh(ms.vertices, ms.normals, ms.uv, ms.vertexCount, ms.GetIndices(0), (int)ms.GetIndexCount(0));

        //NvMeshCleaner cleaner = new NvMeshCleaner();
        //cleaner.cleanMesh(mymesh);

        NvFractureTool fractureTool = new NvFractureTool();
        fractureTool.setRemoveIslands(islands);
        fractureTool.setSourceMesh(mymesh);

        if (fractureType == FractureTypes.Voronoi) _Voronoi(fractureTool, mymesh);
        if (fractureType == FractureTypes.Clustered) _Clustered(fractureTool, mymesh);
        if (fractureType == FractureTypes.Slicing) _Slicing(fractureTool, mymesh);
        if (fractureType == FractureTypes.Skinned) _Skinned(fractureTool, mymesh);
        if (fractureType == FractureTypes.Plane) _Plane(fractureTool, mymesh);
        if (fractureType == FractureTypes.Cutout) _Cutout(fractureTool, mymesh);

        fractureTool.finalizeFracturing();

        NvLogger.Log("Chunk Count: " + fractureTool.getChunkCount());

        if (makePrefab)
        {
            if (!AssetDatabase.IsValidFolder("Assets/NvBlast Prefabs")) AssetDatabase.CreateFolder("Assets", "NvBlast Prefabs");
            if (!AssetDatabase.IsValidFolder("Assets/NvBlast Prefabs/Meshes")) AssetDatabase.CreateFolder("Assets/NvBlast Prefabs", "Meshes");
            if (!AssetDatabase.IsValidFolder("Assets/NvBlast Prefabs/Fractured")) AssetDatabase.CreateFolder("Assets/NvBlast Prefabs", "Fractured");

            FileUtil.DeleteFileOrDirectory("Assets/NvBlast Prefabs/Meshes/" + source.name);
            AssetDatabase.Refresh();
            AssetDatabase.CreateFolder("Assets/NvBlast Prefabs/Meshes", source.name);
        }

        for (int i = 1; i < fractureTool.getChunkCount(); i++)
        {
            GameObject ck = new GameObject("Chunk" + i);
            ck.transform.parent = cs.transform;
            ck.transform.position = Vector3.zero;
            ck.transform.rotation = Quaternion.identity;

            MeshFilter ckmf = ck.AddComponent<MeshFilter>();
            MeshRenderer ckmr = ck.AddComponent<MeshRenderer>();

            ckmr.sharedMaterials = mats;

            NvMesh outside = fractureTool.getChunkMesh(i, false);
            NvMesh inside = fractureTool.getChunkMesh(i, true);

            Mesh m = outside.toUnityMesh();
            m.subMeshCount = 2;
            m.SetIndices(inside.getIndexes(), MeshTopology.Triangles, 1);
            ckmf.sharedMesh = m;

            if (makePrefab)
            {
                AssetDatabase.CreateAsset(m, "Assets/NvBlast Prefabs/Meshes/" + source.name + "/Chunk" + i + ".asset");
            }

            if (!makePrefab) ck.AddComponent<ChunkInfo>();

            if (makePrefab || previewColliders)
            {
                ck.AddComponent<Rigidbody>();
                MeshCollider mc = ck.AddComponent<MeshCollider>();
                mc.inflateMesh = true;
                mc.convex = true;
            }
        }

        if (makePrefab)
        {
            GameObject p = PrefabUtility.CreatePrefab("Assets/NvBlast Prefabs/Fractured/" + source.name + "_fractured.prefab", cs);

            GameObject fo;

            bool skinnedMesh = false;
            if (source.GetComponent<SkinnedMeshRenderer>() != null) skinnedMesh = true;

            if (skinnedMesh)
                fo = Instantiate(source.transform.root.gameObject);
            else
                fo = Instantiate(source);

            Destructible d = fo.AddComponent<Destructible>();
            d.fracturedPrefab = p;

            bool hasCollider = false;
            if (fo.GetComponent<MeshCollider>() != null) hasCollider = true;
            if (fo.GetComponent<BoxCollider>() != null) hasCollider = true;
            if (fo.GetComponent<SphereCollider>() != null) hasCollider = true;
            if (fo.GetComponent<CapsuleCollider>() != null) hasCollider = true;

            if (!hasCollider)
            {
                BoxCollider bc = fo.AddComponent<BoxCollider>();
                if (skinnedMesh)
                {
                    Bounds b = source.GetComponent<SkinnedMeshRenderer>().bounds;
                    bc.center = new Vector3(0,.5f,0);
                    bc.size = b.size;
                }
            }

            PrefabUtility.CreatePrefab("Assets/NvBlast Prefabs/" + source.name + ".prefab", fo);
            DestroyImmediate(fo);
        }

        cs.transform.rotation = source.transform.rotation;

        UpdatePreview();
    }

    private void _Cutout(NvFractureTool fractureTool, NvMesh mesh)
    {
    }

    private void _Plane(NvFractureTool fractureTool, NvMesh mesh)
    {
    }

    private void _Skinned(NvFractureTool fractureTool, NvMesh mesh)
    {
        SkinnedMeshRenderer smr = source.GetComponent<SkinnedMeshRenderer>();
        NvVoronoiSitesGenerator sites = new NvVoronoiSitesGenerator(mesh);
        sites.boneSiteGeneration(smr);
        fractureTool.voronoiFracturing(0, sites);
    }

    private void _Slicing(NvFractureTool fractureTool, NvMesh mesh)
    {
        SlicingConfiguration conf = new SlicingConfiguration();
        conf.slices = slices;
        conf.offset_variations = offset_variations;
        conf.angle_variations = angle_variations;

        conf.noise.amplitude = amplitude;
        conf.noise.frequency = frequency;
        conf.noise.octaveNumber = octaveNumber;
        conf.noise.surfaceResolution = surfaceResolution;

        fractureTool.slicing(0, conf, false);
    }

    private void _Clustered(NvFractureTool fractureTool, NvMesh mesh)
    {
        NvVoronoiSitesGenerator sites = new NvVoronoiSitesGenerator(mesh);
        sites.clusteredSitesGeneration(clusters, sitesPerCluster, clusterRadius);
        fractureTool.voronoiFracturing(0, sites);
    }

    private void _Voronoi(NvFractureTool fractureTool, NvMesh mesh)
    {
        NvVoronoiSitesGenerator sites = new NvVoronoiSitesGenerator(mesh);
        sites.uniformlyGenerateSitesInMesh(totalChunks);
        fractureTool.voronoiFracturing(0, sites);
    }

    private bool GUI_Voronoi()
    {
        GUILayout.Space(20);
        GUILayout.Label("VORONOI FRACTURE", GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinHeight(32));

        totalChunks = EditorGUILayout.IntSlider("Total Chunks", totalChunks, 2, 100);

        if (GUILayout.Button("Visualize Points"))
        {
            _Visualize();
        }
        return true;
    }

    private bool GUI_Clustered()
    {
        GUILayout.Space(20);
        GUILayout.Label("CLUSTERED VORONOI FRACTURE", GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinHeight(32));

        clusters = EditorGUILayout.IntField("Clusters", clusters);
        sitesPerCluster = EditorGUILayout.IntField("Sites", sitesPerCluster);
        clusterRadius = EditorGUILayout.FloatField("Radius", clusterRadius);

        if (GUILayout.Button("Visualize Points"))
        {
            _Visualize();
        }

        return true;
    }

    private bool GUI_Skinned()
    {
        GUILayout.Space(20);
        GUILayout.Label("SKINNED MESH VORONOI FRACTURE", GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinHeight(32));

        if (source.GetComponent<SkinnedMeshRenderer>() == null)
        {
            EditorGUILayout.HelpBox("Skinned Mesh Not Selected", MessageType.Error);
            return false;
        }

        if (source.transform.root.position != Vector3.zero)
        {
            EditorGUILayout.HelpBox("Root must be at 0,0,0 for Skinned Meshes", MessageType.Info);
            if (GUILayout.Button("FIX"))
            {
                source.transform.root.position = Vector3.zero;
                source.transform.root.rotation = Quaternion.identity;
                source.transform.root.localScale = Vector3.one;
            }

            return false;
        }

        if (GUILayout.Button("Visualize Points"))
        {
            _Visualize();
        }

        return true;
    }

    private bool GUI_Slicing()
    {
        GUILayout.Space(20);
        GUILayout.Label("SLICING FRACTURE", GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinHeight(32));

        slices = EditorGUILayout.Vector3IntField("Slices", slices);
        offset_variations = EditorGUILayout.Slider("Offset", offset_variations, 0, 1);
        angle_variations = EditorGUILayout.Slider("Angle", angle_variations, 0, 1);

        GUILayout.BeginHorizontal();
        amplitude = EditorGUILayout.FloatField("Amplitude", amplitude);
        frequency = EditorGUILayout.FloatField("Frequency", frequency);
        GUILayout.EndHorizontal();

        GUILayout.BeginHorizontal();
        octaveNumber = EditorGUILayout.IntField("Octave", octaveNumber);
        surfaceResolution = EditorGUILayout.IntField("Resolution", surfaceResolution);
        GUILayout.EndHorizontal();

        return true;
    }

    private bool GUI_Plane()
    {
        GUILayout.Space(20);
        GUILayout.Label("PLANE FRACTURE", GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinHeight(32));

        GUILayout.Label("Coming Soon...");
        return false;
    }

    private bool GUI_Cutout()
    {
        GUILayout.Space(20);
        GUILayout.Label("CUTOUT FRACTURE", GUI.skin.box, GUILayout.ExpandWidth(true), GUILayout.MinHeight(32));

        GUILayout.Label("Coming Soon...");
        return false;
    }

    private void _Visualize()
    {
        NvBlastExtUnity.setSeed(seed);

        CleanUp();
        if (source == null) return;

        GameObject ps = new GameObject("POINTS");
        ps.transform.position = Vector3.zero;
        ps.transform.rotation = Quaternion.identity;
        ps.transform.localScale = Vector3.one;

        Mesh ms = null;

        MeshFilter mf = source.GetComponent<MeshFilter>();
        SkinnedMeshRenderer smr = source.GetComponent<SkinnedMeshRenderer>();

        if (mf != null)
        {
            ms = source.GetComponent<MeshFilter>().sharedMesh;
        }
        if (smr != null)
        {
            smr.gameObject.transform.position = Vector3.zero;
            smr.gameObject.transform.rotation = Quaternion.identity;
            smr.gameObject.transform.localScale = Vector3.one;
            ms = new Mesh();
            smr.BakeMesh(ms);
            //ms = smr.sharedMesh;
        }

        if (ms == null) return;

        NvMesh mymesh = new NvMesh(ms.vertices, ms.normals, ms.uv, ms.vertexCount, ms.GetIndices(0), (int)ms.GetIndexCount(0));

        NvVoronoiSitesGenerator sites = new NvVoronoiSitesGenerator(mymesh);
        if (fractureType == FractureTypes.Voronoi) sites.uniformlyGenerateSitesInMesh(totalChunks);
        if (fractureType == FractureTypes.Clustered) sites.clusteredSitesGeneration(clusters, sitesPerCluster, clusterRadius);
        if (fractureType == FractureTypes.Skinned) sites.boneSiteGeneration(smr);

        Vector3[] vs = sites.getSites();

        for (int i = 0; i < vs.Length; i++)
        {
            GameObject po = Instantiate(point, vs[i], Quaternion.identity, ps.transform);
            po.hideFlags = HideFlags.NotEditable;
        }

        ps.transform.rotation = source.transform.rotation;
        ps.transform.position = source.transform.position;
    }

    private void CleanUp()
    {
        GameObject.DestroyImmediate(GameObject.Find("POINTS"));
        GameObject.DestroyImmediate(GameObject.Find("CHUNKS"));
    }

    private void UpdatePreview()
    {
        GameObject cs = GameObject.Find("CHUNKS");
        if (cs == null) return;

        Transform[] ts = cs.transform.GetComponentsInChildren<Transform>();

        foreach (Transform t in ts)
        {
            ChunkInfo ci = t.GetComponent<ChunkInfo>();
            if (ci != null)
            {
                ci.UpdatePreview(previewDistance);
            }
        }
    }
}