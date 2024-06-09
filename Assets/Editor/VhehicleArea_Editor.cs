using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(VehicleArea))]
public class VhehicleArea_Editor : Editor
{
    private void OnSceneGUI()
    {
        VehicleArea controller = (VehicleArea)target;

        controller._minPos = Handles.PositionHandle(controller._minPos, Quaternion.identity);
        controller._maxPos = Handles.PositionHandle(controller._maxPos, Quaternion.identity);
    }
}
