using System;
using System.IO;
using System.Text.RegularExpressions;
using Autodesk.Fbx;
using UnityEditor;
using UnityEditor.Formats.Fbx.Exporter;
using UnityEngine;
using Object = UnityEngine.Object;

namespace Junk.Fracture.Editor
{
    public static class AssetHandlingUtility
    {
        public static string ConvertToRelativePath(string fullPath)
        {
            // Ensure the full path includes "Assets" so it can be chopped off
            int assetsIndex = fullPath.IndexOf("Assets", StringComparison.Ordinal);
            if (assetsIndex >= 0)
            {
                return fullPath.Substring(assetsIndex);
            }

            Debug.LogError("The path does not contain 'Assets': " + fullPath);
            return fullPath; // Return full path as fallback (not ideal)
        }

        /// <summary>
        /// Gets the original material asset from the instance or searches for a material with a similar name.
        /// </summary>
        /// <param name="mat">The material instance to retrieve the asset from.</param>
        /// <returns>The original material asset if found; otherwise, a similar material based on the name.</returns>
        public static Material GetMaterialAsset(Material mat)
        {
            // Attempt to find the asset path for the provided material instance
            string assetPath = AssetDatabase.GetAssetPath(mat);

            if (!string.IsNullOrEmpty(assetPath))
            {
                // Load the original material asset from the asset path
                return AssetDatabase.LoadAssetAtPath<Material>(assetPath);
            }

            // If no asset path is found, search for a similar material by name
            if (mat != null)
            {
                string cleanedName = CleanMaterialName(mat.name);

                // Search for materials in the AssetDatabase
                string[] guids = AssetDatabase.FindAssets("t:Material");
                foreach (string guid in guids)
                {
                    string   path          = AssetDatabase.GUIDToAssetPath(guid);
                    Material assetMaterial = AssetDatabase.LoadAssetAtPath<Material>(path);

                    if (assetMaterial != null)
                    {
                        string assetName = CleanMaterialName(assetMaterial.name);
                        if (assetName.Equals(cleanedName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return assetMaterial;
                        }
                    }
                }
            }

            // If no similar material is found, log an error and return null
            Debug.LogError("Material instance is not associated with an asset and no similar material was found.");
            return null;
        }

        /// <summary>
        /// Gets the asset from the instance
        /// </summary>
        public static Material[] GetMaterialAssets(Material[] mats)
        {
            for (int i = 0; i < mats.Length; i++)
            {
                mats[i] = GetMaterialAsset(mats[i]);
            }

            return mats;
        }

        /// <summary>
        /// Because we created the mesh using another method in this class,
        /// we assume its always the first and only mesh in the model file.
        /// </summary>
        /// <param name="assetPath"></param>
        /// <returns></returns>
        public static Mesh LoadDefaultMesh(string assetPath)
        {
            assetPath = ConvertToRelativePath(assetPath);

            Object asset = AssetDatabase.LoadAssetAtPath<Object>(assetPath);

            if (asset == null)
            {
                Debug.LogError($"No asset found at {assetPath}. Check the path.");
                return null;
            }

            // Load the asset at the specified path
            var data = AssetDatabase.LoadAllAssetsAtPath(assetPath);

            foreach (var d in data)
                if (d is Mesh)
                    return (Mesh)d;

            Debug.LogError($"Failed to load asset at {assetPath}");
            return null;
        }

        public static void ExportMesh(GameObject obj, string path)
        {
            string filePath = path;

            ExportModelOptions exportSettings = new ExportModelOptions();
            exportSettings.ExportFormat  = ExportFormat.ASCII;
            exportSettings.KeepInstances = false;

            // Note: If you don't pass any export settings, Unity uses the default settings.
            ModelExporter.ExportObject(filePath, obj, exportSettings);

            AddFractureAttributeToFBX(filePath);
            //Debug.Log("Exported Mesh to " + filePath);
        }

        /// <summary>
        /// Cleans the material name to remove instance suffixes.
        /// </summary>
        /// <param name="name">The name of the material to clean.</param>
        /// <returns>The cleaned name.</returns>
        private static string CleanMaterialName(string name)
        {
            if (string.IsNullOrEmpty(name)) return name;

            // Remove " (Instance)" suffix and similar patterns
            while (name.EndsWith(" (Instance)"))
            {
                name = name.Substring(0, name.Length - " (Instance)".Length);
            }

            return name.Trim();
        }

        public static string StripSpecialCharacters(string input)
        {
            // Replace all characters that are not letters, digits, or whitespace with an empty string
            return Regex.Replace(input, @"[^a-zA-Z0-9\s]", "");
        }

        //private static string fbxPath = "Assets/Cube.fbx";

        public static void AddFractureAttributeToFBX(string fbxPath)
        {
            // Check if the file exists
            if (!File.Exists(fbxPath))
            {
                Debug.LogError($"File not found at path: {fbxPath}");
                return;
            }

            // Read the FBX file content
            string fbxContent = File.ReadAllText(fbxPath);

            // Modify the content to add the FractureAttribute
            string modifiedContent = AddFractureAttributeToModels(fbxContent);

            // Write the modified content back to the file
            File.WriteAllText(fbxPath, modifiedContent);

            Debug.Log("FractureAttribute added to all models in the FBX file.");
        }

        private static string AddFractureAttributeToModels(string fbxContent)
    {
        // Split the content by lines for easier processing
        string[] lines                 = fbxContent.Split(new[] { '\r', '\n' }, System.StringSplitOptions.None);
        bool     inModelSection        = false;
        bool     inPropertiesSection   = false;
        bool     shouldAddAttribute    = false;
        string   indentation           = "";
        string   fractureAttributeLine = "\tP: \"FractureAttribute\", \"Bool\", \"\", \"A+U\",1";

        // Regular expression to match _aX, _bX, _cX, or _dX where X is an integer
        Regex validNameRegex = new Regex(@"_[abcd]\d+$", RegexOptions.IgnoreCase);

        using (StringWriter newFbxContent = new StringWriter())
        {
            foreach (string line in lines)
            {
                // Detect the start of a Model node and check if the name matches the pattern
                if (line.Trim().StartsWith("Model: "))
                {
                    inModelSection = true;

                    // Extract the model name and check against the regex
                    string modelName = line.Split(',')[1].Trim().Trim('"');
                    shouldAddAttribute = validNameRegex.IsMatch(modelName);
                }

                // Detect the Properties70 section inside a Model node
                if (inModelSection && line.Trim().StartsWith("Properties70:"))
                {
                    inPropertiesSection = true;
                    indentation         = line.Substring(0, line.IndexOf("Properties70:"));
                }

                // Detect the end of the Properties70 section and add the FractureAttribute if needed
                if (inModelSection && inPropertiesSection && line.Trim().StartsWith("}"))
                {
                    if (shouldAddAttribute)
                    {
                        newFbxContent.WriteLine(indentation + fractureAttributeLine);
                    }
                    inPropertiesSection = false;
                    inModelSection      = false;
                }

                // Write the current line
                newFbxContent.WriteLine(line);
            }

            return newFbxContent.ToString();
        }
    }
    }
}