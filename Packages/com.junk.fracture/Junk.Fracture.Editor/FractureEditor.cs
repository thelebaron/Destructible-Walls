using System;
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
        private       FractureSetupData data;
        private const string            umxlPath = "Packages/com.junk.fracture/Junk.Fracture.Editor/Resources/FractureEditorMain.uxml";
        private const string            stylePath = "Packages/com.junk.fracture/Junk.Fracture.Editor/Resources/FractureEditorMain_Style.uss";

        private       ObjectField  objectField;
        private       Button       randomSeedButton;
        private       IntegerField seedIntField;

        public static void Open(Object target)
        {
            var window = (FractureEditor)GetWindow(typeof(FractureEditor));
            window.Clear();
            
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

            // Optionally bind an initial GameObject
            objectField.value = Selection.activeGameObject;

            // Handle value changes
            objectField.RegisterValueChangedCallback(evt =>
            {
                //Debug.Log("Selected GameObject: " + evt.newValue);
            });
            
            randomSeedButton.RegisterCallback<ClickEvent>(ev => seedIntField.value = UnityEngine.Random.Range(0, int.MaxValue));
        }

        private void SetupVisualElements()
        {
            var rootElement = rootVisualElement;

            objectField = rootElement.Q<ObjectField>("object-field");
            randomSeedButton = rootElement.Q<Button>("randomseed-button");
            seedIntField = rootElement.Q<IntegerField>("seed-intfield");
        }

        private void Clear()
        {
            if (data != null)
                if (data.cache != null)
                    data.cache = null;
        }

        private void OnGUI()
        {
            if (data == null)
            {
                
            }
            return;
            GUILayout.Label("Welcome !");
            data.labelText = Selection.activeObject == null ? data.labelText : Selection.activeObject.name;
            data.labelText = EditorGUILayout.TextField(data.labelText);
            
            GUILayout.Label("Fracture Cache:");
            data.cache = (FractureCache)EditorGUILayout.ObjectField(data.cache, typeof(FractureCache), true);

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Seed:");
            data.seed = EditorGUILayout.IntField(data.seed);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUILayout.ObjectField(data.cache.Mesh, typeof(Mesh), false);
            
            
            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Inside Material:");
            data.insideMaterial = (Material)EditorGUILayout.ObjectField(data.insideMaterial, typeof(Material), true);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            GUILayout.BeginHorizontal();
            GUILayout.Space(20);
            GUILayout.Label("Outside Material:");
            data.outsideMaterial = (Material)EditorGUILayout.ObjectField(data.outsideMaterial, typeof(Material), true);
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();

            EditorGUI.indentLevel++;
            EditorGUI.indentLevel++;
            //fractureType
            data.fractureType =(FractureType)EditorGUILayout.EnumPopup(data.fractureType);
            // switch
            switch (data.fractureType)
            {
                case FractureType.Voronoi:

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Density:");
                    data.density = EditorGUILayout.FloatField(data.density);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Total Chunks:");
                    data.totalChunks = EditorGUILayout.IntField(data.totalChunks);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    EditorGUI.indentLevel++;
                    GUILayout.Space(20);


                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    break;
                case FractureType.Clustered:
                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Clusters:");
                    data.clusters = EditorGUILayout.IntField(data.clusters);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Sites Per Cluster:");
                    data.sitesPerCluster = EditorGUILayout.IntField(data.sitesPerCluster);
                    GUILayout.FlexibleSpace();
                    GUILayout.EndHorizontal();

                    GUILayout.BeginHorizontal();
                    GUILayout.Space(20);
                    GUILayout.Label("Cluster Radius:");
                    data.clusterRadius = EditorGUILayout.FloatField(data.clusterRadius);
                    GUILayout.EndHorizontal();
                    break;
            }
            
            if (GUILayout.Button("Fracture node"))
            {
                if (data.seed == -1)
                    data.seed = new System.Random().Next();
                data.cache.Clear();
                NvidiaBridgeUtility.Intialize(data.cache, data);

                AssetDatabase.Refresh();
                // refresh inspector
                EditorUtility.SetDirty(data.cache);
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;

            EditorGUI.indentLevel++;

            if (data.cache.Children.Count > 0)
            {
                GUILayout.BeginHorizontal();
                EditorGUI.indentLevel++;
                GUILayout.Space(20);
                if (GUILayout.Button("Fracture children"))
                {
                    if (data.seed == -1)
                        data.seed = new System.Random().Next();
                    for (var index = 0; index < data.cache.Children.Count; index++)
                    {
                        var child = data.cache.Children[index];
                        child.Clear();
                        NvidiaBridgeUtility.Intialize(child, data);
                    }

                    AssetDatabase.Refresh();
                    // refresh inspector
                    EditorUtility.SetDirty(data.cache);
                }

                GUILayout.FlexibleSpace();
                GUILayout.EndHorizontal();

                EditorGUILayout.Space();
            }

            EditorGUI.indentLevel--;
            EditorGUI.indentLevel--;
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
    }
}