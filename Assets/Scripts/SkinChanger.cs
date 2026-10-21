using UnityEngine;

public class SkinChanger : MonoBehaviour
{
    public Hero hero;                     // Para saber de qué héroe son las skins
    public SkinManager skinManager;       // Referencia al SkinManager

    public GameObject[] skinObjects;      // Se activa si está equipada
    public GameObject[] occupiedObjects;  // Se activa si está ocupada (unlocked o equipped)

    private void Update()
    {
        if (skinManager == null || hero == null) return;

        for (int i = 0; i < skinManager.skins.Length; i++)
        {
            bool unlocked = skinManager.skins[i].unlocked;
            bool equipped = skinManager.skins[i].equipped;

            if (i < skinObjects.Length)
            {
                if (equipped)
                    skinObjects[i].SetActive(true);
                else
                    skinObjects[i].SetActive(false);
            }


            // Activar indicador de ocupado
            if (i < occupiedObjects.Length)
                occupiedObjects[i].SetActive(unlocked || equipped);
        }
    }

    public void SelectSkin(int index)
    {
        if (skinManager == null) return;

        // Desbloquear y equipar en el SkinManager
        // (EquipSkin ya llama a SaveSkins() internamente, no hace falta repetirlo aquí)
        skinManager.UnlockSkin(index);
        skinManager.EquipSkin(index);

        // 🔥 Desactivar todas las skins visuales
        for (int i = 0; i < skinObjects.Length; i++)
            skinObjects[i].SetActive(i == index);

        // 🔥 Actualizar los indicadores de ocupado
        for (int i = 0; i < occupiedObjects.Length && i < skinManager.skins.Length; i++)
            occupiedObjects[i].SetActive(i == index || skinManager.skins[i].unlocked);

        Debug.Log("Skin seleccionada: " + index);
    }

}