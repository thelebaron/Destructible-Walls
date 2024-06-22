using System;
using System.Collections.Generic;
using Junk.Entities.Hybrid;
using UnityEditor;
using UnityEngine;

namespace Junk.Entities.Editor
{
    [UnityEditor.CustomEditor(typeof(SceneObjects))]
    public class SceneObjectsEditor : UnityEditor.Editor
    {
        private List<GameObject> _hiddenObjects;
        private List<HideFlags> _hiddenFlags;

        private void OnEnable()
        {
            _hiddenObjects = new List<GameObject>();
            _hiddenFlags = new List<HideFlags>();
        }
        
        private void OnDisable()
        {
            for (var index = 0; index < _hiddenObjects.Count; index++)
            {
                var hiddenObject = _hiddenObjects[index];
                hiddenObject.hideFlags = _hiddenFlags[index];
            }
            _hiddenObjects.Clear();
            _hiddenFlags.Clear();
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            
            // button
            if (GUILayout.Button("Show hidden scene objects"))
            {
                var allObjects =  GetAllObjectsOnlyInScene();
                foreach (var obj in allObjects)
                {
                    obj.hideFlags = HideFlags.None;
                }
            }
        }
        
        List<Light> GetAllObjectsOnlyInScene()
        {
            List<Light> objectsInScene = new List<Light>();

            foreach (Light go in Resources.FindObjectsOfTypeAll(typeof(Light)) as Light[])
            {
                if (!EditorUtility.IsPersistent(go.transform.root.gameObject))
                    objectsInScene.Add(go);
            }

            return objectsInScene;
        }
    }
}