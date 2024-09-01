using System.IO;
using NUnit.Framework;
using UnityEngine;
using UnityEditor;

namespace Junk.Assets.Editor
{
    /*public class FBXAttributeProcessor : AssetPostprocessor
    {
        void OnPostprocessModel(GameObject g)
        {
            // Check if the imported asset is an FBX file
            if (assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
            {
                // Find the "FractureAttribute" in the FBX's user properties
                SerializedObject   fbxAsset       = new SerializedObject(assetImporter);
                SerializedProperty userProperties = fbxAsset.FindProperty("m_UserProperties");

                Debug.Log(userProperties == null);
                if (userProperties != null && userProperties.isArray)
                {
                    Debug.Log(userProperties.arraySize);
                    for (int i = 0; i < userProperties.arraySize; i++)
                    {
                        var    property = userProperties.GetArrayElementAtIndex(i);
                        string name     = property.FindPropertyRelative("m_Name").stringValue;
                        string type     = property.FindPropertyRelative("m_Type").stringValue;
                        string value    = property.FindPropertyRelative("m_Value").stringValue;

                        // Check if the property is "FractureAttribute" and is of type boolean
                        if (name == "FractureAttribute" && type == "bool")
                        {
                            // Add the FBXAttributes component to the root GameObject
                            FBXAttributes fbxAttributes = g.AddComponent<FBXAttributes>();

                            // Set the FractureAttribute value based on the FBX attribute
                            fbxAttributes.FractureAttribute = value == "True";
                            break;
                        }
                    }
                }
            }
        }
    }*/

    class FBXAttributeProcessor : AssetPostprocessor
    {
        private void OnPostprocessGameObjectWithUserProperties(GameObject g, string[] names, object[] dataObjects)
        {
            if (!assetPath.EndsWith(".fbx", System.StringComparison.OrdinalIgnoreCase))
                return;
            
            var importer   = (ModelImporter)assetImporter;
            var           fileName = Path.GetFileName(importer.assetPath);
            //Debug.LogFormat("OnPostprocessGameObjectWithUserProperties(go = {0}) asset = {1}", g.name, fileName);
            
            //var vec3 = Vector3.zero;
            for (int i = 0; i < names.Length; i++)
            {
                var name  = names[i];
                var data = dataObjects[i];
                
                if(data is null)
                    continue;
                
                switch (name)
                {
                    case "FractureAttribute":
                        var boolValue = (bool)data;
                        SetAttributes(g).FractureAttribute = boolValue;
                        break;
                    case "VectorData":
                        //vec3 = (Vector3)(Vector4)val;
                        break;
                }
            }
        }

        private static FBXAttributes SetAttributes(GameObject g)
        {
            var fbxAttributes = g.GetComponent<FBXAttributes>();
            if(fbxAttributes == null)
                fbxAttributes = g.AddComponent<FBXAttributes>();
            
            return fbxAttributes;
        }
    }
}