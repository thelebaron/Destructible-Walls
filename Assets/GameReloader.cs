using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public struct HelloWorld: IComponentData{}
public class GameReloader : MonoBehaviour
{
#if UNITY_EDITOR
    [UnityEditor.Callbacks.DidReloadScripts]
    private static void OnScriptsReloaded() {
        if (UnityEditor.EditorApplication.isPlaying) {
            UnityEditor.EditorApplication.ExitPlaymode();
            UnityEditor.EditorApplication.playModeStateChanged += EditorApplication_playModeStateChanged;
        }
    }
 
    private static void EditorApplication_playModeStateChanged(UnityEditor.PlayModeStateChange obj) {
        UnityEditor.EditorApplication.playModeStateChanged -= EditorApplication_playModeStateChanged;
        UnityEditor.EditorApplication.EnterPlaymode();
    }
#endif
}
