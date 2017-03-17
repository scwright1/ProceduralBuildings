using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomEditor(typeof(BuildBuildingScript))]
public class BuildBuildingEditor : Editor {

    public override void OnInspectorGUI() {

        DrawDefaultInspector();

        BuildBuildingScript generator = (BuildBuildingScript)target;
        if (GUILayout.Button("Generate Building")) {
            generator.BuildObject();
        }
    }
}