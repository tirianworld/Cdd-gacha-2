using UnityEngine;
using UnityEngine.UI;

public class AddItemButton : MonoBehaviour
{
    public Item_SO itemToAdd;   // Ítem que se añadirá al inventario
    public int maxAmount = 1;   // Cantidad máxima posible (mínimo siempre será 1)
    public Button button;

    private void Start()
    {
        if (button == null)
            button = GetComponent<Button>();

        button.onClick.AddListener(AddItem);
    }

    private void AddItem()
    {
        if (itemToAdd == null)
        {
            Debug.LogWarning("No hay item asignado al botón.");
            return;
        }

        // Cantidad aleatoria entre 1 y maxAmount
        int amount = Random.Range(1, maxAmount + 1);

        // Añadir al inventario del GameManager
        GameManager.Instance.playerInventory.AddItem(itemToAdd, amount);

        // Guardar inventario
        GameManager.Instance.SaveInventory();

        // Refrescar UI si existe un InventoryManager activo
        var invUI = FindObjectOfType<InventoryManager>();
        if (invUI != null)
            invUI.RefreshInventoryUI();

        Debug.Log($"Añadido {amount}x {itemToAdd.itemName} al inventario.");
    }
}
