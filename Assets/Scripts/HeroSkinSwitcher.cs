using UnityEngine;

public class HeroSkinSwitcher : MonoBehaviour
{
    [Header("Heroe objetivo")]
    public Hero targetHero;

    [Header("Objetos visuales por skin")]
    public GameObject[] skinObjects; // Cada índice corresponde a una skin

    private int lastSkinIndex = -1;
    void Start()
    {
        if (targetHero != null)
        {
            ApplySkin(targetHero.currentSkinIndex);
            lastSkinIndex = targetHero.currentSkinIndex;
        }
    }

    void Update()
    {
        if (targetHero == null || skinObjects.Length == 0) return;

        // Si cambió la skin activa, refrescamos
        if (targetHero.currentSkinIndex != lastSkinIndex)
        {
            ApplySkin(targetHero.currentSkinIndex);
            lastSkinIndex = targetHero.currentSkinIndex;
        }
    }

    public void ApplySkin(int index)
    {
        if (index < 0 || index >= skinObjects.Length) return;

        // Desactivar todos
        for (int i = 0; i < skinObjects.Length; i++)
        {
            if (skinObjects[i] != null)
                skinObjects[i].SetActive(i == index);
        }

        Debug.Log($"{targetHero.heroName} ahora muestra el objeto visual de la skin {index + 1}.");
    }
}
