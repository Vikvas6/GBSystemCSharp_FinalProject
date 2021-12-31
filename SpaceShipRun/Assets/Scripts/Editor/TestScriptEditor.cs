using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(TestScript))]
public class TestScriptEditor : Editor
{
    private TestScript targetComponent;

    public void OnEnable()
    {
        targetComponent = (TestScript)target;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();

        if (GUILayout.Button("value++"))
            targetComponent.value++;
    }
}
