using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class InventoryEntry
{
    public Item_SO item;
    public int count;
}

[System.Serializable]
public class Inventory
{
    public List<InventoryEntry> items = new List<InventoryEntry>();

    public bool AddItem(Item_SO item, int amount = 1)
    {
        if (item == null) return false;
        var entry = items.Find(e => e.item == item);
        if (entry == null)
        {
            entry = new InventoryEntry { item = item, count = 0 };
            items.Add(entry);
        }
        entry.count += amount;
        return true;
    }

    public bool RemoveItem(Item_SO item, int amount = 1)
    {
        var entry = items.Find(e => e.item == item);
        if (entry == null || entry.count < amount) return false;

        entry.count -= amount;
        if (entry.count <= 0) items.Remove(entry);
        return true;
    }

    public int GetCount(Item_SO item)
    {
        var entry = items.Find(e => e.item == item);
        return entry != null ? entry.count : 0;
    }

    public void Clear() => items.Clear();

}