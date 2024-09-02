using System;
using System.Collections.Generic;
using System.Linq;
using Junk.Fracture.Hybrid;
using UnityEngine;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine.Assertions;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;
using Random = Unity.Mathematics.Random;

namespace Junk.Fracture.Editor
{
    public class FractureEditor : EditorWindow
    {
        private       FracturingData data;
        private const string            umxlPath           = "Packages/com.junk.fracture/Junk.Fracture.Editor/Resources/FractureEditorMain.uxml";
        private const string            stylePath          = "Packages/com.junk.fracture/Junk.Fracture.Editor/Resources/FractureEditorMain_Style.uss";
        private const string            insideMaterialPath = "Packages/com.junk.fracture/Junk.Fracture/Materials/InsideMaterial.mat";
        private const string            outsideMaterialPath = "Packages/com.junk.fracture/Junk.Fracture/Materials/OutsideMaterial.mat";

        private ObjectField  objectField;
        private ObjectField  meshField;
        private Button       randomSeedButton;
        private IntegerField seedIntField;
        private ObjectField  insideMaterialField;
        private ObjectField  outsideMaterialField;
        private SliderInt    fractureSlider;
        private Label        fractureSliderLabel;
        private Slider    previewDistanceSlider;
        private Label previewDistanceLabel;
        private Toggle       applyToObjectToggle;
        private IntegerField fractureCountField;
        private Button       fractureButton;
        private Button       createBondsButton;
        private Button       resetButton;
        private Button       saveButton;
        private TextField    logField;

        public static void Open(Object target)
        {
            var window = (FractureEditor)GetWindow(typeof(FractureEditor));
            window.minSize = new Vector2(290, 370);
            
            FractureEditorMethods.Setup(ref window.data, target, EditorMode.OpenEditor);
            window.Show();
        }

        [MenuItem("Tools/Fracture Editor")]
        public static void ShowWindow()
        {
            GetWindow<FractureEditor>("Fracture Editor");
        }

        public void CreateGUI()
        {
            // Load the UXML file
            var uxmlAsset = AssetDatabase.LoadAssetAtPath<VisualTreeAsset>(umxlPath);

            var root = rootVisualElement;
            uxmlAsset.CloneTree(root);

            var styleSheet = AssetDatabase.LoadAssetAtPath<StyleSheet>(stylePath);
            root.styleSheets.Add(styleSheet);

            SetupVisualElements();
            SetupReferences();
        }

        private void SetupReferences()
        {
            objectField.objectType = typeof(GameObject); // Ensure it only accepts GameObjects
            meshField.objectType = typeof(Mesh);
            
            // Optionally bind an initial GameObject
            objectField.value = Selection.activeGameObject;

            var go = (GameObject)objectField.value;
            if (go.GetComponent<MeshFilter>())
            {
                meshField.value = go.GetComponent<MeshFilter>().sharedMesh;
            }

            // Handle value changes
            objectField.RegisterValueChangedCallback(evt => {
                //Debug.Log("Selected GameObject: " + evt.newValue);
            });
            
            randomSeedButton.RegisterCallback<ClickEvent>(ev => seedIntField.value = UnityEngine.Random.Range(0, int.MaxValue));
            
            seedIntField.RegisterValueChangedCallback(ev => EditorPrefs.SetInt("FractureEditor_Seed", seedIntField.value));
            
            seedIntField.value =  EditorPrefs.GetInt("FractureEditor_Seed", defaultValue: 12345); 
            
            insideMaterialField.objectType = typeof(Material);
            outsideMaterialField.objectType = typeof(Material);
            
            insideMaterialField.value = AssetDatabase.LoadAssetAtPath<Material>(insideMaterialPath);
            outsideMaterialField.value = AssetDatabase.LoadAssetAtPath<Material>(outsideMaterialPath);
            
            fractureSlider.RegisterValueChangedCallback(ev => fractureSliderLabel.text = $"{fractureSlider.value}");
            previewDistanceSlider.RegisterValueChangedCallback(ev =>
                {
                    previewDistanceLabel.text = $"{previewDistanceSlider.value}";
                    var gameObject = (GameObject)objectField.value;
                    GeometryEx.SetPreviewDistance(gameObject, previewDistanceSlider.value);
                }
            );
            
            fractureButton.RegisterCallback<ClickEvent>(ev => FractureObject());
            createBondsButton.RegisterCallback<ClickEvent>(ev => CreateBonds());
            resetButton.RegisterCallback<ClickEvent>(ev => ResetValues());
            saveButton.RegisterCallback<ClickEvent>(ev => Save());
        }

        private void FractureObject()
        {
            // remove existing fractures
            var gameObject = (GameObject)objectField.value;
            var children   = gameObject.GetComponentsInChildren<ModelChunkInfo>().ToList();
            if(children.Count>1)
            {
                for (int i = 0; i < children.Count; i++)
                {
                    if (children[i].gameObject != gameObject) continue;
                    children.RemoveAt(i);
                    break;
                }

                foreach (var c in children)
                {
                    Object.DestroyImmediate(c.gameObject);
                }
            }
            
            data.seed            = EditorGUILayout.IntField(data.seed);
            data.targetObject    = (GameObject)objectField.value;
            data.targetMesh      = (Mesh)meshField.value;
            data.insideMaterial  = (Material)insideMaterialField.value;
            data.outsideMaterial = (Material)outsideMaterialField.value;
            //fractureType
            data.fractureType  = FractureType.Voronoi;
            data.density       = 500;
            data.totalChunks   = fractureCountField.value;
            data.applyToObject = applyToObjectToggle.value;
            
            data.fractureList ??= new List<GameObject>();
            
            /*
                data.clusterRadius   = EditorGUILayout.FloatField(data.clusterRadius);
                data.clusters        = EditorGUILayout.IntField(data.clusters);
                data.sitesPerCluster = EditorGUILayout.IntField(data.sitesPerCluster);
            */
            
            NvidiaBridgeUtility.Intialize(data, out var message);
            LogToConsole(message);
        }
        
        private void CreateBonds()
        {
            var gameObject = (GameObject)objectField.value;
            var children   = gameObject.GetComponentsInChildren<Transform>().ToList();
            children.Remove(gameObject.transform);
            
            var chunks = new List<(GameObject, Mesh)>();

            foreach (var c in children)
            {
                var meshFilter = c.gameObject.GetComponent<MeshFilter>();
                if (meshFilter != null && meshFilter.sharedMesh != null)
                {
                    chunks.Add((c.gameObject, meshFilter.sharedMesh));
                }
            }
            
            GeometryEx.GetOverlaps(chunks, 0.05f);
        }
        
        private void ResetValues()
        {
            fractureSlider.value     = 1;
            fractureCountField.value = 5;
            
            if(data.fractureList!=null)
            {
                foreach (var go in data.fractureList)
                {
                    Object.DestroyImmediate(go);
                }
                data.fractureList.Clear();
            }

            var gameObject = (GameObject)objectField.value;
            var children   = gameObject.GetComponentsInChildren<Transform>().ToList();
            children.Remove(gameObject.transform);
            
            foreach (var go in children)
            {
                if(go.GetComponent<ModelChunkInfo>())
                    Object.DestroyImmediate(go.gameObject);
            }
        }

        private void SetupVisualElements()
        {
            var rootElement = rootVisualElement;

            objectField           = rootElement.Q<ObjectField>("object-field");
            randomSeedButton      = rootElement.Q<Button>("randomseed-button");
            seedIntField          = rootElement.Q<IntegerField>("seed-intfield");
            insideMaterialField   = rootElement.Q<ObjectField>("inside-material-field");
            outsideMaterialField  = rootElement.Q<ObjectField>("outside-material-field");
            meshField             = rootElement.Q<ObjectField>("mesh-field");
            fractureSlider        = rootElement.Q<SliderInt>("fracture-slider");
            previewDistanceSlider = rootElement.Q<Slider>("preview-distance-slider");
            previewDistanceLabel  = rootElement.Q<Label>("preview-distance-label");
            applyToObjectToggle   = rootElement.Q<Toggle>("apply-to-object-toggle");
            fractureCountField    = rootElement.Q<IntegerField>("fracture-count");
            fractureSliderLabel   = rootElement.Q<Label>("fracture-nesting-label");
            fractureButton        = rootElement.Q<Button>("fracture-button");
            createBondsButton     = rootElement.Q<Button>("create-bonds-button");
            resetButton           = rootElement.Q<Button>("reset-button");
            saveButton            = rootElement.Q<Button>("save-button");
            logField              = rootElement.Q<TextField>("log-field");
        }

        private void LogToConsole(string message)
        {
            logField.value = message;
        }

        private void Update()
        {
            // Just do some random modifications here.
            float time = (float)EditorApplication.timeSinceStartup * 15;

            // Since this is the most important window in the editor, let's use our
            // resources to make this nice and smooth, even when running in the background.
            Repaint();
        }

        public void OnEnable()
        {
            // NO-OP - ignore any initialization for creategui
        }
        
        private void Save()
        {
            var gameObject    = (GameObject) objectField.value;
            var path = EditorUtility.SaveFilePanel("Save", "Assets/", name, "fbx");
            AssetHandlingUtility.ExportMesh(gameObject, path);
        }

        private void OnDisable()
        {
            var gameObject = (GameObject) objectField.value;
            GeometryEx.ResetPreviewDistance(gameObject);
        }
    }
}