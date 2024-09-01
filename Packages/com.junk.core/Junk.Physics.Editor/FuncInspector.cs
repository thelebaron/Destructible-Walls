using Junk.Entities.Hybrid;
using Junk.Gameplay.Hybrid;
using UnityEngine;
using UnityEditor;

namespace Junk.Gameplay.Editor
{
    [CustomEditor(typeof(GameplayAuthoring))]
    public class FuncAuthoringInspector : UnityEditor.Editor
    {
        public override void OnInspectorGUI()
        {
            var authoring = target as GameplayAuthoring;
            authoring.GizmosAlpha = EditorGUILayout.FloatField("Gizmos Alpha", authoring.GizmosAlpha);
            
            if (authoring.IsTrigger)
            {
                EditorGUILayout.LabelField("Trigger", EditorStyles.wordWrappedLabel);
                // Space
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("Target:", authoring.Target);
                foreach (var target in authoring.Targets)
                {
                    EditorGUILayout.ObjectField("Target:", target, typeof(GameObject), true);
                }
                authoring.TriggerCenter  = EditorGUILayout.Vector3Field("Center", authoring.TriggerCenter);
                authoring.TriggerExtents = EditorGUILayout.Vector3Field("Extents", authoring.TriggerExtents);
            }
            
            if (authoring.IsTarget)
            {
                EditorGUILayout.LabelField("IsTarget", EditorStyles.wordWrappedLabel);
                // Space
                EditorGUILayout.Space();
                EditorGUILayout.LabelField("TargetName:", authoring.TargetName);
            }
            
            if(authoring.HasAngle)
            {
                EditorGUILayout.LabelField("HasAngle", EditorStyles.wordWrappedLabel);
                // Space
                EditorGUILayout.Space();
                authoring.Angle = EditorGUILayout.FloatField("Angle", authoring.Angle);
            }
            
            if(authoring.HasHealth)
            {
                EditorGUILayout.LabelField("Health", EditorStyles.wordWrappedLabel);
                // Space
                EditorGUILayout.Space();
                authoring.HealthValue = EditorGUILayout.FloatField("Health", authoring.HealthValue);
            }
            
            if(authoring.IsDestructible)
            {
                EditorGUILayout.LabelField("Destructible", EditorStyles.wordWrappedLabel);
                authoring.ColliderCenter  = EditorGUILayout.Vector3Field("Collider Center", authoring.ColliderCenter);
                authoring.ColliderExtents = EditorGUILayout.Vector3Field("Collider Extents", authoring.ColliderExtents);
            }
            
            else
            {
                return;
            }

            //DrawDefaultInspector();

        }
    }
}
