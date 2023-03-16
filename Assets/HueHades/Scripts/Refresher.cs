using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

#if UNITY_EDITOR
[ExecuteInEditMode]
#endif
public class Refresher : MonoBehaviour
{
#if UNITY_EDITOR
    void Start()
    {
        AssetDatabase.Refresh();
        UnityEditor.Compilation.CompilationPipeline.RequestScriptCompilation();
        DestroyImmediate(this);
    }
#endif
}
