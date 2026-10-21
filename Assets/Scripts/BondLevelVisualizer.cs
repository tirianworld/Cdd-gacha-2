using UnityEngine;

public class BondLevelVisualizer : MonoBehaviour
{
    [Header("Referencia al héroe")]
    public Hero hero;

    [Header("Objetos a activar según Bond Level (5 en total)")]
    public GameObject[] bondObjects = new GameObject[5];

    [Header("Bond Level máximo para activar los 5")]
    public int maxBondLevel = 100000;

    private void Update()
    {
        UpdateBondVisuals();
    }

    public void UpdateBondVisuals()
    {
        if (hero == null) return;

        // Calculamos el porcentaje del Bond Level
        float percent = Mathf.Clamp01((float)hero.BondLevel / maxBondLevel);

        // Cada objeto representa un 20%
        int activeCount = Mathf.FloorToInt(percent * 5);

        // Activar/desactivar objetos
        for (int i = 0; i < bondObjects.Length; i++)
        {
            if (bondObjects[i] != null)
                bondObjects[i].SetActive(i < activeCount);
        }
    }
}
