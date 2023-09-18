
using System;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;

namespace Junk.LineRenderer.ShaderGUI
{

    /// <summary>
    /// This is bugged: enable/disable properties are not working.
    /// You must enable inspector debug mode
    /// under keywords find _IgnoreFog and set 0/1 to enable/disable fog.
    /// 
    /// </summary>
    public class LineShaderGUI : BaseShaderGUI
    {
        public static readonly string IGNOREFOG = "_IgnoreFog";
        public static readonly string FOGSTRENGTH = "_FogStrength";

        public static readonly GUIContent       RetroOptions         = EditorGUIUtility.TrTextContent("Retro Options", "Options orientated around achieving a retro fps look.");
        public static readonly GUIContent       ignoreFogEnabledText = EditorGUIUtility.TrTextContent("Ignore Fog", "Material will be unaffected by fog.");
        protected              MaterialProperty ignoreFogProp { get; set; }
        
        public static readonly GUIContent       fogStrengthText = EditorGUIUtility.TrTextContent("Fog Strength", "How much fog affects the material.");
        protected              MaterialProperty fogStrengthProp { get; set; }
        public override void FindProperties(MaterialProperty[] properties)
        {
            base.FindProperties(properties);

            ignoreFogProp   = FindProperty(IGNOREFOG, properties, true);
            fogStrengthProp = FindProperty(FOGSTRENGTH, properties, true);
        }
        
        internal static void DrawFloatProperty(GUIContent styles, MaterialProperty prop)
        {
            if (prop == null)
                return;

            EditorGUI.BeginChangeCheck();
            EditorGUI.showMixedValue = prop.hasMixedValue;
            float newValue = EditorGUILayout.FloatField(styles, prop.floatValue);
            if (EditorGUI.EndChangeCheck())
                prop.floatValue = newValue;
            EditorGUI.showMixedValue = false;
        }
        
        public override void OnGUI(MaterialEditor materialEditorIn, MaterialProperty[] properties)
        {
            base.OnGUI(materialEditorIn, properties);
            // Note: I think calling above already calls DrawSurfaceOptions(material);
            
            var material = materialEditorIn.target as Material;
            DrawFogOptions(material);
            DrawEmissionProperties(material, true);
        }
        
        public virtual void DrawFogOptions(Material material)
        {
            if (ignoreFogProp == null)
            {
                Debug.Log("No ignore fog property found.");
                return;
            }
            DrawFloatToggleProperty(/*material, IGNOREFOG, */ ignoreFogEnabledText, ignoreFogProp);
            if (fogStrengthProp == null)
            {
                Debug.Log("No fog strength property found.");
                return;
            }
            DrawFloatProperty(fogStrengthText, fogStrengthProp);
        }
        

        // material changed check
        public override void ValidateMaterial(Material material)
        {
            SetMaterialKeywords(material);
        }
        private bool GetBool(float value)
        {
            return value > 0.5f;
        }
        
        // material main surface inputs
        public override void DrawSurfaceInputs(Material material)
        {
            base.DrawSurfaceInputs(material);
            DrawTileOffset(materialEditor, baseMapProp);
        }

        public override void AssignNewShaderToMaterial(Material material, Shader oldShader, Shader newShader)
        {
            if (material == null)
                throw new ArgumentNullException("material");

            // _Emission property is lost after assigning Standard shader to the material
            // thus transfer it before assigning the new shader
            if (material.HasProperty("_Emission"))
            {
                material.SetColor("_EmissionColor", material.GetColor("_Emission"));
            }
            
            if (material.HasProperty(IGNOREFOG))
            {
                material.SetColor(IGNOREFOG, material.GetColor(IGNOREFOG));
            }
            
            base.AssignNewShaderToMaterial(material, oldShader, newShader);

            if (oldShader == null || !oldShader.name.Contains("Legacy Shaders/"))
            {
                SetupMaterialBlendMode(material);
                return;
            }

            SurfaceType surfaceType = SurfaceType.Opaque;
            BlendMode blendMode = BlendMode.Alpha;
            if (oldShader.name.Contains("/Transparent/Cutout/"))
            {
                surfaceType = SurfaceType.Opaque;
                material.SetFloat("_AlphaClip", 1);
            }
            else if (oldShader.name.Contains("/Transparent/"))
            {
                // NOTE: legacy shaders did not provide physically based transparency
                // therefore Fade mode
                surfaceType = SurfaceType.Transparent;
                blendMode = BlendMode.Alpha;
            }
            material.SetFloat("_Blend", (float)blendMode);

            material.SetFloat("_Surface", (float)surfaceType);
            if (surfaceType == SurfaceType.Opaque)
            {
                material.DisableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
            else
            {
                material.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");
            }
        }
    }
}