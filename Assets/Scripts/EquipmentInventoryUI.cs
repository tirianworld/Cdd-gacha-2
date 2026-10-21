using UnityEngine;
using System.Collections.Generic;

public class EquipmentInventoryUI : MonoBehaviour
{
    public GameObject slotPrefab;
    public Transform slotContainer;

    public Hero targetHero; // ← Héroe al que se equipará

    public void RefreshUI()
    {
        // 1. Limpiar slots actuales
        foreach (Transform child in slotContainer)
            Destroy(child.gameObject);

        // 2. Crear una lista rápida de todos los IDs de equipo que ya están en uso por algún héroe
        HashSet<string> equippedItemIDs = new HashSet<string>();
        foreach (Hero hero in GameManager.Instance.roster)
        {
            if (hero != null)
            {
                foreach (var eq in hero.equippedInstances)
                {
                    if (eq != null && !string.IsNullOrEmpty(eq.uniqueID))
                    {
                        equippedItemIDs.Add(eq.uniqueID);
                    }
                }
            }
        }

        // 3. Crear los slots solo para los items que NO están equipados
        foreach (var entry in GameManager.Instance.equipmentInventory)
        {
            // 🔥 Si el ID de este ítem está en la lista de equipados, nos lo saltamos
            if (equippedItemIDs.Contains(entry.instance.uniqueID))
                continue;

            GameObject slotObj = Instantiate(slotPrefab, slotContainer);
            EquipmentSlot slot = slotObj.GetComponent<EquipmentSlot>();

            slot.equipmentInstance = entry.instance;

            // Aquí asignamos el héroe
            slot.assignedHero = targetHero;

            slot.RefreshSlot();
        }
    }
}