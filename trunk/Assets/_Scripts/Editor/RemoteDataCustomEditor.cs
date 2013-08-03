using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Collections;
using Object = UnityEngine.Object;

[CustomEditor(typeof(RemoteData))]
public class RemoteDataCustomEditor : Editor
{
    private RemoteData _remData;
    /// <summary>
    /// Заменяет собой стандартное содержание редактора компонента
    /// </summary>
    public override void OnInspectorGUI() {
        _remData = (RemoteData)target;
        GUI.changed = false;
        EditorGUILayout.Separator();
        if (GUILayout.Button("Кнопка для отладочных целей", GUILayout.MinHeight(30))) {
            //_remData.LoadAllAssets();
            GUISkin t = new GUISkin();
            GameObject go = new GameObject();
            DebugF.Log("Пробую определить тип : " + go.GetType());
            Type type = Types.GetType(t.GetType().ToString(), "UnityEngine");// Type.GetType(, true, false);
            string res;
            if (type == null)
                res = "null";
            else
                res = type.ToString();
            DebugF.Log("В результате : " + res);
        }
        EditorGUILayout.Separator();
        
        if (GUI.changed)
            EditorUtility.SetDirty(_remData);
        base.OnInspectorGUI();
    }

   
}
