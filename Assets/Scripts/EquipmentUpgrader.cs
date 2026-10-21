using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;

// ============================================================
//   EQUIPMENT UPGRADER
//   Lee el equipamiento que se tiene (una EquipmentInstance
//   concreta del inventario, o la ya equipada en un héroe) y
//   permite mejorarla:
//
//     - Subir NIVEL
//     - Subir ESTRELLA
//     - Subir DESPERTAR
//
//   Cada mejora puede pagarse con ORO o con GEMAS (a elección
//   del jugador), más materiales que son DISTINTOS según el
//   Equipment_SO.Type del ítem (anillo, sello, casco, arma,
//   armadura).
// ============================================================
public class EquipmentUpgrader : MonoBehaviour
{
    // 🔥 GameManager fijo: siempre resuelve contra el singleton real
    // (GameManager.Instance), nunca una referencia arrastrada a mano
    // que podría apuntar a una copia duplicada/instanciada.
    public GameManager gameManager => GameManager.Instance;

    public enum CurrencyType { Gold, Gems }
    public enum UpgradeStage { Level, Star, Awakening }

    [Header("Instancia objetivo (el equipo que se va a mejorar)")]
    public EquipmentInstance targetInstance;

    [Header("Héroe dueño (si esta instancia está equipada, para recalcular stats)")]
    public Hero ownerHero;

    [Header("UI opcional a refrescar tras mejorar")]
    public EquipmentSlot linkedCatalogSlot;          // slot del catálogo/inventario
    public EquippedEquipmentSlot linkedEquippedSlot; // slot del panel "equipo puesto"

    // -------------------------------
    //   TOPES DE MEJORA
    // -------------------------------
    [Header("Topes")]
    public int maxLevel = 60;
    public int maxStars = 5;
    public int maxAwakening = 3;

    // -------------------------------
    //   COSTE EN ORO / GEMAS POR ETAPA
    // -------------------------------
    [System.Serializable]
    public class UpgradeCostConfig
    {
        [Header("Oro")]
        public int baseGoldCost = 100;
        public float goldMultiplier = 1.15f;

        [Header("Gemas")]
        public int baseGemCost = 50;
        public float gemMultiplier = 1.15f;

        public int GetCost(CurrencyType currency, int currentTier)
        {
            if (currency == CurrencyType.Gold)
                return Mathf.RoundToInt(baseGoldCost * Mathf.Pow(goldMultiplier, currentTier));
            else
                return Mathf.RoundToInt(baseGemCost * Mathf.Pow(gemMultiplier, currentTier));
        }
    }

    [Header("Coste de NIVEL")]
    public UpgradeCostConfig levelCost = new UpgradeCostConfig();

    [Header("Coste de ESTRELLA")]
    public UpgradeCostConfig starCost = new UpgradeCostConfig { baseGoldCost = 150, baseGemCost = 50 };

    [Header("Coste de DESPERTAR")]
    public UpgradeCostConfig awakeningCost = new UpgradeCostConfig { baseGoldCost = 400, baseGemCost = 150 };

    // -------------------------------
    //   MATERIALES, DISTINTOS POR TIPO DE EQUIPO
    // -------------------------------
    [System.Serializable]
    public class UpgradeRequirement
    {
        public Item_SO item;
        public int amount = 1;
    }

    [System.Serializable]
    public class TypeMaterialRequirement
    {
        public Equipment_SO.Type equipmentType;

        [Header("Materiales para subir de NIVEL")]
        public List<UpgradeRequirement> levelUpMaterials = new List<UpgradeRequirement>();

        [Header("Materiales para subir de ESTRELLA")]
        public List<UpgradeRequirement> starUpMaterials = new List<UpgradeRequirement>();

        [Header("Materiales para subir de DESPERTAR")]
        public List<UpgradeRequirement> awakeningUpMaterials = new List<UpgradeRequirement>();
    }

    [Header("Materiales por tipo de equipo (uno por cada anillo/sello/casco/arma/armadura)")]
    public List<TypeMaterialRequirement> materialsByType = new List<TypeMaterialRequirement>();

    [Header("Eventos")]
    public UnityEvent onUpgradeSuccess;
    public UnityEvent onUpgradeFailed;

    // ============================================================
    //   ASIGNAR OBJETIVO
    // ============================================================
    public void SetTarget(EquipmentInstance instance, Hero owner = null,
        EquipmentSlot catalogSlot = null, EquippedEquipmentSlot equippedSlot = null)
    {
        targetInstance = instance;
        ownerHero = owner;
        linkedCatalogSlot = catalogSlot;
        linkedEquippedSlot = equippedSlot;
    }

    // ============================================================
    //   MATERIALES CORRESPONDIENTES AL TIPO DEL OBJETIVO ACTUAL
    // ============================================================
    public List<UpgradeRequirement> GetMaterialsForCurrentTarget(UpgradeStage stage)
    {
        if (targetInstance == null || targetInstance.baseData == null) return null;
        return GetMaterialsForType(targetInstance.baseData.type, stage);
    }

    public List<UpgradeRequirement> GetMaterialsForType(Equipment_SO.Type type, UpgradeStage stage)
    {
        var entry = materialsByType.FirstOrDefault(m => m.equipmentType == type);
        if (entry == null) return null;

        switch (stage)
        {
            case UpgradeStage.Level: return entry.levelUpMaterials;
            case UpgradeStage.Star: return entry.starUpMaterials;
            case UpgradeStage.Awakening: return entry.awakeningUpMaterials;
            default: return null;
        }
    }

    // ============================================================
    //   COSTES PARA LA SIGUIENTE MEJORA (para pintar en la UI)
    // ============================================================
    public int GetCostForNextLevel(CurrencyType currency)
    {
        if (targetInstance == null) return 0;
        return levelCost.GetCost(currency, targetInstance.level - 1);
    }

    public int GetCostForNextStar(CurrencyType currency)
    {
        if (targetInstance == null) return 0;
        return starCost.GetCost(currency, targetInstance.stars);
    }

    public int GetCostForNextAwakening(CurrencyType currency)
    {
        if (targetInstance == null) return 0;
        return awakeningCost.GetCost(currency, targetInstance.awakening);
    }

    // ============================================================
    //   WRAPPERS SIN PARÁMETROS -> para enganchar directo a Button.OnClick
    // ============================================================
    public void TryUpgradeLevelWithGold() => TryUpgradeLevel(CurrencyType.Gold);
    public void TryUpgradeLevelWithGems() => TryUpgradeLevel(CurrencyType.Gems);
    public void TryUpgradeStarWithGold() => TryUpgradeStar(CurrencyType.Gold);
    public void TryUpgradeStarWithGems() => TryUpgradeStar(CurrencyType.Gems);
    public void TryUpgradeAwakeningWithGold() => TryUpgradeAwakening(CurrencyType.Gold);
    public void TryUpgradeAwakeningWithGems() => TryUpgradeAwakening(CurrencyType.Gems);

    // ============================================================
    //   SUBIR NIVEL
    // ============================================================
    public bool TryUpgradeLevel(CurrencyType currency)
    {
        if (!ValidateBasics()) return Fail();

        if (targetInstance.level >= maxLevel)
        {
            Debug.Log($"{targetInstance.baseData.itemName} ya está al nivel máximo ({maxLevel}).");
            return Fail();
        }

        int cost = GetCostForNextLevel(currency);
        var materials = GetMaterialsForCurrentTarget(UpgradeStage.Level);

        if (!HasEnoughCurrency(currency, cost)) return Fail();
        if (!HasEnoughMaterials(materials)) return Fail();

        SpendCurrency(currency, cost);
        ConsumeMaterials(materials);

        targetInstance.level++;

        // El bonus por nivel en EquipmentSlot.ApplyInstanceBonuses es "level * 2f" de vida máxima.
        // Al subir 1 nivel, el delta es siempre +2f.
        ApplyStatDelta(maxHealthDelta: 2f);

        SaveAndRefresh();
        onUpgradeSuccess?.Invoke();
        Debug.Log($"{targetInstance.baseData.itemName} subió a nivel {targetInstance.level} (pagado con {currency}).");
        return true;
    }

    // ============================================================
    //   SUBIR ESTRELLA
    // ============================================================
    public bool TryUpgradeStar(CurrencyType currency)
    {
        if (!ValidateBasics()) return Fail();

        if (targetInstance.stars >= maxStars)
        {
            Debug.Log($"{targetInstance.baseData.itemName} ya tiene el máximo de estrellas ({maxStars}).");
            return Fail();
        }

        int cost = GetCostForNextStar(currency);
        var materials = GetMaterialsForCurrentTarget(UpgradeStage.Star);

        if (!HasEnoughCurrency(currency, cost)) return Fail();
        if (!HasEnoughMaterials(materials)) return Fail();

        SpendCurrency(currency, cost);
        ConsumeMaterials(materials);

        targetInstance.stars++;

        // El bonus por estrella en EquipmentSlot.ApplyInstanceBonuses es "stars * 1.5f" de ataque.
        // Al subir 1 estrella, el delta es siempre +1.5f.
        ApplyStatDelta(attackDelta: 1.5f);

        SaveAndRefresh();
        onUpgradeSuccess?.Invoke();
        Debug.Log($"{targetInstance.baseData.itemName} subió a {targetInstance.stars} estrella(s) (pagado con {currency}).");
        return true;
    }

    // ============================================================
    //   SUBIR DESPERTAR
    // ============================================================
    public bool TryUpgradeAwakening(CurrencyType currency)
    {
        if (!ValidateBasics()) return Fail();

        if (targetInstance.awakening >= maxAwakening)
        {
            Debug.Log($"{targetInstance.baseData.itemName} ya tiene el despertar máximo ({maxAwakening}).");
            return Fail();
        }

        int cost = GetCostForNextAwakening(currency);
        var materials = GetMaterialsForCurrentTarget(UpgradeStage.Awakening);

        if (!HasEnoughCurrency(currency, cost)) return Fail();
        if (!HasEnoughMaterials(materials)) return Fail();

        SpendCurrency(currency, cost);
        ConsumeMaterials(materials);

        targetInstance.awakening++;

        // El bonus por despertar en EquipmentSlot.ApplyInstanceBonuses es "awakening * 3f" de defensa.
        // Al subir 1 despertar, el delta es siempre +3f.
        ApplyStatDelta(defenseDelta: 3f);

        SaveAndRefresh();
        onUpgradeSuccess?.Invoke();
        Debug.Log($"{targetInstance.baseData.itemName} subió a despertar {targetInstance.awakening} (pagado con {currency}).");
        return true;
    }

    // ============================================================
    //   VALIDACIÓN COMÚN
    // ============================================================
    private bool ValidateBasics()
    {
        if (gameManager == null)
        {
            Debug.LogError("EquipmentUpgrader: no se encontró GameManager.Instance en la escena.");
            return false;
        }

        if (targetInstance == null || targetInstance.baseData == null)
        {
            Debug.LogWarning("EquipmentUpgrader: no hay ninguna instancia de equipo asignada para mejorar.");
            return false;
        }

        return true;
    }

    private bool Fail()
    {
        onUpgradeFailed?.Invoke();
        return false;
    }

    // ============================================================
    //   ORO / GEMAS
    // ============================================================
    private bool HasEnoughCurrency(CurrencyType currency, int cost)
    {
        int available = currency == CurrencyType.Gold ? gameManager.Gold : gameManager.MemoryCristals;

        if (available < cost)
        {
            string nombre = currency == CurrencyType.Gold ? "Oro" : "Gemas";
            Debug.Log($"{nombre} insuficiente. Necesitas {cost}, tienes {available}.");
            return false;
        }

        return true;
    }

    private void SpendCurrency(CurrencyType currency, int cost)
    {
        if (currency == CurrencyType.Gold)
            gameManager.Gold -= cost;
        else
            gameManager.MemoryCristals -= cost;
    }

    // ============================================================
    //   MATERIALES (mismo patrón que Crafter.CanCraft/ConsumeItems)
    // ============================================================
    private bool HasEnoughMaterials(List<UpgradeRequirement> materials)
    {
        if (materials == null || materials.Count == 0) return true;

        foreach (var req in materials)
        {
            if (req.item == null)
            {
                Debug.LogError("EquipmentUpgrader: hay un material sin Item_SO asignado.");
                return false;
            }

            int current = gameManager.playerInventory.GetCount(req.item);
            if (current < req.amount)
            {
                Debug.Log($"Materiales insuficientes: {req.item.itemName} ({current}/{req.amount}).");
                return false;
            }
        }

        return true;
    }

    private void ConsumeMaterials(List<UpgradeRequirement> materials)
    {
        if (materials == null || materials.Count == 0) return;

        foreach (var req in materials)
        {
            gameManager.playerInventory.RemoveItem(req.item, req.amount);
        }

        gameManager.SaveInventory();
    }

    // ============================================================
    //   APLICAR EL DELTA DE STATS AL HÉROE (si está equipado)
    // ============================================================
    private void ApplyStatDelta(float maxHealthDelta = 0f, float attackDelta = 0f, float defenseDelta = 0f)
    {
        if (ownerHero == null) return;

        // Solo aplica si esta instancia está realmente puesta en el héroe
        if (!ownerHero.equippedInstances.Contains(targetInstance)) return;

        ownerHero.stats.maxHealth += maxHealthDelta;
        ownerHero.stats.attack += attackDelta;
        ownerHero.stats.defense += defenseDelta;

        ownerHero.UpdateCombatClass();
    }

    // ============================================================
    //   GUARDAR Y REFRESCAR UI
    // ============================================================
    private void SaveAndRefresh()
    {
        gameManager.SaveEquipmentInventory();
        gameManager.SaveEquipment();
        gameManager.SaveData();

        if (ownerHero != null)
        {
            ownerHero.SaveHeroData_Prefs();
            gameManager.RefreshEquippedSlots(ownerHero);
        }

        if (linkedCatalogSlot != null)
            linkedCatalogSlot.UpdateUI();

        if (linkedEquippedSlot != null)
            linkedEquippedSlot.RefreshSlot();
    }
}