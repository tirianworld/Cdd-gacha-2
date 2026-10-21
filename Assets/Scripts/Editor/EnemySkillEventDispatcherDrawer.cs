#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(EnemySkillEventDispatcher))]
public class EnemySkillEventDispatcherDrawer : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var enemyProp = serializedObject.FindProperty("enemy");
        EditorGUILayout.PropertyField(enemyProp);

        var enemy = enemyProp.objectReferenceValue as Enemy;

        var eventsProp = serializedObject.FindProperty("onEnemySkillExecutedEvents");

        if (enemy == null)
        {
            EditorGUILayout.HelpBox("Asigna un Enemy para mostrar los eventos por skill.", MessageType.Info);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        if (enemy.heroSkills == null)
        {
            EditorGUILayout.HelpBox("El enemigo no tiene lista de HeroSkills.", MessageType.Warning);
            serializedObject.ApplyModifiedProperties();
            return;
        }

        for (int i = 0; i < eventsProp.arraySize; i++)
        {
            var skill = enemy.heroSkills[i];
            var eventProp = eventsProp.GetArrayElementAtIndex(i);

            string label = skill != null ? skill.skillName : $"Skill {i}";
            EditorGUILayout.PropertyField(eventProp, new GUIContent(label), true);
        }

        serializedObject.ApplyModifiedProperties();
    }
}
#endif
