using System;
using UnityEditor;
using UnityEngine;

namespace Junk.Destroy.Baking
{
    public partial class FractureEditorWindow : EditorWindow
    {
        private RenderTexture        renderTexture;
        private GameObject           targetObject;
        private PreviewRenderUtility previewUtility;

        private void InitializePreview()
        {
            /*
            if (fractureCache == null || fractureCache.Mesh == null)
            {
                return;
            }

            if (targetObject == null)
            {
                targetObject           = new GameObject();
                targetObject.name     = fractureCache.name + "_Preview";
                //targetObject.hideFlags = HideFlags.HideAndDontSave;
            }
            
            targetObject.AddComponent<MeshFilter>().sharedMesh       = fractureCache.Mesh;
            targetObject.AddComponent<MeshRenderer>().sharedMaterial = fractureCache.Fractures[0].OutsideMaterial;
            
            
            SetupPreviewScene();*/
            /*
            var previewRenderUtility = new PreviewRenderUtility();
            previewRenderUtility.cameraFieldOfView = 30f;
            previewRenderUtility.DrawMesh(myModel.GetComponent<MeshFilter>().sharedMesh, Matrix4x4.identity, myModel.GetComponent<MeshRenderer>().sharedMaterial, 0);
            renderTexture = previewRenderUtility.EndPreview();
            previewRenderUtility.Cleanup();*/
        }

        private void OnDisable()
        {
            if (previewUtility != null)
                previewUtility.Cleanup();

            if (targetObject != null)
                DestroyImmediate(targetObject);
        }

        private void SetupPreviewScene()
        {
            previewUtility = new PreviewRenderUtility();
            
            //targetObject                    = GameObject.CreatePrimitive(PrimitiveType.Cube);
            targetObject.transform.position = Vector3.zero;
		
            // Since we want to manage this instance ourselves, hide it
            // from the current active scene, but remember to also destroy it.
            targetObject.hideFlags = HideFlags.HideAndDontSave;
            
            previewUtility.AddSingleGO(targetObject);

            // Camera is spawned at origin, so position is in front of the cube.
            previewUtility.camera.transform.position = new Vector3(0f, 0f, -10f);
		
            // This is usually set very small for good performance, but
            // we need to shift the range to something our cube can fit between.
            previewUtility.camera.nearClipPlane = 5f;
            previewUtility.camera.farClipPlane  = 20f;
        }
        
        private void DrawPreviewGui()
        {
            /*
            Debug.Log(targetObject);
            // Render the preview scene into a texture and stick it
            // onto the current editor window. It'll behave like a custom game view.
            Rect rect = new Rect(0, 0, 512, 512);
            previewUtility.BeginPreview(rect, previewBackground: GUIStyle.none);
            previewUtility.Render();
            var texture = previewUtility.EndPreview();
            GUI.DrawTexture(rect, texture);*/
        }
    }
}