
using thelebaron.CustomEditor;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

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
        if (field.name == "OriginalMesh")
        {
            field.RemoveAt(1); // why
            meshField = new ObjectField
            {
                objectType = typeof(Mesh),
                value      = null
            };
            field.Add(meshField);
        }
        
        
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
}
