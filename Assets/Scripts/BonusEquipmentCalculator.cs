using UnityEngine;
using TMPro;

// ============================================================
//   BonusEquipmentCalculator
//   Calcula cuánto mejora el equipo actualmente equipado a un
//   héroe y lo muestra en TextMeshPro (uno por estadística, o
//   un resumen conjunto). Suma tanto los bonos base del
//   Equipment_SO (healthBonus/attackBonus/defenseBonus) como los
//   bonos generados por nivel/estrellas/despertar de cada
//   EquipmentInstance — misma fórmula que ya usa
//   EquipmentSlot.ApplyInstanceBonuses() / Hero.Equip() / Hero.Unequip(),
//   así que los números coinciden con los stats reales del héroe.
//
//   Uso típico: colocarlo en el panel de detalle del héroe, arrastrar
//   el Hero objetivo y los TMP donde quieres ver el bono. Llamar a
//   RefreshBonuses() cada vez que se equipe/desequipe algo (o dejar
//   autoRefreshOnUpdate activado si preferís que se recalcule solo,
//   igual que hace EquipmentGridSorter con su Update()).
// ============================================================
public class BonusEquipmentCalculator : MonoBehaviour
{
    [Header("Héroe a analizar")]
    public Hero targetHero;

    [Header("Textos de bono TOTAL (suma de todo el equipo puesto)")]
    public TextMeshProUGUI healthBonusText;
    public TextMeshProUGUI attackBonusText;
    public TextMeshProUGUI defenseBonusText;

    [Header("Texto resumen opcional (una sola línea con todo junto)")]
    public TextMeshProUGUI summaryText;

    [Header("Formato de texto")]
    public string healthPrefix = "Vida: ";
    public string attackPrefix = "Ataque: ";
    public string defensePrefix = "Defensa: ";
    public bool showPlusSign = true;

    [Header("Mostrar además el % de mejora respecto al héroe sin equipo")]
    public bool showPercentage = false;

    [Header("Auto refresco")]
    [Tooltip("Si está activo, recalcula cada frame (igual que EquipmentGridSorter.Update()). " +
             "Si preferís más control, dejalo apagado y llamá a RefreshBonuses() manualmente " +
             "(por ejemplo desde GameManager.RefreshEquippedSlots o justo después de EquipToHero/Unequip).")]
    public bool autoRefreshOnUpdate = false;

    // Totales calculados en el último RefreshBonuses(), por si otro script los necesita
    public float TotalHealthBonus { get; private set; }
    public float TotalAttackBonus { get; private set; }
    public float TotalDefenseBonus { get; private set; }

    private void Start()
    {
        RefreshBonuses();
    }

    private void OnEnable()
    {
        RefreshBonuses();
    }

    private void Update()
    {
        if (autoRefreshOnUpdate)
            RefreshBonuses();
    }

    // ============================================================
    //   RECALCULAR TODO Y PINTAR LOS TMP
    // ============================================================
    public void RefreshBonuses()
    {
        if (targetHero == null)
        {
            SetEmpty();
            return;
        }

        float health = 0f;
        float attack = 0f;
        float defense = 0f;

        foreach (var inst in targetHero.equippedInstances)
        {
            if (inst == null || inst.baseData == null) continue;

            // Bono base del Equipment_SO (fijo, no cambia con nivel/estrellas/despertar)
            health += inst.baseData.healthBonus;
            attack += inst.baseData.attackBonus;
            defense += inst.baseData.defenseBonus;

            // Bono adicional según la instancia del jugador (nivel/estrellas/despertar).
            // Misma fórmula que EquipmentSlot.ApplyInstanceBonuses().
            health += inst.level * 2f;
            attack += inst.stars * 1.5f;
            defense += inst.awakening * 3f;
        }

        TotalHealthBonus = health;
        TotalAttackBonus = attack;
        TotalDefenseBonus = defense;

        PaintUI();
    }

    private void PaintUI()
    {
        string sign = showPlusSign ? "+" : "";

        if (healthBonusText != null)
            healthBonusText.text =
                $"{healthPrefix}{sign}{FormatValue(TotalHealthBonus)}{PercentSuffix(TotalHealthBonus, BaseHealthWithoutEquipment())}";

        if (attackBonusText != null)
            attackBonusText.text =
                $"{attackPrefix}{sign}{FormatValue(TotalAttackBonus)}{PercentSuffix(TotalAttackBonus, BaseAttackWithoutEquipment())}";

        if (defenseBonusText != null)
            defenseBonusText.text =
                $"{defensePrefix}{sign}{FormatValue(TotalDefenseBonus)}{PercentSuffix(TotalDefenseBonus, BaseDefenseWithoutEquipment())}";

        if (summaryText != null)
        {
            summaryText.text =
                $"{healthPrefix}{sign}{FormatValue(TotalHealthBonus)}   " +
                $"{attackPrefix}{sign}{FormatValue(TotalAttackBonus)}   " +
                $"{defensePrefix}{sign}{FormatValue(TotalDefenseBonus)}";
        }
    }

    private void SetEmpty()
    {
        TotalHealthBonus = 0f;
        TotalAttackBonus = 0f;
        TotalDefenseBonus = 0f;

        if (healthBonusText != null) healthBonusText.text = $"{healthPrefix}+0";
        if (attackBonusText != null) attackBonusText.text = $"{attackPrefix}+0";
        if (defenseBonusText != null) defenseBonusText.text = $"{defensePrefix}+0";
        if (summaryText != null) summaryText.text = "";
    }

    // ============================================================
    //   Stats del héroe SIN el bono de equipo actual, para poder
    //   calcular el % de mejora. hero.stats YA incluye el equipo
    //   puesto (así lo aplican Hero.Equip / EquipmentSlot.EquipToHero),
    //   así que simplemente restamos el total que acabamos de sumar.
    // ============================================================
    private float BaseHealthWithoutEquipment() => targetHero.stats.maxHealth - TotalHealthBonus;
    private float BaseAttackWithoutEquipment() => targetHero.stats.attack - TotalAttackBonus;
    private float BaseDefenseWithoutEquipment() => targetHero.stats.defense - TotalDefenseBonus;

    private string PercentSuffix(float bonus, float baseValue)
    {
        if (!showPercentage || baseValue <= 0f) return "";
        float pct = (bonus / baseValue) * 100f;
        return $" ({pct:0.#}%)";
    }

    private string FormatValue(float value)
    {
        // Sin decimales si el número ya es "entero limpio", si no, 1 decimal
        return Mathf.Approximately(value, Mathf.Round(value))
            ? value.ToString("0")
            : value.ToString("0.#");
    }
}
