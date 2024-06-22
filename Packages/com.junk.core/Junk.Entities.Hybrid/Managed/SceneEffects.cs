using System;
using UnityEngine;
using UnityEngine.VFX;
using Object = UnityEngine.Object;

namespace Junk.Entities.Hybrid
{
    [AddComponentMenu("Game/Scene Effects")]
    public class SceneEffects : MonoBehaviour
    {
        [SerializeField] private VisualEffectResources ManagedEffectsContainer;
        private                  VisualEffect         sparkGraph;
        private                  VisualEffect         smokeGraph;
        private                  VisualEffect         bloodHeadshotGraph;
        private                  VisualEffect         bloodSprayGraph;
        private                  VisualEffect         moverGraph;

        #if UNITY_EDITOR
        private void OnValidate()
        {
            if(ManagedEffectsContainer== null)
                ManagedEffectsContainer = VisualEffectResources.GetOrCreateSettings();
        }
        #endif  
        
        public void Awake()
        {
            sparkGraph         = Object.Instantiate(ManagedEffectsContainer.SparkEffect.gameObject).GetComponent<VisualEffect>();
            smokeGraph         = Object.Instantiate(ManagedEffectsContainer.SmokeEffect.gameObject).GetComponent<VisualEffect>();
            bloodHeadshotGraph = Object.Instantiate(ManagedEffectsContainer.BloodHeadshotEffect.gameObject).GetComponent<VisualEffect>();
            bloodSprayGraph    = Object.Instantiate(ManagedEffectsContainer.BloodSprayEffect.gameObject).GetComponent<VisualEffect>();
            moverGraph         = Object.Instantiate(ManagedEffectsContainer.TrailMoverEffect.gameObject).GetComponent<VisualEffect>();
            
            // Parent to this game object
            sparkGraph.transform.SetParent(transform);
            smokeGraph.transform.SetParent(transform);
            bloodHeadshotGraph.transform.SetParent(transform);
            bloodSprayGraph.transform.SetParent(transform);
            moverGraph.transform.SetParent(transform);
            
            VFXReferences.SparksGraph        = sparkGraph;
            VFXReferences.SmokeGraph         = smokeGraph;
            VFXReferences.BloodSprayGraph  = bloodSprayGraph;
            VFXReferences.BloodHeadshotGraph = bloodHeadshotGraph;
            VFXReferences.TrailGraph         = moverGraph;
        }
    }
}