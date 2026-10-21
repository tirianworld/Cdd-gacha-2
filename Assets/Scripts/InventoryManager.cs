using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class InventoryManager : MonoBehaviour
{
    public GameManager GM;

    [Header("Slots ya creados en el Canvas")]
    public List<InventoryItemUI> itemSlots;

    private void OnEnable()
    {
        RefreshInventoryUI();
    }

    public void RefreshInventoryUI()
    {
        var inventory = GM.playerInventory.items;

        foreach (var slot in itemSlots)
        {
            // Buscar si el jugador tiene el item que representa este slot
            var entry = inventory.FirstOrDefault(e => e.item == slot.itemRepresented);

            if (entry != null)
            {
                slot.item = entry.item;
                slot.quantity = entry.count;
                slot.Refresh();
                slot.gameObject.SetActive(true);
            }
            else
            {
                slot.gameObject.SetActive(false);
            }
        }
    }
    public bool HasItem(Item_SO item, int amount)
    {
        var entry = GM.playerInventory.items.FirstOrDefault(e => e.item == item);
        return entry != null && entry.count >= amount;
    }

    public void RemoveItem(Item_SO item, int amount)
    {
        var entry = GM.playerInventory.items.FirstOrDefault(e => e.item == item);
        if (entry == null) return;

        entry.count -= amount;

        if (entry.count <= 0)
            GM.playerInventory.items.Remove(entry);

        GM.SaveInventory();
    }


}
