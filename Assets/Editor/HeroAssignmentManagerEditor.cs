using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(HeroAssignmentManager))]
public class HeroAssignmentManagerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        HeroAssignmentManager manager = (HeroAssignmentManager)target;

        GUILayout.Space(10);
        GUI.backgroundColor = Color.green;

        if (GUILayout.Button("💾 Guardar héroes asignados"))
        {
            manager.SaveAllAssignedHeroes();
        }

        GUI.backgroundColor = Color.white;
    }

}
