using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(ObstacleData))]
public class ObstacleEditor : Editor
{
    public override void OnInspectorGUI()
    {
        ObstacleData obstacleData = (ObstacleData)target;

        for (int i = 0; i < 10; i++)
        {
            EditorGUILayout.BeginHorizontal();
            for (int j = 0; j < 10; j++)
            {
                int index = i * 10 + j;
                obstacleData.obstacleGrid[index] = EditorGUILayout.Toggle(obstacleData.obstacleGrid[index], GUILayout.Width(20));
            }
            EditorGUILayout.EndHorizontal();
        }

        if (GUILayout.Button("Save"))
        {
            EditorUtility.SetDirty(obstacleData);
        }
    }
}
