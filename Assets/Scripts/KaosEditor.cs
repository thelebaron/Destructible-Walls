using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
#if UNITY_EDITOR
using kaos;
using thelebaron.CustomEditor;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

public partial class KaosEditor : EditorBase
{
    #region FIELDS

    private FractureData data;
    
    private GameObject fractureObject;
    
    private ObjectField prefabField;
    private ObjectField meshField;
    private GameObject Prefab => (GameObject) prefabField.value;
    private Mesh PrefabMesh { get; set; }

    private ObjectField materialInsideField;
    private ObjectField materialOutideField;
    private Toggle postprocess;

    private Label distanceLabel;
    private Slider distancePreviewSlider;
    private SliderInt fractureNestingSlider;
    //private UnityEditor.UIElements. fractureCount;
    
    private System.Random systemRandom;
    private IntegerField seed;
    private float totalMass;
    private FloatField density;

    // Voronoi fields
    private IntegerField fractureCount;
    // Cluster fields
    private IntegerField Clusters; //5// lowercase
    private IntegerField SitesPerCluster; //5// lowercase
    private FloatField ClusterRadius; //1 // lowercase

    private Vector3IntField slices;
    private Slider slicesOffset;
    private Slider slicesAngle;
    private FloatField slicesAmplitude;
    private FloatField slicesFrequency;
    private IntegerField slicesOctave;
    private IntegerField slicesSurfaceResolution;
    
    private List<List<NodeInfoBehaviour>> nestedNodes;
    private Preferences preferences;

    #endregion
    
    
    private bool placementTypeDetected;
    private bool rotationTypeDetected;

    public static List<GameObject> s_spawnedList = new List<GameObject>();
    // Raycast Properties
    private IntegerField iterations;
    //private Vector3Field spread;
    private FloatField prefabScale;

    private EnumField fractureType;// = new EnumField();
    
    [MenuItem("Tools/KAOS _%K")] // ctrl shift Open _%#T
    public static void ShowWindow()
    {
        // Opens the window, otherwise focuses it if it’s already open.
        var window = GetWindow<KaosEditor>();
        // Adds a title to the window.
        window.titleContent = new GUIContent("Kaos Editor");
        // Sets a minimum size to the window.
        window.minSize = new Vector2(315, 370);
    }

    private void OnEnable()
    {
        SceneView.duringSceneGui += OnScene;
        
        var children = rootVisualElement.Children();
        var child = children.FirstOrDefault();

        rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("KaosEditorMain_Style"));

        // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
        var quickToolVisualTree = Resources.Load<VisualTreeAsset>("KaosEditorMain");
        quickToolVisualTree.CloneTree(rootVisualElement);
        
        // Queries all the buttons (via type) in our root and passes them
        // in the SetupButton method.
        rootVisualElement.Query<Button>().ForEach(Setup_Button);

        var objects = rootVisualElement.Query<ObjectField>();
        objects.ForEach(Setup_ObjectField);
        
        rootVisualElement.Query<FloatField>().ForEach(Setup_FloatFields);
        var intfields = rootVisualElement.Query<IntegerField>();
        intfields.ForEach(Setup_IntFields);
        
        /*
        rootVisualElement.Q<PropertyField>("ArrayList").Bind(new SerializedObject(this));
        */
        
        RegisterCallbacks(rootVisualElement);

        // Load saved settings
        preferences = Preferences.Default();
        preferences = kaos.Serialization.Load();
        kaos.Serialization.Save(preferences);
        OnRandomSeed();
        TryGetMaterials();
    }

    private void OnDisable()
    {
        Save();
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= OnScene;
    }

    private void OnExport()
    {
        throw new NotImplementedException();
    }

    private void OnReset()
    {
        DestroyImmediate(fractureObject);
        /*prefabField.value = null;
        meshField.value = null;*/
    }
    
    private static void OnScene(SceneView sceneview)
    {

        EditorGUI.BeginChangeCheck();

        Handles.BeginGUI();
            
        Handles.Button (new Vector3 (500.0f, 0, 500.0f), Quaternion.LookRotation(Vector3.up), 55.0f, 0.0f, Handles.RectangleHandleCap);
            
        Handles.DrawLine(SceneCameraHitPosition, SceneCameraHitPosition + Vector3.up);
        
        
        Handles.EndGUI();
        DrawBigBrushGizmo();
        Handles.Button (new Vector3 (100.0f, 0, 100.0f), Quaternion.LookRotation(Vector3.up), 1.0f, 0.0f, Handles.RectangleHandleCap);

        //Draws the brush circle


        EditorGUI.EndChangeCheck();
    }
    
    private static void DrawBigBrushGizmo()
    {
        var lineEndUpPos = SceneCameraHitPosition + Vector3.up;
        var surfaceNormal = SceneCameraRaycastHit.normal;
        if (surfaceNormal.Equals(Vector3.zero))
            surfaceNormal = Vector3.up;
        
        Handles.color = Color.red;
        Handles.DrawLine(SceneCameraHitPosition, SceneCameraHitPosition + surfaceNormal);
        Handles.color = Color.green;
        Handles.CircleHandleCap(0, SceneCameraHitPosition, Quaternion.LookRotation(surfaceNormal, Vector3.up), 1f, EventType.Repaint);


    }
    
    private void OnGUI()
    {
        EditorGUI.BeginChangeCheck();
        //Debug.Log("OnSceneGUI");

        Handles.BeginGUI();
        //Debug.Log("BeginGUI");
        
        Handles.color = Color.red;
        
        Handles.DrawWireArc(SceneCameraPosition, Vector3.up, -Vector3.right, 180, 1);
        //Handles.SetCamera(Camera.current);
        
        Handles.EndGUI();


        EditorGUI.EndChangeCheck();
    }

    
    private enum FractureType
    {
        Voronoi,
        Clustered,
        Slicing,
        SkinnedDISABLED,
        PlaneDISABLED,
        CutoutDISABLED
    }
    
}
#endif