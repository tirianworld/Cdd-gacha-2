using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;


public class HeroSkinReferenceDrawer : PropertyDrawer
{
    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        EditorGUI.BeginProperty(position, label, property);

        var heroProp = property.FindPropertyRelative("targetHero");
        var indexProp = property.FindPropertyRelative("selectedSkinIndex");

        // Campo para elegir el Hero
        Rect heroRect = new Rect(position.x, position.y, position.width, EditorGUIUtility.singleLineHeight);
        EditorGUI.PropertyField(heroRect, heroProp);

        Hero hero = heroProp.objectReferenceValue as Hero;

       

        EditorGUI.EndProperty();
    }

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUIUtility.singleLineHeight * 2 + 4;
    }
}
#endif
