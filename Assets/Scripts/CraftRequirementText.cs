using UnityEngine;
using TMPro;

/// <summary>
/// Muestra en un TextMeshProUGUI la cantidad que tiene el jugador de un item
/// junto a la cantidad necesaria para craftear, en formato "actual/necesario".
///
/// USO:
/// 1. Colocar este script en el mismo GameObject que tiene el TextMeshProUGUI
///    (por ejemplo, dentro de cada fila de requisito del panel de crafteo).
/// 2. Arrastrar el Crafter correspondiente en el campo "crafter".
/// 3. Arrastrar el Item_SO que esa fila representa en el campo "itemToShow"
///    (debe ser el mismo Item_SO que aparece en requirements de ese Crafter).
///
/// Busca el requisito por referencia de Item_SO en vez de por índice,
/// así no se rompe si cambia el orden de la lista de requisitos.
/// </summary>
[RequireComponent(typeof(TextMeshProUGUI))]
public class CraftRequirementText : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("Crafter que contiene la lista de requisitos")]
    public Crafter crafter;

    [Tooltip("Item cuyo requisito se quiere mostrar en este texto")]
    public Item_SO itemToShow;

    [Header("Colores (opcional)")]
    public bool colorearSegunCantidad = true;
    public Color colorSuficiente = Color.white;
    public Color colorInsuficiente = Color.red;

    [Header("Actualización")]
    [Tooltip("Cada cuántos segundos se refresca el texto. Usar 0 para refrescar cada frame.")]
    public float intervaloActualizacion = 0.25f;

    private TextMeshProUGUI text;

    private void Awake()
    {
        text = GetComponent<TextMeshProUGUI>();
    }

    private void OnEnable()
    {
        RefreshText();

        if (intervaloActualizacion > 0f)
            InvokeRepeating(nameof(RefreshText), intervaloActualizacion, intervaloActualizacion);
    }

    private void OnDisable()
    {
        CancelInvoke(nameof(RefreshText));
    }

    private void Update()
    {
        // Si se deja intervaloActualizacion en 0, se refresca cada frame en vez de con InvokeRepeating
        if (intervaloActualizacion <= 0f)
            RefreshText();
    }

    /// <summary>
    /// Actualiza manualmente el texto. Se puede llamar desde fuera
    /// (por ejemplo justo después de craftear) para forzar un refresco inmediato.
    /// </summary>
    public void RefreshText()
    {
        if (crafter == null || itemToShow == null)
            return;

        if (crafter.gameManager == null || crafter.gameManager.playerInventory == null)
            return;

        Crafter.CraftRequirement req = null;
        foreach (var r in crafter.requirements)
        {
            if (r.item == itemToShow)
            {
                req = r;
                break;
            }
        }

        if (req == null)
        {
            text.text = "-/-";
            return;
        }

        int current = crafter.gameManager.playerInventory.GetCount(itemToShow);
        int needed = req.amount;

        text.text = $"{current}/{needed}";

        if (colorearSegunCantidad)
            text.color = current >= needed ? colorSuficiente : colorInsuficiente;
    }
}
