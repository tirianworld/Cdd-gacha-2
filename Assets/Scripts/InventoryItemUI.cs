using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryItemUI : MonoBehaviour
{
    [Header("Item que representa este slot")]
    public Item_SO itemRepresented;   // ? ESTE ES EL CAMBIO IMPORTANTE

    [Header("Item Data")]
    public Item_SO item;
    public int quantity;

    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityText;

    [Header("Descripción")]
    public GameObject descriptionPanel;
    public TextMeshProUGUI descriptionText;
    public GameManager gamemanager;
    public void Refresh()
    {
        if (item == null) return;

        if (iconImage != null)
            iconImage.sprite = item.icon;

        if (nameText != null)
            nameText.text = item.itemName;

        if (quantityText != null)
            quantityText.text = quantity.ToString();
    }

    public void OnItemClicked()
    {
        if (descriptionPanel != null)
            descriptionPanel.SetActive(true);

        if (descriptionText != null)
            descriptionText.text = item.description;
    }
    public void RemoveOne()
    {
        if (item == null) return;

        // Restar en el inventario REAL
        gamemanager.RemoveOneFromInventory(item);

        // Leer la cantidad actualizada desde el inventario real
        var entry = gamemanager.playerInventory.items.Find(e => e.item == item);
        quantity = entry != null ? entry.count : 0;

        // Si ya no existe, ocultar el slot
        if (quantity <= 0)
            gameObject.SetActive(false);

        Refresh();
    }



}
