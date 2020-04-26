using System;
using System.Collections.Generic;
using System.Linq;
using thelebaron.CustomEditor;
using Unity.Mathematics;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.UIElements;
using Random = UnityEngine.Random;

public class KaosMainEditor : EditorBase
{
    //private RotationType prefabRotationType = RotationType.None;
    //private ScaleType prefabScaleType = ScaleType.None;
    private bool placementTypeDetected;
    private bool rotationTypeDetected;
    private ObjectField containerObjectField;
    private ObjectField prefabObjectField;
    
    public List<GameObject> spawnedList = new List<GameObject>();
    public List<GameObject> mergedList = new List<GameObject>();
    private bool spawnOnSelected;

    public static List<GameObject> s_spawnedList = new List<GameObject>();
    // Raycast Properties
    private IntegerField iterations;
    private Vector3Field spread;
    private FloatField positionOffset;
    private FloatField prefabScale;

    private EnumField prefabPlacementType;// = new EnumField();
    private EnumField prefabRotationType;// = new EnumField();
    private EnumField prefabScaleType;// = new EnumField();

    [MenuItem("Tools/LitterBug _%L")] // ctrl shift Open _%#T
    public static void ShowWindow()
    {
        // Opens the window, otherwise focuses it if it’s already open.
        var window = GetWindow<KaosMainEditor>();
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
        
        var floats = root.Query<FloatField>();
        floats.ForEach(Setup_FloatFields);
        
        /*
        rootVisualElement.Q<PropertyField>("ArrayList").Bind(new SerializedObject(this));
        */
        
        SetupRaycastProperties(rootVisualElement);
        SetupPrefabProperties(rootVisualElement);

        var prefs = kaos.KaosEditorSerialization.Load();
        //Debug.Log(prefs);
        prefabObjectField.value = GameObject.Find(prefs.SelectionName);
    }

    private void OnDisable()
    {
        var selection = (GameObject) prefabObjectField.value;
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
        }
        kaos.KaosEditorSerialization.Save((GameObject)prefabObjectField.value);
    }

    private void OnDestroy()
    {
        
        
        SceneView.duringSceneGui -= OnScene;
    }

    private void SetupRaycastProperties(VisualElement visualElement)
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
    }

    private void Setup_FloatFields(FloatField obj)
    {
        if (obj.name == "PositionOffset")
        {
            positionOffset = obj;
        }
    }


    
    private void Setup_ObjectField(ObjectField field)
    {
        if (field.name == "SceneContainer")
        {
            field.RemoveAt(1);
            containerObjectField = new ObjectField
            {
                objectType = typeof(GameObject),
                value = null
            };
            field.Add(containerObjectField);
        }
        
        if (field.name == "SpawnObject")
        {
            field.RemoveAt(1);
            prefabObjectField = new ObjectField
            {
                objectType = typeof(GameObject),
                value = null
            };
            field.Add(prefabObjectField);
        }
    }
    
    private void Setup_Button(Button button)
    {
        if (button.name == "Spawn") 
            button.clickable.clicked += OnSpawn;
        if (button.name == "Merge") 
            button.clickable.clicked += OnMerge;
        if (button.name == "Clear") 
            button.clickable.clicked += OnClear;
        if (button.name == "Destroy") 
            button.clickable.clicked += OnDestroyMerged;
        
    }

    private void SetupPrefabProperties(VisualElement visualElement)
    {
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

    // Clear spawned prefabs
    private void OnClear()
    {
        while (spawnedList.Count > 0)
        {
            for (int i = 0; i < spawnedList.Count; i++)
            {
                DestroyImmediate(spawnedList[i]);
                spawnedList.RemoveAt(i);
                break;
            }
            

        }
        spawnedList.Clear();
    }
    
    // Destroy merged prefabs
    private void OnDestroyMerged()
    {
        while (mergedList.Count > 0)
        {
            for (int i = 0; i < mergedList.Count; i++)
            {
                DestroyImmediate(mergedList[i]);
                mergedList.RemoveAt(i);
                break;
            }
            

        }
        mergedList.Clear();
    }
    // Spawn prefabs
    private void OnSpawn()
    {
        //Debug.Log("Onspawn");
        // unsure about this code
        //rootVisualElement.Query<ObjectField>().ForEach(SpawnObjects);
        if (iterations.value <= 0)
            iterations.value = 1;
        for (int i = 0; i < iterations.value; i++)
        {
            SpawnObjects();
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
        /*
        if (PaintMode)
        {
            Handles.color = GizmoBrushColour;

            //Quaternion.LookRotation(new Vector3(0, 180, 1)) flat circle
            var thicken1 = Radius * 1.005f;
            var thicken2 = Radius * 1.01f;
            var thicken3 = Radius * 1.02f;
            var thicken4 = Radius * 1.03f;
            var thicken11 = Radius * 1.005f;
            var thicken21 = Radius * 1.015f;
            var thicken31 = Radius * 1.025f;
            var thicken41 = Radius * 1.035f;

            if (NormalDirection == Vector3.zero)
                NormalDirection = Vector3.up;

            //show a line thats always up
            var lineEndUpPos = PainterPosition + Vector3.up;
            Handles.DrawLine(PainterPosition, lineEndUpPos);

            //show a line thats always reflects normal direction, to help gauge space
            var lineEndPos = PainterPosition + RayCastHitInfo.normal;
            Handles.DrawLine(PainterPosition, lineEndPos);

            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), Radius,
                EventType.Repaint);
            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), Radius,
                EventType.Repaint);
            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), Radius,
                EventType.Repaint);
            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), thicken1,
                EventType.Repaint);
            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), thicken2,
                EventType.Repaint);
            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), thicken3,
                EventType.Repaint);
            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), thicken4,
                EventType.Repaint);
            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), thicken11,
                EventType.Repaint);
            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), thicken21,
                EventType.Repaint);
            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), thicken31,
                EventType.Repaint);
            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), thicken41,
                EventType.Repaint);


            //Handles.DrawSolidArc(m_PainterPosition, Vector3.up, -Vector3.right, 180, 3);

            Handles.color = GizmoBrushFocalColour;
            var focalSize = 0.5f;

            if (AwesomeExtensions.IsPositive(FocalShift))
            {
                float OldMax = 1;
                float OldMin = 0.1f;
                float NewMax = 1;
                float NewMin = 0.5f;
                float OldRange = (OldMax - OldMin);
                float NewRange = (NewMax - NewMin);
                float NewValue = (((FocalShift - OldMin) * NewRange) / OldRange) + NewMin;
                Debug.Log(NewValue);

                focalSize = NewValue;
            }

            if (AwesomeExtensions.IsNegative(FocalShift))
            {
                //float OldMax = 1; float OldMin = 0.1f; float NewMax = 0.5f; float NewMin = 0.01f;
                float OldMax = 0;
                float OldMin = -1f;
                float NewMax = 0.5f;
                float NewMin = 0f;
                float OldRange = (OldMax - OldMin);
                float NewRange = (NewMax - NewMin);
                float NewValue = (((FocalShift - OldMin) * NewRange) / OldRange) + NewMin;
                Debug.Log(NewValue);
                //float remap = 0.5f - NewValue;
                focalSize = NewValue;
            }




            focalSize = Radius * focalSize;
            //focalSize += m_FocalShift;

            //  (m_Radius * 0.5f) * (m_FocalShift * 1);
            //Debug.Log(focalSize);

            Handles.CircleHandleCap(0, PainterPosition, Quaternion.LookRotation(NormalDirection), focalSize,
                EventType.Repaint);
        }*/

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

    private void SpawnObjects()//ObjectField field)
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
        
        var offset = positionOffset.value;
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
    }
    

    #region FinishedMethods
    
    /// <summary>
    /// Callback that registers the bool to whatever the new value selected in the inspector is.
    /// </summary>
    private void OnToggle_SpawnOnSelected(ChangeEvent<bool> evt)
    {
        spawnOnSelected = evt.newValue;
    }

    /// <summary>
    /// Primary method for logic when merging prefabs
    /// </summary>
    private void OnMerge()
    {
        // Combine all prefabs
        CombinePrefabMeshes();
        // Cleanup combined prefabs
        OnClear();
        /*
        foreach (var gameObject in spawnedList)
        {
            DestroyImmediate(gameObject);
        }
        
        // Clear static list of all user spawned in prefabs
        spawnedList.Clear();*/
    }

    /// <summary>
    /// Sub logic of merging prefabs
    /// </summary>
    private void CombinePrefabMeshes()
    {
        //var array = spawnedList.ToArray();
        var meshFiltersList = new List<MeshFilter>();

        /*while (meshFiltersList.Count!= spawnedList.Count)
        {
            foreach (var spawn in spawnedList)
            {
                var mf = spawn.transform.GetComponent<MeshFilter>();
                if (mf != null)
                {
                    meshFiltersList.Add(mf);
                }
            }
        }*/
        
        // Can only access static list for now
        //s_spawnedList.Clear();
        //s_spawnedList = spawnedList;
        
        foreach (var gameObject in spawnedList)
        {
            var mf = gameObject.transform.GetComponent<MeshFilter>();
            if (mf != null)
            {
                meshFiltersList.Add(mf);
            }
        }
        
        if(meshFiltersList.Count < 1)
            return;
        
        var go = new GameObject("Merged Prefabs");
        go.transform.SetParent(ContainerGameObject().transform);
        go.transform.gameObject.SetActive(true);
        go.transform.position = Vector3.zero;
        go.transform.rotation = Quaternion.identity;
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;
        
        MeshFilter[] meshFilters = meshFiltersList.ToArray();
        CombineInstance[] combine = new CombineInstance[meshFilters.Length];
        
        var i = 0;
        while (i < meshFilters.Length)
        {
            //combine[i].mesh = meshObjects[k].GetComponent<MeshFilter>().sharedMesh;
            // set transform to parent/container transform

            //var pos = root.transform.position + meshFilters[i].transform.localPosition;
            //combine[i].transform = meshFilters[i].transform.localToWorldMatrix * root.root.worldToLocalMatrix;
            
            combine[i].transform = go.transform.worldToLocalMatrix * meshFilters[i].transform.localToWorldMatrix;
            
            combine[i].mesh = meshFilters[i].sharedMesh;
            //combine[i].transform = meshFilters[i].transform.localToWorldMatrix;
            meshFilters[i].gameObject.SetActive(false);
            
            i++;
        }
        

        
        var mat = go.AddComponent<MeshRenderer>();
        mat.sharedMaterial = DefaultMaterialHack();
        var meshFilter = go.AddComponent<MeshFilter>();
        meshFilter.sharedMesh = new Mesh {indexFormat = IndexFormat.UInt32};
        meshFilter.sharedMesh.CombineMeshes(combine);
        
        mergedList.Add(go);
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
    
    /// <summary>
    /// Returns the gameobject contained in the inspector container field,
    /// which is used as a container for all spawned prefabs
    /// </summary>
    /// <returns>a GameObject</returns>
    private GameObject ContainerGameObject()
    {
        var go = (GameObject)containerObjectField.value;
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
    }

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
