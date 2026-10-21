using System.Collections.Generic;
using UnityEngine;

public class GridNavigator : MonoBehaviour
{
    [Header("Padre del GridLayoutGroup")]
    public Transform contenedor;

    [Header("Cantidad visible a la vez")]
    public int cantidadVisible = 4;

    private List<Transform> elementos = new List<Transform>();

    void Start()
    {
        // Guardamos los hijos en orden
        foreach (Transform child in contenedor)
            elementos.Add(child);

        ActualizarVisibilidad();
    }

    public void Siguiente()
    {
        // Mover el primero al final
        Transform primero = elementos[0];
        elementos.RemoveAt(0);
        elementos.Add(primero);

        ActualizarOrden();
        ActualizarVisibilidad();
    }

    public void Anterior()
    {
        // Mover el último al inicio
        Transform ultimo = elementos[elementos.Count - 1];
        elementos.RemoveAt(elementos.Count - 1);
        elementos.Insert(0, ultimo);

        ActualizarOrden();
        ActualizarVisibilidad();
    }

    private void ActualizarOrden()
    {
        // Reordenar en la jerarquía
        for (int i = 0; i < elementos.Count; i++)
        {
            elementos[i].SetSiblingIndex(i);
        }
    }

    private void ActualizarVisibilidad()
    {
        for (int i = 0; i < elementos.Count; i++)
        {
            elementos[i].gameObject.SetActive(i < cantidadVisible);
        }
    }
}
