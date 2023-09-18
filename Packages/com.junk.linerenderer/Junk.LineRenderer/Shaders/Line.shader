// Unity built-in shader source. Copyright (c) 2016 Unity Technologies. MIT license (see license.txt)

// Unlit alpha-blended shader.
// - no lighting
// - no lightmap support
// - no per-material color

Shader "EcsLineRenderer/E7 Line"
{
    Properties
    {
        [Toggle] _IgnoreFog ("Ignore Fog", Float) = 1
		_FogStrength ("Fog Strength", Float) = 1

        _BaseMap ("Base (RGB) Trans (A)", 2D) = "white" {}
        _BaseColor ("Main Color", Color) = (1,1,1,1)
        
        _EmissionMap ("Emission", 2D) = "black" {}
        [HDR] _EmissionColor ("Emission Color", Color) = (0,0,0,0)
        
        // Blending state
        _Surface("__surface", Float) = 0.0
        _Blend("__blend", Float) = 0.0
        _Cull("__cull", Float) = 2.0
        [ToggleUI] _AlphaClip("__clip", Float) = 0.0
        [HideInInspector] _SrcBlend("__src", Float) = 1.0
        [HideInInspector] _DstBlend("__dst", Float) = 0.0
        [HideInInspector] _ZWrite("__zw", Float) = 1.0
    }

    SubShader
    {
        Tags {"RenderType" = "Opaque" "IgnoreProjector" = "True" "RenderPipeline" = "UniversalPipeline" "ShaderModel"="4.5"}
        LOD 300
        //LOD 100
        
        //ZWrite On
        //Blend SrcAlpha OneMinusSrcAlpha
        
        // Use same blending / depth states as Standard shader
        Blend[_SrcBlend][_DstBlend]
        ZWrite[_ZWrite]
        Cull[_Cull]

        Pass
        {
            //Name "ForwardLit"
            //Tags { "LightMode" = "UniversalForward" }
            
            HLSLPROGRAM
            #pragma exclude_renderers gles gles3 glcore
            #pragma target 4.5
            
            // -------------------------------------
            // Unity defined keywords
            #pragma multi_compile _ DIRLIGHTMAP_COMBINED
            #pragma multi_compile _ LIGHTMAP_ON
            #pragma multi_compile _ DYNAMICLIGHTMAP_ON
            #pragma multi_compile_fog
            #pragma multi_compile_fragment _ DEBUG_DISPLAY
            
            //--------------------------------------
            // GPU Instancing
            #pragma multi_compile_instancing
            #pragma instancing_options renderinglayer
            #pragma multi_compile _ DOTS_INSTANCING_ON
            #pragma shader_feature_local_fragment _ _IGNOREFOG_ON
            
            #pragma vertex LitPassVertexSimple
            #pragma fragment LitPassFragmentSimple
            
            #include "LineInput.hlsl"
            #include "LineForwardPass.hlsl"
            
            ENDHLSL
        }
    }
    Fallback  "Hidden/Universal Render Pipeline/FallbackError"
    CustomEditor "E7.LineRenderer.ShaderGUI.LineShaderGUI"
}

