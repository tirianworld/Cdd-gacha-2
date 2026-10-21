using UnityEngine;

public class TavernVisitor : MonoBehaviour
{
    [Header("Lista de visitantes posibles")]
    public GameObject[] visitors;

    void Start()
    {
        ActivateRandomVisitor();
    }

    public void ActivateRandomVisitor()
    {
        if (visitors == null || visitors.Length == 0)
        {
            Debug.LogWarning("No hay visitantes asignados en el array.");
            return;
        }

        // Elegir índice aleatorio
        int randomIndex = Random.Range(0, visitors.Length);

        // Activar solo el visitante aleatorio
        for (int i = 0; i < visitors.Length; i++)
        {
            visitors[i].SetActive(i == randomIndex);
        }
    }
}
