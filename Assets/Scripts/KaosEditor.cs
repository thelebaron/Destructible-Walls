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

    private ObjectField meshField;
    private Mesh originalMesh;
    
    private IntegerField fractureCount;
    
    private ObjectField materialInsideField;
    private ObjectField materialOutideField;

    private Material insideMaterial;
    private Material outsideMaterial;
    
    private System.Random systemRandom;
    private IntegerField seed;
    private float totalMass;
    private FloatField density;

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
        var root = rootVisualElement;
        var children = root.Children();
        var child = children.FirstOrDefault();

        //Debug.Log(root);
        // Associates a stylesheet to our root. Thanks to inheritance, all root’s
        // children will have access to it.
        root.styleSheets.Add(Resources.Load<StyleSheet>("KaosEditorMain_Style"));

        // Loads and clones our VisualTree (eg. our UXML structure) inside the root.
        var quickToolVisualTree = Resources.Load<VisualTreeAsset>("KaosEditorMain");
        quickToolVisualTree.CloneTree(root);

        // Queries all the buttons (via type) in our root and passes them
        // in the SetupButton method.
        var toolButtons = root.Query<Button>();
        toolButtons.ForEach(Setup_Button);

        var objects = root.Query<ObjectField>();
        objects.ForEach(Setup_ObjectField);

        //var enums = root.Query<EnumField>();
        //enums.ForEach(Setup_Enums);
        
        var toggles = root.Query<Toggle>();
        toggles.ForEach(Setup_Toggles);
        
        //var vector3s = root.Query<Vector3Field>();
        //vector3s.ForEach(Setup_Vector3Fields);
        
        //var floats = root.Query<FloatField>();
        root.Query<FloatField>().ForEach(Setup_FloatFields);
        var intfields = root.Query<IntegerField>();
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
        //Debug.Log(prefs);
        //materialOutideField.value = GameObject.Find(prefs.SelectionName);

    }

    private void OnDisable()
    {
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
        throw new NotImplementedException();
    }


    private void MyCallback()
    {
        Debug.Log("doesitwork");
    }
    
    private void RegisterCallbacks(VisualElement visualElement)
    {
        // event object
        // Scale size adjustment field
        var meshfield = visualElement.Q<ObjectField>("OriginalMesh");
        
        meshfield.RegisterCallback<ChangeEvent<ObjectField>>(evt =>
        {
            meshField = evt.newValue;
            MyCallback();
            //Debug.Log(meshfield);
        });
        //meshfield.RegisterCallback<ChangeEvent<ObjectField>>(ChangedMesh);
        fractureCount = rootVisualElement.Q<IntegerField>("FractureCount");
        fractureCount.RegisterCallback<ChangeEvent<IntegerField>>((evt) =>
        {
            fractureCount = evt.newValue;
        });
        
        density = rootVisualElement.Q<FloatField>("Density");
        density.RegisterCallback<ChangeEvent<FloatField>>((evt) =>
        {
            density = evt.newValue;
        });
        
        seed = rootVisualElement.Q<IntegerField>("Seed");
        seed.RegisterCallback<ChangeEvent<IntegerField>>((evt) =>
        {
            seed = evt.newValue;
        });
        

        
        // Placement enum
        var placementUxmlField = rootVisualElement.Q<EnumField>("PlacementType");
        // initialize enum
        placementUxmlField.Init(PlacementType.Container);
        placementUxmlField.value = PlacementType.Container;
        prefabPlacementType = placementUxmlField;
        // Mirror value of uxml field into the C# field.
        placementUxmlField.RegisterCallback<ChangeEvent<EnumField>>((evt) =>
        {
            prefabPlacementType = evt.newValue;
            MyCallback();
        });
        
        var fractureUxmlField = rootVisualElement.Q<EnumField>("FractureType");
        fractureUxmlField.Init(FractureType.Voronoi);
        fractureType = fractureUxmlField;
        // Mirror value of uxml field into the C# field.
        fractureUxmlField.RegisterCallback<ChangeEvent<EnumField>>((evt) =>
        {
            fractureType = evt.newValue;
            Debug.Log("sdffsd");
        });
        
        
        
        // Rotation enum
        var rotationUxmlField = rootVisualElement.Q<EnumField>("RotationType");
        // initialize enum
        rotationUxmlField.Init(RotationType.RandomNormal);
        rotationUxmlField.value = RotationType.RandomNormal;
        prefabRotationType = rotationUxmlField;
        // Mirror value of uxml field into the C# field.
        rotationUxmlField.RegisterCallback<ChangeEvent<EnumField>>((evt) =>
        {
            prefabRotationType = evt.newValue;
            MyCallback();
            Debug.Log("Hello");
        });
        
        // Scale size adjustment field
        var uxmlScaleField = rootVisualElement.Q<FloatField>("Scale");
        prefabScale = uxmlScaleField;
        uxmlScaleField.RegisterCallback<ChangeEvent<FloatField>>((evt) =>
        {
            prefabScale =  evt.newValue;
        });
        
        // Scale randomization enum
        var scaleUxmlField = rootVisualElement.Q<EnumField>("PrefabScale");
        // initialize enum
        scaleUxmlField.Init(ScaleType.None);
        scaleUxmlField.value = ScaleType.None;
        prefabScaleType = scaleUxmlField;
        // Mirror value of uxml field into the C# field.
        scaleUxmlField.RegisterCallback<ChangeEvent<EnumField>>((evt) =>
        {
            prefabScaleType = evt.newValue;
        });
        
        
        
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
        Slicing
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
