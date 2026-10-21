using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Crafter : MonoBehaviour
{
    [Header("Referencia al GameManager")]
    public GameManager gameManager;

    [System.Serializable]
    public class CraftRequirement
    {
        public Item_SO item;
        public int amount = 1;
    }

    [Header("Requisitos para craftear")]
    public List<CraftRequirement> requirements = new List<CraftRequirement>();

    [Header("Resultado del crafteo (Item_SO o Food_SO)")]
    public ScriptableObject result;

    [Header("Eventos")]
    public UnityEvent onCraftSuccess;
    public UnityEvent onCraftFailed;

    private void Awake()
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;
    }

    public void TryCraft()
    {
        if (CanCraft())
        {
            ConsumeItems();
            GiveResult();
            onCraftSuccess?.Invoke();
            Debug.Log("Crafteo completado con éxito.");
        }
        else
        {
            onCraftFailed?.Invoke();
            Debug.Log("No tienes los materiales necesarios.");
        }
    }

    private bool CanCraft()
    {
        foreach (var req in requirements)
        {
            if (req.item == null)
            {
                Debug.LogError("Requisito sin Item_SO asignado.");
                return false;
            }

            int current = gameManager.playerInventory.GetCount(req.item);
            if (current < req.amount)
                return false;
        }

        return true;
    }

    private void ConsumeItems()
    {
        foreach (var req in requirements)
        {
            gameManager.playerInventory.RemoveItem(req.item, req.amount);
        }

        gameManager.SaveInventory();
    }

    private void GiveResult()
    {
        if (result == null)
        {
            Debug.LogWarning("No hay resultado asignado al Crafter.");
            return;
        }

        // 🔥 IMPORTANTE: Equipment_SO hereda de Item_SO, así que este chequeo
        // debe ir ANTES que el de Item_SO. Si no, el equipo craftado terminaría
        // metido en el inventario normal (playerInventory) en vez de en el
        // inventario de equipo (equipmentInventory), y nunca sería equipable.
        if (result is Equipment_SO equipment)
        {
            gameManager.AddEquipment(equipment, 1);
        }
        // Si es Item_SO (y no Equipment_SO) → añadir al inventario normal
        else if (result is Item_SO item)
        {
            gameManager.playerInventory.AddItem(item, 1);
        }
        // Si es Food_SO → añadir como comida (si tienes un inventario de comida)

    }
}