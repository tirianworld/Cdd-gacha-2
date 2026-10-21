using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Item_SO))]
public class ItemSOEditor : Editor
{

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();
        // Dibujar los campos por defecto
        Item_SO item = (Item_SO)target;
        // Mostrar la miniatura del sprite si existe
        if (item.icon != null)
        {
            GUILayout.Label("Icon");
            GUILayout.BeginHorizontal();
            GUILayout.FlexibleSpace();
            GUILayout.Label(item.icon.texture, GUILayout.Width(64), GUILayout.Height(64));
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

  

        // Obtener referencia al objeto
  

    }
}
