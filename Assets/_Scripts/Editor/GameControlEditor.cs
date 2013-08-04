using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameControl))]
public class GameControlEditor : Editor
{
    private GameControl _gameControl;

    /// <summary>
    /// Заменяет собой стандартное содержание редактора компонента
    /// </summary>
    public override void OnInspectorGUI() {
        _gameControl = (GameControl)target;
        GUI.changed = false;
        EditorGUILayout.Separator();
        if (GUILayout.Button("А ну быстро на следующий уровень", GUILayout.MinHeight(30))) {
            _gameControl.Test();
        }
        EditorGUILayout.Separator();
        
        if (GUI.changed)
            EditorUtility.SetDirty(_gameControl);
        base.OnInspectorGUI();
    }
}