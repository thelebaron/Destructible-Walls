
using System;
using System.Collections.Generic;
using System.Linq;
using kaos;
using Project.Scripts.Utils;
using Unity.Mathematics;
using UnityEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;
using Object = UnityEngine.Object;

public partial class KaosEditor
{
    
    private void Setup_FloatFields(FloatField obj)
    {
        /*if (obj.name == "TotalMass")
        {
            totalMass = obj;
        }*/
    }
    private void Setup_IntFields(IntegerField obj)
    {

    }
    
    private void Setup_ObjectField(ObjectField field)
    {
        /*
        if (field.name == "OriginalMesh")
        {
            field.RemoveAt(1); // why
            meshField = new ObjectField
            {
                objectType = typeof(Mesh),
                value      = null
            };
            field.Add(meshField);
            
            meshField.RegisterCallback<ChangeEvent<Mesh>>(evt =>
            {
                meshField.value = evt.newValue;
                Debug.Log(meshField.value);
            });
        }*/
        
        if (field.name == "MaterialInside")
        {
            field.RemoveAt(1);
            materialInsideField = new ObjectField
            {
                objectType = typeof(Material),
                value      = null
            };
            field.Add(materialInsideField);
        }
        if (field.name == "MaterialOutside")
        {
            field.RemoveAt(1);
            materialOutideField = new ObjectField
            {
                objectType = typeof(Material),
                value      = null
            };
            field.Add(materialOutideField);
        }
        

    }
    
    private void Setup_Button(Button button)
    {
        if (button.name == "Fracture") 
            button.clickable.clicked += OnFracture;
        if (button.name == "Reset") 
            button.clickable.clicked += OnReset;
        if (button.name == "Export") 
            button.clickable.clicked += OnExport;
        
        if (button.name == "RandomSeed") 
            button.clickable.clicked += OnRandomSeed;
    }
    
    private void RegisterCallbacks(VisualElement visualElement)
    {
        // Scale size adjustment field
        var prefab_field = visualElement.Q<ObjectField>("prefab-field");
        prefabField = prefab_field;
        prefab_field.objectType = typeof(GameObject);
        prefab_field.RegisterCallback<ChangeEvent<Object>>(evt =>
        {
            
            prefabField.value = evt.newValue;//prefab;
            
            
            OnChangePrefab(evt);
        });
        //prefabField = fracture_object;
        

        // event object
        var mesh_field = visualElement.Q<ObjectField>("mesh-input");
        meshField = mesh_field;
        mesh_field.objectType = typeof(Mesh);
        
        mesh_field.RegisterCallback<ChangeEvent<Object>>(evt => // needed Object not ObjectField or Mesh
        {
            meshField.value = evt.newValue;
            
        });
        

        
        

        var label = visualElement.Q<Label>("preview-distance-label");
        label.text = "1";
        
        var previewSlider = visualElement.Q<Slider>("preview-distance-slider");
        distancePreviewSlider = previewSlider;
        previewSlider.value = 1;
        previewSlider.RegisterCallback<ChangeEvent<float>>(evt =>
        {
            label.text = math.round(evt.newValue).ToString(); //maths.round(evt.newValue).ToString();
            //Debug.Log(distancePreviewSlider.value);
        });
        
        var fracture_label = visualElement.Q<Label>("fracture-nesting-label");
        fracture_label.text = 1.ToString();
        var fracture_levels_slider = visualElement.Q<SliderInt>("fracture-nesting-label");
        fractureNestingSlider = fracture_levels_slider;
        fracture_levels_slider.value   = 1;
        fracture_levels_slider.RegisterCallback<ChangeEvent<int>>(evt => {
            fracture_label.text = evt.newValue.ToString();
            OnChangeFractureNesting(evt.newValue);
        });

        //fracture-levels-label
        
        
        
        
        
        
        
        
        
        
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

        {
            // CLuster fields
            Clusters = rootVisualElement.Q<IntegerField>("Clusters");
            Clusters.value = 5;
            Clusters.RegisterCallback<ChangeEvent<IntegerField>>((evt) =>
            {
                Clusters = evt.newValue;
            });
            SitesPerCluster = rootVisualElement.Q<IntegerField>("SitesPerCluster");
            SitesPerCluster.value = 5;
            SitesPerCluster.RegisterCallback<ChangeEvent<IntegerField>>((evt) =>
            {
                SitesPerCluster = evt.newValue;
            });
            ClusterRadius = rootVisualElement.Q<FloatField>("ClusterRadius");
            ClusterRadius.value = 1;
            ClusterRadius.RegisterCallback<ChangeEvent<FloatField>>((evt) =>
            {
                ClusterRadius = evt.newValue;
            });
        }
        
        var fractureUxmlField = rootVisualElement.Q<EnumField>("FractureType");
        fractureUxmlField.Init(FractureType.Voronoi);
        fractureType = fractureUxmlField;
        // Mirror value of uxml field into the C# field.
        fractureUxmlField.RegisterCallback<ChangeEvent<Enum>>((evt) =>
        {
            fractureType.value = evt.newValue;
            Debug.Log(fractureType.value);
        });
        
        

        var toggle_field = rootVisualElement.Q<Toggle>("postprocess-option");
        postprocess = toggle_field;
        
        
        // Slices
        {
            var slices_field = rootVisualElement.Q<Vector3IntField>("slices-field");
            slices = slices_field;
            slices_field.RegisterCallback<ChangeEvent<Vector3Int>>((evt) => {
                slices.value = evt.newValue;
            });
            
            var slices_offset = rootVisualElement.Q<Slider>("slices-offset-slider");
            slicesOffset = slices_offset;
            slices_offset.RegisterCallback<ChangeEvent<float>>((evt) => {
                slicesOffset.value = evt.newValue;
            });
            
            var slices_angle = rootVisualElement.Q<Slider>("slices-angle-slider");
            slicesAngle = slices_angle;
            slices_angle.RegisterCallback<ChangeEvent<float>>((evt) => {
                slicesAngle.value = evt.newValue;
            });
            
            var slices_amplitude = rootVisualElement.Q<FloatField>("slices-amplitude-field");
            slicesAmplitude = slices_amplitude;
            slices_amplitude.RegisterCallback<ChangeEvent<float>>((evt) => {
                slicesAmplitude.value = evt.newValue;
            });
            var slices_frequency = rootVisualElement.Q<FloatField>("slices-frequency-field");
            slicesFrequency = slices_amplitude;
            slices_frequency.RegisterCallback<ChangeEvent<float>>((evt) => {
                slicesFrequency.value = evt.newValue;
            });
            
            var slices_octave = rootVisualElement.Q<IntegerField>("slices-octave-field");
            slicesOctave = slices_octave;
            slices_octave.RegisterCallback<ChangeEvent<int>>((evt) => {
                slicesOctave.value = evt.newValue;
            });

            var slices_surfaceresolution = rootVisualElement.Q<IntegerField>("slices-surface-resolution");
            slicesSurfaceResolution = slices_surfaceresolution;
            slices_surfaceresolution.RegisterCallback<ChangeEvent<int>>((evt) => {
                slicesSurfaceResolution.value = evt.newValue;
            });


        }

    }

    private void OnChangeFractureNesting(int count)
    {
        //OnReset();
        nestedNodes = new List<List<NodeInfoBehaviour>>();
        for (var i = 0; i < count; i++)
        {
            nestedNodes.Add(new List<NodeInfoBehaviour>());
        }
    }

    private void OnChangePrefab(ChangeEvent<Object> evt)
    {        
        if(evt.newValue==null)
             return;
        if(evt.newValue.GetType()!= typeof(GameObject))
            Debug.LogError("Object is not a GameObject!");
        
        var go = (GameObject) evt.newValue;

        if (go.GetComponent<FractureData>() == null)
            data = go.AddComponent<FractureData>();
        if (go.GetComponent<FractureData>() != null)
            data = go.GetComponent<FractureData>();
        data.Reset();
        
        if(!IsPrefab((GameObject)evt.newValue))
            Debug.LogError("Not a prefab!");
        
        var meshFilter = Prefab.GetComponent<MeshFilter>();
        if(meshFilter==null)
            Debug.LogError("No MeshFilter found!");

        if(meshFilter.sharedMesh==null)
            Debug.LogError("No mesh inside MeshFilter found!");
        
        var mesh = meshFilter.sharedMesh;

        PrefabMesh = mesh;
        meshField.value = PrefabMesh;
    }

    private bool IsPrefab(GameObject gameObject)
    {
        //return PrefabUtility.GetPrefabParent(gameObject) == null && PrefabUtility.GetPrefabObject(gameObject) != null; // Is a prefab
        //return PrefabUtility.GetCorrespondingObjectFromSource(gameObject) == null && PrefabUtility.GetPrefabInstanceHandle(gameObject) != null; // Is a prefab
        
        return PrefabUtility.IsPartOfAnyPrefab(gameObject);
    }
    
    
    private void TryGetMesh()
    {
        //Debug.Log("meshasset:"+AssetDatabase.GetAssetPath(Selection.activeGameObject.GetComponent<MeshFilter>().sharedMesh));
        // is actual obj
        Debug.Log(AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(preferences.Mesh));
        
        if(AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(preferences.Mesh) == null)
            return;
        // Try load last mesh
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(preferences.Mesh) != null)
            meshField.value = AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(preferences.Mesh);
    }
    
    private void TryGetMaterials()
    {
        // Try load settings material, if not load default material
        //if (AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(kaosPreferences.MaterialInside) == null) 
            //materialInside = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/Materials/Inside.mat");
        //if (AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(kaosPreferences.MaterialInside) != null)
            //materialInside = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(kaosPreferences.MaterialInside);
        
        // Try load settings material, if not load default material
        //if (AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(kaosPreferences.MaterialOutside) == null)
            //materialOutside = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/Materials/Outside.mat");
        //if (AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(kaosPreferences.MaterialOutside) != null)
            //materialOutside = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>(kaosPreferences.MaterialOutside);
       

        materialInsideField.value = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/Materials/Inside.mat");
        materialOutideField.value = AssetDatabase.LoadAssetAtPath<UnityEngine.Material>("Assets/Materials/Outside.mat");
    }

    private void Save()
    {
        return;
        //Debug.Log(meshField.value);
        //Debug.Log(AssetDatabase.GetAssetPath(meshField.value));
        
        //kaosPreferences.Mesh = AssetDatabase.GetAssetPath(meshField.value);
        preferences.MaterialInside = AssetDatabase.GetAssetPath(materialInsideField.value);
        preferences.MaterialOutside = AssetDatabase.GetAssetPath(materialOutideField.value);
        
        Serialization.Save(preferences);
    }



}
