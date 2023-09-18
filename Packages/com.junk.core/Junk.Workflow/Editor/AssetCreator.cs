
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Serialization;

#if UNITY_EDITOR
using UnityEditor;
namespace Junk.Workflow
{
    
    public class AssetCreator : EditorWindow
    {
        public string                  m_name;
        public Object                  m_model;
        public Object                  m_texture;
        public Material                m_material;
        public string                  m_guid;

        public List<string> m_tags    = new List<string>();

        public ModelData.AssetCategory m_category;
        public ModelData.AssetType     m_type;
        
        //private ModelData    m_modelData = new ModelData();
        
        [MenuItem("Tools/Procgen")]
        private static void Init()
        {
            var window = (AssetCreator) GetWindow(typeof(AssetCreator));
            window.Show();
        }
        
        private void OnEnable()
        {
            UpdateSelection();
        }

        private void OnSelectionChange()
        {
            UpdateSelection();
        }

        private void OnGUI()
        {
            if (m_model != null)
            {
                GUILayout.Label("Name: ", EditorStyles.boldLabel);
                m_name = EditorGUILayout.TextArea(m_name);
            }
            
            EditorGUI.BeginChangeCheck();
            m_model    = EditorGUILayout.ObjectField(m_model, typeof(GameObject), false);
            //var tags = EditorGUILayout.PropertyField(list, new GUIContent("My List Test"), true);
            
            if(m_model!=null)
                m_texture  = EditorGUILayout.ObjectField(m_texture, typeof(Texture2D), false);
            
            if (m_model == null || m_model.GetType()!= typeof(GameObject))// || textures.Count == 0)
            {
                EditorGUILayout.HelpBox(string.Format("Can't find any model or model is missing/invalid"), MessageType.Error);
                return;
            }
            // Display all textures in the list
            GUILayout.Label("Models: ", EditorStyles.boldLabel);
            if(m_model!=null)
                GUILayout.Label(m_model.name);
            
            if (m_model != null)
            {
                m_category = (ModelData.AssetCategory)EditorGUILayout.EnumPopup("Category:", m_category);
                m_type     = (ModelData.AssetType)EditorGUILayout.EnumPopup("Type:", m_type);
                
                GUILayout.Label("Tags: ", EditorStyles.boldLabel);
                for(int i = 0; i < m_tags.Count; i++)
                {
                    m_tags[i] = EditorGUILayout.TextArea(m_tags[i], GUILayout.Width(160));
                    //GUILayout.Label("Tag: " + m_tags[i], EditorStyles.boldLabel);
                }
                if (GUILayout.Button("Add tag", GUILayout.Width(120)))
                {
                    m_tags.Add("");
                }
                if (GUILayout.Button("Remove tag", GUILayout.Width(120)))
                {
                    if(m_tags.Count>0)
                        m_tags.RemoveAt(m_tags.Count-1);
                }
                // add layout space vertical
                GUILayout.Space(10);
                
                m_guid = EditorGUILayout.TextArea(m_guid);
                
                if (GUILayout.Button("Generate GUID", GUILayout.Width(180)))
                {
                    m_guid = GameAssetUtility.UniqueGuid();
                }
            }
            
            //if(m_texture!=null)
                //m_material = EditorGUILayout.ObjectField(m_material, typeof(Material), false);
                
                //prefab = EditorGUILayout.ObjectField(prefab, typeof(GameObject), false);
            
            //EditorGUI.PropertyField(r, sp, GUIContent.none);

            
            if (EditorGUI.EndChangeCheck())
            {
                UpdateSelection();
            }

            
            EditorGUILayout.Space();
            
            if (GUILayout.Button("CreateAsset"))
            {
                var contentAsset = new ModelData();
                
                contentAsset.name          = m_name;
                contentAsset.model         = (GameObject) m_model;
                contentAsset.texture       = (Texture2D) m_texture;
                contentAsset.material      = (Material) m_material;
                contentAsset.assetCategory = m_category;
                contentAsset.assetType     = m_type;
                contentAsset.tags          = m_tags;
                
                if (m_material == null)
                {
                    m_material           = GetMaterial(contentAsset);
                    contentAsset.material = (Material) m_material;
                }
                
                contentAsset.CreateAsset();
            }
            if (GUILayout.Button("Clear"))
            {
                //md = null;
                m_name     = null;
                m_model    = null;
                m_texture  = null;
                m_material = null;
                m_tags     = new List<string>();
            }
            
            if (GUILayout.Button("EchoAllAssets"))
            {
                //GameAssetUtility.RecursiveGet();
            }
        }

        private void AtlasSizeDropdown()
        {
            
        }

        private void UpdateSelection()
        {
            //textures.Clear();
            //meshFilters.Clear();
            //GrabTexturesFromCubemap();
            
            /*
            foreach (var selectedObject in Selection.gameObjects)
            {
                meshRenderers = selectedObject.GetComponentsInChildren<MeshRenderer>();
                foreach (var meshFilter in meshRenderers)
                {
                    meshFilters.Add(meshFilter.GetComponent<MeshFilter>());
                }

            }*/
        }

        public static Material GetMaterial(ModelData data)
        {
            var p = AssetDatabase.GetAssetPath(data.model);
            p = Path.GetDirectoryName(p);
            var material = new Material(Shader.Find("RetroJunk"));
            AssetDatabase.CreateAsset(material, p + "/" + data.name + ".mat");
            data.material = material;

            {
                // Set material params
                data.material.SetColor("_BaseColor", Color.white);
                data.material.SetTexture("_BaseMap", data.texture);
                //modelData.material.SetFloat("FakeContrast", 0); //not working
                AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(data.material));
            }
                    
            AssetDatabase.Refresh();
            return material;
        }
    }
}
#endif
