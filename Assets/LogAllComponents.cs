using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteAlways]
public class LogAllComponents : MonoBehaviour
{
    

    // Update is called once per frame
    void Update()
    {
        var components = GetComponents(typeof(UnityEngine.Component));
        foreach (var component in components)
        {
            Debug.Log(components.Length);
            Debug.Log(component.GetType());
        }
    }
}
