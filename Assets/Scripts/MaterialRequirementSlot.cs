using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Representa UN material requerido para una mejora (nivel/estrella/despertar).
/// Cambia el ícono según el Item_SO asignado, muestra "actual/requerido",
/// y activa un conjunto de objetos distinto según si se tiene suficiente
/// cantidad o no. Mismo patrón que SkinOwnershipIndicator (excluyente, refresco
/// en Update() o manual con Refresh()).
/// </summary>
public class MaterialRequirementSlot : MonoBehaviour
{
    // 🔥 GameManager fijo: siempre el singleton real, nunca una referencia
    // arrastrada a mano que podría quedar apuntando a una copia duplicada.
    private GameManager gamemanager => GameManager.Instance;

    [Header("Datos del requisito")]
    public Item_SO item;
    public int requiredAmount = 1;

    [Header("UI")]
    [Tooltip("Ícono del material. Se actualiza automáticamente con item.icon.")]
    public Image iconImage;
    public TextMeshProUGUI amountText;
    public TextMeshProUGUI nameText;

    [Header("Si TIENES suficiente cantidad del material (current >= requiredAmount)")]
    public GameObject[] enoughObjects;

    [Header("Si NO tienes suficiente cantidad del material")]
    public GameObject[] missingObjects;

    private void Start()
    {
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    // Llamar esto al instanciar el slot dinámicamente (ver EquipmentUpgradeMaterialsPanel)
    public void SetRequirement(Item_SO newItem, int newAmount)
    {
        item = newItem;
        requiredAmount = newAmount;
        Refresh();
    }

    public void Refresh()
    {
        if (gamemanager == null || gamemanager.playerInventory == null || item == null)
            return;

        // 🔥 Cambia la imagen del material según el Item_SO asignado a este requisito
        if (iconImage != null)
        {
            iconImage.sprite = item.icon;
            iconImage.enabled = item.icon != null;
        }

        if (nameText != null)
            nameText.text = item.itemName;

        int current = gamemanager.playerInventory.GetCount(item);
        bool enough = current >= requiredAmount;

        if (amountText != null)
            amountText.text = $"{current}/{requiredAmount}";

        SetActiveAll(enoughObjects, enough);
        SetActiveAll(missingObjects, !enough);
    }

    private void SetActiveAll(GameObject[] objects, bool active)
    {
        if (objects == null) return;

        foreach (var obj in objects)
        {
            if (obj != null && obj.activeSelf != active)
                obj.SetActive(active);
        }
    }
}
