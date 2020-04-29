
using System;
using System.Collections.Generic;
using kaos;
using thelebaron.CustomEditor;
using thelebaron.mathematics;
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
        // event object
        // Scale size adjustment field
        var mesh_field = visualElement.Q<ObjectField>("mesh-input");
        meshInputField = mesh_field;
        mesh_field.objectType = typeof(Mesh);
        
        mesh_field.RegisterCallback<ChangeEvent<Object>>(evt => // needed Object not ObjectField or Mesh
        {
            meshInputField.value = evt.newValue;
            Debug.Log(AssetDatabase.GetAssetPath(evt.newValue));
        });
        

        
        

        var label = visualElement.Q<Label>("preview-distance-label");
        label.text = "1";
        
        var previewSlider = visualElement.Q<Slider>("preview-distance-slider");
        distancePreviewSlider = previewSlider;
        previewSlider.value = 1;
        previewSlider.RegisterCallback<ChangeEvent<float>>(evt => {
            label.text = maths.round(evt.newValue).ToString();
            //Debug.Log(distancePreviewSlider.value);
        });
        
        var fracture_label = visualElement.Q<Label>("fracture-nesting-label");
        var fracture_levels_slider = visualElement.Q<SliderInt>("fracture-nesting-label");
        fractureNestingSlider = fracture_levels_slider;
        fracture_levels_slider.value   = 3;
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

    private void OnChangeFractureNesting(int count)
    {
        OnReset();
        nestedNodes = new List<List<NodeInfo>>();
        for (var i = 0; i < count; i++)
        {
            nestedNodes.Add(new List<NodeInfo>());
        }
    }

    
    
    private void TryGetMesh()
    {
        //Debug.Log("meshasset:"+AssetDatabase.GetAssetPath(Selection.activeGameObject.GetComponent<MeshFilter>().sharedMesh));
        // is actual obj
        Debug.Log(AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(kaosPreferences.Mesh));
        
        if(AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(kaosPreferences.Mesh) == null)
            return;
        // Try load last mesh
        if (AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(kaosPreferences.Mesh) != null)
            meshInputField.value = AssetDatabase.LoadAssetAtPath<UnityEngine.Mesh>(kaosPreferences.Mesh);
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
        kaosPreferences.MaterialInside = AssetDatabase.GetAssetPath(materialInsideField.value);
        kaosPreferences.MaterialOutside = AssetDatabase.GetAssetPath(materialOutideField.value);
        
        KaosSerialization.Save(kaosPreferences);
    }



}
