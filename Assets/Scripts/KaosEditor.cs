using System;
using System.Collections.Generic;
using System.Linq;
using Unity.Mathematics;
using Object = System.Object;
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

    private ObjectField meshInputField;
    
    private GameObject root;
    private List<List<NodeInfo>> nestedNodes;
    
    private ObjectField materialInsideField;
    private ObjectField materialOutideField;

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
    private IntegerField Clusters; //5
    private IntegerField SitesPerCluster; //5
    private FloatField ClusterRadius; //1
    public int   clusters        = 5;
    public int   sitesPerCluster = 5;
    public float clusterRadius   = 1;
    
    private KaosPreferences kaosPreferences;

    #endregion
    
    
    private bool placementTypeDetected;
    private bool rotationTypeDetected;
    public List<GameObject> spawnedList = new List<GameObject>();
    public List<GameObject> mergedList = new List<GameObject>();
    private bool spawnOnSelected;

    public static List<GameObject> s_spawnedList = new List<GameObject>();
    // Raycast Properties
    private IntegerField iterations;
    //private Vector3Field spread;
    private FloatField prefabScale;

    private EnumField fractureType;// = new EnumField();
    private EnumField prefabPlacementType;// = new EnumField();
    private EnumField prefabRotationType;// = new EnumField();
    private EnumField prefabScaleType;// = new EnumField();
    private PlacementType placement;

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
        

        
        // Reference to the root of the window.
        //var root = rootVisualElement;
        var children = rootVisualElement.Children();
        var child = children.FirstOrDefault();

        //Debug.Log(root);
        // Associates a stylesheet to our root. Thanks to inheritance, all root’s
        // children will have access to it.
        rootVisualElement.styleSheets.Add(Resources.Load<StyleSheet>("KaosEditorMain_Style"));

        // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
        var quickToolVisualTree = Resources.Load<VisualTreeAsset>("KaosEditorMain");
        quickToolVisualTree.CloneTree(rootVisualElement);


        
        // Queries all the buttons (via type) in our root and passes them
        // in the SetupButton method.
        var toolButtons = rootVisualElement.Query<Button>();
        toolButtons.ForEach(Setup_Button);

        var objects = rootVisualElement.Query<ObjectField>();
        objects.ForEach(Setup_ObjectField);

        //var enums = root.Query<EnumField>();
        //enums.ForEach(Setup_Enums);
        
        var toggles = rootVisualElement.Query<Toggle>();
        toggles.ForEach(Setup_Toggles);
        
        //var vector3s = root.Query<Vector3Field>();
        //vector3s.ForEach(Setup_Vector3Fields);
        
        //var floats = root.Query<FloatField>();
        rootVisualElement.Query<FloatField>().ForEach(Setup_FloatFields);
        var intfields = rootVisualElement.Query<IntegerField>();
        intfields.ForEach(Setup_IntFields);
        
        /*
        rootVisualElement.Q<PropertyField>("ArrayList").Bind(new SerializedObject(this));
        */
        
        //SetupRaycastProperties(rootVisualElement);
        RegisterCallbacks(rootVisualElement);

        // Load saved settings
        var prefs = kaos.KaosSerialization.Load();
        if(prefs == null)
            kaosPreferences = new KaosPreferences();
        
        OnRandomSeed();
        TryGetMaterials();
        //TryGetMesh(); // unsure how to approach for now as it returns obj
        

    }

    private void OnDisable()
    {
        Save();
        
        return;
        /*
        var selection = (GameObject) materialOutideField.value;
        var selectionName = selection.name;
        var array = FindObjectsOfType<GameObject>();
        int counter = 0;
        foreach (var g in array)
        {
            if (g.name == selectionName)
                counter++;
        }

        if (counter > 1)
        {
            Debug.Log("Multiple objects found with same name as selection, not saving.");
            return;
        }*/
        //kaos.KaosEditorSerialization.Save((GameObject)prefabObjectField.value);
    }

    private void OnDestroy()
    {
        SceneView.duringSceneGui -= OnScene;
    }

    /*private void SetupRaycastProperties(VisualElement visualElement)
    {
        var uxmlIterationsField = rootVisualElement.Q<IntegerField>("Iterations");
        iterations = uxmlIterationsField;
        uxmlIterationsField.RegisterCallback<ChangeEvent<IntegerField>>((evt) =>
        {
            iterations = evt.newValue;
        });
        
        var uxmlVector3Field = rootVisualElement.Q<Vector3Field>("Spread");
        spread = uxmlVector3Field;
        // Mirror value of uxml field into the C# field.
        uxmlVector3Field.RegisterCallback<ChangeEvent<Vector3Field>>((evt) =>
        {
            spread = evt.newValue;
        });
        //Debug.Log(spread.value);
    }*/


    private void OnExport()
    {
        throw new NotImplementedException();
    }

    private void OnReset()
    {
        DestroyImmediate(root);
    }

    private void Setup_Toggles(Toggle toggle)
    {
        if (toggle.name == "SpawnOnSelected")
        {
            //toggle.value = Menu.GetChecked(BURST_MENU);
            spawnOnSelected = toggle.value;
            toggle.RegisterValueChangedCallback(OnToggle_SpawnOnSelected);
        }
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

    /*private void SpawnObjects()//ObjectField field)
    {
        var root = ContainerGameObject();
        var prefab = PrefabGameObject();
        
        // Null checks
        if (containerObjectField.value == null || prefabObjectField.value == null)
        {
            Debug.LogWarning("Missing fields");
            return;
        }
        
        var dir = SpreadDirection(spread.value, spread.value, Quaternion.LookRotation(SceneCamera.transform.forward.normalized, Vector3.up));
        dir = dir.normalized * 100;
        
        if(!Physics.Raycast(SceneCamera.transform.position, dir,  out var raycastHit, 100)) 
            return;
        
        var offset = totalMass.value;
        var rotation = prefab.transform.rotation;

        switch (prefabRotationType.value)
        {
            case RotationType.None:
                break;
            case RotationType.NormalForward:
                rotation = Quaternion.LookRotation(raycastHit.normal, Vector3.up);
                break;
            case RotationType.Random:
                rotation = Random.rotation;
                break;
            case RotationType.RandomNormal:
                rotation = Quaternion.LookRotation(raycastHit.normal, Vector3.up);
                var randomEulerAngles = rotation.eulerAngles;
                randomEulerAngles.z = Random.Range(-180f, 180f);
                rotation.eulerAngles = randomEulerAngles;
                break;
            default:
                break;
        }
        
        switch (prefabPlacementType.value)
        {
            case PlacementType.All:
            {
                var spawn = Instantiate(prefab, raycastHit.point + raycastHit.normal.normalized * offset, rotation) as GameObject;
                spawn.name = prefab.name;
                spawn.transform.SetParent(root.transform);
                spawnedList.Add(spawn);
                spawn.transform.localScale *= prefabScale.value;
                
                break;
            }
            case PlacementType.Container:
            {
                var collider = ContainerGameObject().GetComponent<Collider>();
                if(collider == null || raycastHit.collider != collider)
                    return;
                
                var spawn = Instantiate(prefab, raycastHit.point + raycastHit.normal.normalized * offset, rotation) as GameObject;
                spawn.name = prefab.name;
                spawn.transform.SetParent(root.transform);
                spawnedList.Add(spawn);
                spawn.transform.localScale *= prefabScale.value;
                
                break;
            }
            case PlacementType.Selected:
            {
                var gameObject = Selection.activeGameObject;
                var collider = gameObject.GetComponent<Collider>();
                if(gameObject == null || collider == null || raycastHit.collider != collider)
                    return;
                
                var spawn = Instantiate(prefab, raycastHit.point + raycastHit.normal.normalized * offset, rotation) as GameObject;
                spawn.name = prefab.name;
                spawn.transform.SetParent(root.transform);
                spawnedList.Add(spawn);
                spawn.transform.localScale *= prefabScale.value;
                
                break;
            }
            
            default:
                throw new ArgumentOutOfRangeException();
        }
    }*/
    

    #region FinishedMethods
    
    /// <summary>
    /// Callback that registers the bool to whatever the new value selected in the inspector is.
    /// </summary>
    private void OnToggle_SpawnOnSelected(ChangeEvent<bool> evt)
    {
        spawnOnSelected = evt.newValue;
    }
    

    private enum PlacementType
    {
        All,
        Container,
        Selected
    }

    private enum RotationType
    {
        None,
        NormalForward,
        RandomNormal,
        Random
    }
    
    private enum ScaleType
    {
        None,
        RandomUniform,
        RandomNonUniform
    }

    private enum FractureType
    {
        Voronoi,
        Clustered,
        /*Slicing,
        Skinned,
        Plane,
        Cutout*/
    }
    
    /*/// <summary>
   /// Returns the gameobject contained in the inspector container field,
   /// which is used as a container for all spawned prefabs
   /// </summary>
   /// <returns>a GameObject</returns>
   private GameObject ContainerGameObject()
   {
       var go = (GameObject)materialOutideField.value;
       return go;
   }
   
  
   /// <summary>
   /// Returns the gameobject contained in the inspector container field,
   /// which is used to spawn all prefabs
   /// </summary>
   /// <returns>a GameObject</returns>
   private GameObject PrefabGameObject()
   {
       var go = (GameObject)prefabObjectField.value;
       
       return go;
   }*/

    private Material DefaultMaterialHack()
    {
        GameObject primitive = GameObject.CreatePrimitive(PrimitiveType.Plane);
        primitive.active = false;
        primitive.hideFlags = HideFlags.DontSave;
        Material diffuse = primitive.GetComponent<MeshRenderer>().sharedMaterial;
        DestroyImmediate(primitive);
        return diffuse;
    }
    
    private static Vector3 SpreadDirection(Vector3 minSpread, Vector3 maxSpread, quaternion rot)
    {
        var rotatedVector = math.mul(rot, Vector3.forward);
        var dir           = rotatedVector;
            
        var x = UnityEngine.Random.Range(-minSpread.x, maxSpread.x);
        var y = UnityEngine.Random.Range(-minSpread.y, maxSpread.y);
        var z = UnityEngine.Random.Range(-minSpread.z, maxSpread.z);
        dir += new float3(x, y, z);
            
        return dir;
    }
    
    #endregion

    #region SharedMethods



    #endregion
    
    #region BrushMethods



        


    #endregion
}
#endif
