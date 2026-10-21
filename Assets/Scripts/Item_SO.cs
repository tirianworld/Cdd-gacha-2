using UnityEngine;

public enum ItemRarity { Common, Uncommon, Rare, Epic, Legendary }

[CreateAssetMenu(menuName = "SDP/Item", fileName = "NewItem")]
public class Item_SO : ScriptableObject
{
    public string itemName = "New Item";
    [TextArea] public string description = "";
    public ItemRarity rarity = ItemRarity.Common;

    // Campo para el sprite

    public Sprite icon;
    [Header("Venta")]
    public int sellValue = 1;

}
