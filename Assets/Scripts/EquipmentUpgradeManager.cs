using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.EventSystems;

// ================================================================
//   MATERIAL DE NIVEL (equivalente a las "pociones" de XPManager,
//   pero para equipamento). Se puede añadir cualquier cantidad
//   disponible en el inventario; cada unidad aporta "puntos".
// ================================================================
[System.Serializable]
public class EquipLevelMaterial
{
    public Item_SO item;
    public int pointValue = 100; // puntos de mejora que aporta 1 unidad de este item

    [Header("UI")]
    public TMP_Text availableText;   // cuántos quedan disponibles en inventario
    public TMP_Text selectedText;    // cuántos ha seleccionado el jugador
    public Button addButton;
    public Button removeButton;
    public GameObject feedbackObj;       // se activa con 1 o más seleccionados
    public GameObject multipleSelectedObj; // se activa con selectedCount >= 1
    public GameObject noSelectionObj;      // 🔥 NUEVO: se activa cuando selectedCount == 0 (pareja de multipleSelectedObj)

    [HideInInspector] public int selectedCount = 0;
}

// ================================================================
//   TIER DE DESPERTAR (equivalente a StarRequirementUI, pero para
//   equipamento). Reutiliza ItemRequirementUI, ya definido en
//   StarManagerUI.cs, para no duplicar la estructura de "cantidad
//   exacta requerida".
// ================================================================
[System.Serializable]
public class EquipAwakenTier
{
    public GameObject tierParent;
    public List<ItemRequirementUI> items;
    public int requiredGold;

    public Button upgradeButton;
    public TMP_Text goldText;
}

public class EquipmentUpgradeManager : MonoBehaviour
{
    // 🔥 NUEVO: singleton, igual que GameManager.Instance.
    // Los slots que vienen de un PREFAB instanciado dinámicamente
    // (ej: "EquipmentSlot_Mejorar(Clone)") no pueden guardar una
    // referencia directa a este manager en el inspector, porque el
    // manager vive en la escena y un asset de prefab no puede apuntar
    // a un objeto de escena. Por eso EquipmentSlot usa este Instance
    // como respaldo cuando su campo "upgradeManager" está vacío.
    public static EquipmentUpgradeManager Instance;

    [Header("Referencias")]
    public GameManager gameManager;

    // Instancia seleccionada actualmente (se asigna desde un
    // EquipmentSlot con forUpgrade = true, vía SelectEquipment()).
    [Header("Equipo objetivo (se rellena solo al seleccionar en el slot)")]
    public EquipmentInstance targetEquipment;
    public Hero ownerHero; // héroe que lo tiene equipado, si aplica (puede ser null)

    [Header("UI - Info del equipo seleccionado")]
    public Image iconImage;
    public TMP_Text nameText;
    public TMP_Text levelText;
    public TMP_Text awakeningText;
    public GameObject noSelectionPanel; // ahora se activa JUNTO con selectionPanel (misma condición: hasTarget)
    public GameObject selectionPanel;   // contenido de mejora, activo cuando hay equipo seleccionado

    // ============================================================
    //   NIVEL (estilo XPManager)
    // ============================================================
    [Header("Nivel - Materiales")]
    public EquipLevelMaterial[] levelMaterials;
    public Button levelUpgradeButton;
    public TMP_Text totalPointsText;
    public TMP_Text goldCostLevelText;
    public Slider levelProgressSlider;

    [Header("Nivel - Configuración")]
    public int maxEquipmentLevel = 20;
    public int basePointsPerLevel = 100;
    public int pointsIncrementPerLevel = 50;

    private int totalPoints = 0;
    private int goldCostLevel = 0;

    // Hold para añadir/quitar materiales de nivel
    private bool isHoldingAddLevel = false;
    private bool isHoldingRemoveLevel = false;
    private bool addLevelHoldJustStopped = false;
    private bool removeLevelHoldJustStopped = false;
    private int holdLevelIndex = -1;
    private float holdDelay = 0.4f;
    private const float minDelay = 0.05f;
    private const float acceleration = 0.05f;
    private Coroutine holdLevelCoroutine;

    // ============================================================
    //   DESPERTAR (estilo StarManagerUI)
    // ============================================================
    [Header("Despertar - Tiers")]
    public List<EquipAwakenTier> awakenTiers = new List<EquipAwakenTier>();
    public int maxAwakening = 5;

    [Header("Vista previa de bonus de instancia")]
    // Bonus que aporta ApplyInstanceBonuses() en EquipmentSlot: HP += level*2, DEF += awakening*3
    public TMP_Text currentBonusText;
    public TMP_Text previewBonusText;

    private bool warnedInsufficientAwakenTiers = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        if (gameManager == null)
            gameManager = GameManager.Instance;

        SetupLevelUI();
        SetupAwakenUI();
        RefreshUI();
    }

    private void Update()
    {
        // Igual que StarManagerUI: refresco continuo para reflejar cambios de inventario/oro
        RefreshUI();
    }

    // ============================================================
    //   SELECCIÓN DE EQUIPO (llamado desde EquipmentSlot.SelectForUpgrade)
    // ============================================================
    public void SelectEquipment(EquipmentInstance inst, Hero owner)
    {
        targetEquipment = inst;
        ownerHero = owner;

        ResetLevelSelection();
        RefreshUI();
    }

    public void ClearSelection()
    {
        targetEquipment = null;
        ownerHero = null;
        ResetLevelSelection();
        RefreshUI();
    }

    // ============================================================
    //   SETUP BOTONES - NIVEL
    // ============================================================
    void SetupLevelUI()
    {
        for (int i = 0; i < levelMaterials.Length; i++)
        {
            int index = i;
            var mat = levelMaterials[i];
            if (mat.addButton == null || mat.removeButton == null) continue;

            mat.addButton.onClick.AddListener(() => AddLevelMaterial(index));
            mat.removeButton.onClick.AddListener(() => RemoveLevelMaterial(index));

            EventTrigger addTrigger = mat.addButton.gameObject.AddComponent<EventTrigger>();
            var addDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            addDown.callback.AddListener((data) => StartHoldAddLevel(index));
            addTrigger.triggers.Add(addDown);
            var addUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            addUp.callback.AddListener((data) => StopHoldAddLevel());
            addTrigger.triggers.Add(addUp);

            EventTrigger removeTrigger = mat.removeButton.gameObject.AddComponent<EventTrigger>();
            var remDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            remDown.callback.AddListener((data) => StartHoldRemoveLevel(index));
            removeTrigger.triggers.Add(remDown);
            var remUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            remUp.callback.AddListener((data) => StopHoldRemoveLevel());
            removeTrigger.triggers.Add(remUp);

            if (mat.feedbackObj != null)
                mat.feedbackObj.SetActive(false);
        }

        if (levelUpgradeButton != null)
            levelUpgradeButton.onClick.AddListener(ApplyLevelUpgrade);
    }

    // ============================================================
    //   SETUP BOTONES - DESPERTAR
    // ============================================================
    void SetupAwakenUI()
    {
        for (int t = 0; t < awakenTiers.Count; t++)
        {
            int tierIndex = t;
            var tier = awakenTiers[t];

            foreach (var itemReq in tier.items)
            {
                itemReq.usedCount = 0;

                itemReq.useButton.onClick.AddListener(() => AddAwakenItem(tierIndex, itemReq));
                itemReq.removeButton.onClick.AddListener(() => RemoveAwakenItem(tierIndex, itemReq));

                if (itemReq.feedbackObj != null)
                    itemReq.feedbackObj.SetActive(false);
            }

            tier.upgradeButton.onClick.AddListener(() => TryAwaken(tierIndex));
        }
    }

    // ============================================================
    //   REFRESH GENERAL
    // ============================================================
    void RefreshUI()
    {
        bool hasTarget = targetEquipment != null;

        if (noSelectionPanel != null) noSelectionPanel.SetActive(hasTarget);
        if (selectionPanel != null) selectionPanel.SetActive(hasTarget);

        if (!hasTarget)
            return;

        // Info básica
        if (iconImage != null && targetEquipment.baseData != null)
            iconImage.sprite = targetEquipment.baseData.icon;
        if (nameText != null && targetEquipment.baseData != null)
            nameText.text = targetEquipment.baseData.itemName;
        if (levelText != null)
            levelText.text = $"Nv {targetEquipment.level}";
        if (awakeningText != null)
            awakeningText.text = $"Despertar {targetEquipment.awakening}/{maxAwakening}";

        RefreshLevelUI();
        RefreshAwakenUI();
        RefreshBonusPreview();
    }

    // ============================================================
    //   NIVEL - LÓGICA (estilo XPManager)
    // ============================================================
    void RefreshLevelUI()
    {
        bool atMaxLevel = targetEquipment.level >= maxEquipmentLevel;

        for (int i = 0; i < levelMaterials.Length; i++)
        {
            var mat = levelMaterials[i];
            if (mat.item == null) continue;

            int total = gameManager.playerInventory.GetCount(mat.item);
            int available = total - mat.selectedCount;

            if (mat.availableText != null) mat.availableText.text = available.ToString();
            if (mat.selectedText != null) mat.selectedText.text = mat.selectedCount.ToString();
            if (mat.removeButton != null) mat.removeButton.gameObject.SetActive(mat.selectedCount > 0);
            if (mat.feedbackObj != null) mat.feedbackObj.SetActive(mat.selectedCount > 0);
            if (mat.multipleSelectedObj != null) mat.multipleSelectedObj.SetActive(mat.selectedCount >= 1);
            if (mat.noSelectionObj != null) mat.noSelectionObj.SetActive(mat.selectedCount == 0);
            if (mat.addButton != null) mat.addButton.interactable = !atMaxLevel && available > 0;
        }

        if (totalPointsText != null)
            totalPointsText.text = totalPoints.ToString();
        if (goldCostLevelText != null)
            goldCostLevelText.text = $"Oro: {goldCostLevel}";

        if (levelProgressSlider != null)
        {
            int needed = PointsNeededForLevel(targetEquipment.level);
            levelProgressSlider.maxValue = needed;
            levelProgressSlider.value = Mathf.Min(totalPoints, needed);
        }

        if (levelUpgradeButton != null)
            levelUpgradeButton.interactable = !atMaxLevel && totalPoints > 0 && gameManager.Gold >= goldCostLevel;
    }

    int PointsNeededForLevel(int level)
    {
        return basePointsPerLevel + (level - 1) * pointsIncrementPerLevel;
    }

    int GetRemainingPointsToMax()
    {
        if (targetEquipment.level >= maxEquipmentLevel) return 0;

        int needed = 0;
        for (int lvl = targetEquipment.level; lvl < maxEquipmentLevel; lvl++)
            needed += PointsNeededForLevel(lvl);

        needed -= totalPoints;
        return Mathf.Max(0, needed);
    }

    void AddLevelMaterial(int index)
    {
        if (removeLevelHoldJustStopped) { removeLevelHoldJustStopped = false; return; }
        if (addLevelHoldJustStopped) { addLevelHoldJustStopped = false; return; }
        if (targetEquipment == null) return;

        var mat = levelMaterials[index];
        int total = gameManager.playerInventory.GetCount(mat.item);
        if (mat.selectedCount >= total) return;

        int remaining = GetRemainingPointsToMax();
        if (remaining <= 0) return;

        mat.selectedCount++;
        totalPoints += mat.pointValue;
        goldCostLevel += Mathf.RoundToInt((mat.pointValue / 2f) * (1f + targetEquipment.level * 0.1f));

        RefreshUI();
    }

    void RemoveLevelMaterial(int index)
    {
        if (removeLevelHoldJustStopped) { removeLevelHoldJustStopped = false; return; }
        if (targetEquipment == null) return;

        var mat = levelMaterials[index];
        if (mat.selectedCount <= 0) return;

        RemoveLevelMaterialInternal(index);
        RefreshUI();
    }

    void RemoveLevelMaterialInternal(int index)
    {
        var mat = levelMaterials[index];
        mat.selectedCount--;
        totalPoints -= mat.pointValue;
        if (totalPoints < 0) totalPoints = 0;

        goldCostLevel -= Mathf.RoundToInt((mat.pointValue / 2f) * (1f + targetEquipment.level * 0.1f));
        if (goldCostLevel < 0) goldCostLevel = 0;
    }

    void ResetLevelSelection()
    {
        totalPoints = 0;
        goldCostLevel = 0;
        if (levelMaterials == null) return;

        foreach (var mat in levelMaterials)
        {
            mat.selectedCount = 0;
            if (mat.feedbackObj != null) mat.feedbackObj.SetActive(false);
            if (mat.multipleSelectedObj != null) mat.multipleSelectedObj.SetActive(false);
            if (mat.noSelectionObj != null) mat.noSelectionObj.SetActive(true);
        }
    }

    // Botón "Mejorar nivel": consume los materiales/oro seleccionados y aplica
    // tantos niveles como los puntos acumulados permitan (igual que GainXP con potions).
    void ApplyLevelUpgrade()
    {
        if (targetEquipment == null)
        {
            Debug.LogWarning("No hay equipo seleccionado para mejorar.");
            return;
        }

        if (targetEquipment.level >= maxEquipmentLevel)
        {
            Debug.LogWarning($"{targetEquipment.baseData.itemName} ya está en el nivel máximo ({maxEquipmentLevel}).");
            ResetLevelSelection();
            RefreshUI();
            return;
        }

        if (gameManager.Gold < goldCostLevel)
        {
            Debug.LogWarning("No hay suficiente oro.");
            return;
        }

        int oldLevel = targetEquipment.level;

        // Consumir puntos en niveles completos; el sobrante de esta sesión se descarta,
        // igual que ocurre con el XP de héroe cuando se supera el nivel máximo.
        int pointsLeft = totalPoints;
        int newLevel = targetEquipment.level;
        while (newLevel < maxEquipmentLevel && pointsLeft >= PointsNeededForLevel(newLevel))
        {
            pointsLeft -= PointsNeededForLevel(newLevel);
            newLevel++;
        }

        if (newLevel == oldLevel)
        {
            Debug.Log("No hay suficientes puntos acumulados para subir de nivel todavía.");
            return;
        }

        gameManager.Gold -= goldCostLevel;

        // Descontar del inventario los materiales realmente seleccionados
        foreach (var mat in levelMaterials)
        {
            if (mat.selectedCount > 0)
                gameManager.playerInventory.RemoveItem(mat.item, mat.selectedCount);
        }

        targetEquipment.level = newLevel;

        RefreshEquippedBonuses(oldLevel, targetEquipment.awakening, newLevel, targetEquipment.awakening);

        gameManager.SaveInventory();
        gameManager.SaveEquipmentInventory();

        ResetLevelSelection();
        RefreshUI();
    }

    // ============================================================
    //   HOLD - AÑADIR/QUITAR MATERIALES DE NIVEL
    // ============================================================
    void StartHoldAddLevel(int index)
    {
        if (targetEquipment == null) return;
        var mat = levelMaterials[index];
        int total = gameManager.playerInventory.GetCount(mat.item);
        if (mat.selectedCount >= total) return;

        isHoldingAddLevel = true;
        holdLevelIndex = index;
        holdDelay = 0.4f;
        holdLevelCoroutine = StartCoroutine(HoldAddLevelRoutine());
    }

    void StopHoldAddLevel()
    {
        isHoldingAddLevel = false;
        holdLevelIndex = -1;
        addLevelHoldJustStopped = true;

        if (holdLevelCoroutine != null)
        {
            StopCoroutine(holdLevelCoroutine);
            holdLevelCoroutine = null;
        }
    }

    IEnumerator HoldAddLevelRoutine()
    {
        while (isHoldingAddLevel)
        {
            var mat = levelMaterials[holdLevelIndex];
            int total = gameManager.playerInventory.GetCount(mat.item);
            int remaining = GetRemainingPointsToMax();

            if (mat.selectedCount < total && remaining > 0)
            {
                mat.selectedCount++;
                totalPoints += mat.pointValue;
                goldCostLevel += Mathf.RoundToInt((mat.pointValue / 2f) * (1f + targetEquipment.level * 0.1f));
                RefreshUI();
            }
            else
            {
                StopHoldAddLevel();
                yield break;
            }

            holdDelay = Mathf.Max(minDelay, holdDelay - acceleration);
            yield return new WaitForSeconds(holdDelay);
        }
    }

    void StartHoldRemoveLevel(int index)
    {
        if (targetEquipment == null) return;
        var mat = levelMaterials[index];
        if (mat.selectedCount <= 0) return;

        isHoldingRemoveLevel = true;
        holdLevelIndex = index;
        holdDelay = 0.4f;
        holdLevelCoroutine = StartCoroutine(HoldRemoveLevelRoutine());
    }

    void StopHoldRemoveLevel()
    {
        isHoldingRemoveLevel = false;
        holdLevelIndex = -1;
        StartCoroutine(BlockAddLevelOneFrame());

        if (holdLevelCoroutine != null)
        {
            StopCoroutine(holdLevelCoroutine);
            holdLevelCoroutine = null;
        }
    }

    IEnumerator BlockAddLevelOneFrame()
    {
        removeLevelHoldJustStopped = true;
        yield return null;
        removeLevelHoldJustStopped = false;
    }

    IEnumerator HoldRemoveLevelRoutine()
    {
        while (isHoldingRemoveLevel)
        {
            var mat = levelMaterials[holdLevelIndex];
            if (mat.selectedCount > 0)
            {
                RemoveLevelMaterialInternal(holdLevelIndex);
                RefreshUI();
            }
            else
            {
                StopHoldRemoveLevel();
                yield break;
            }

            holdDelay = Mathf.Max(minDelay, holdDelay - acceleration);
            yield return new WaitForSeconds(holdDelay);
        }
    }

    // ============================================================
    //   DESPERTAR - LÓGICA (estilo StarManagerUI)
    // ============================================================
    void RefreshAwakenUI()
    {
        if (awakenTiers.Count == 0) return;

        int safeIndex = Mathf.Clamp(targetEquipment.awakening, 0, awakenTiers.Count - 1);

        if (targetEquipment.awakening != safeIndex && targetEquipment.awakening < maxAwakening && !warnedInsufficientAwakenTiers)
        {
            Debug.LogWarning(
                $"[EquipmentUpgradeManager] El equipo tiene despertar = {targetEquipment.awakening} " +
                $"pero la lista 'Awaken Tiers' solo tiene {awakenTiers.Count} elemento(s). " +
                $"Añade más elementos (uno por nivel de despertar) en el Inspector."
            );
            warnedInsufficientAwakenTiers = true;
        }

        for (int i = 0; i < awakenTiers.Count; i++)
        {
            var tier = awakenTiers[i];
            bool isCurrentTier = (i == safeIndex) && (targetEquipment.awakening < maxAwakening);
            if (tier.tierParent != null)
                tier.tierParent.SetActive(isCurrentTier);

            foreach (var itemReq in tier.items)
            {
                int total = gameManager.playerInventory.GetCount(itemReq.item);
                int available = total - itemReq.usedCount;

                if (itemReq.countText != null) itemReq.countText.text = $"{itemReq.usedCount}/{itemReq.quantity}";
                if (itemReq.addText != null) itemReq.addText.text = available.ToString();
                if (itemReq.removeText != null) itemReq.removeText.text = itemReq.usedCount.ToString();
                if (itemReq.removeButton != null) itemReq.removeButton.gameObject.SetActive(itemReq.usedCount > 0);
                if (itemReq.feedbackObj != null) itemReq.feedbackObj.SetActive(itemReq.usedCount > 0);
            }

            if (tier.goldText != null)
                tier.goldText.text = $"Oro: {tier.requiredGold}";

            if (tier.upgradeButton != null)
                tier.upgradeButton.interactable = CanAwaken(tier);
        }
    }

    void AddAwakenItem(int tierIndex, ItemRequirementUI itemReq)
    {
        if (targetEquipment == null) return;

        int total = gameManager.playerInventory.GetCount(itemReq.item);
        int available = total - itemReq.usedCount;

        if (available <= 0 || itemReq.usedCount >= itemReq.quantity) return;

        itemReq.usedCount++;
        if (itemReq.feedbackObj != null) itemReq.feedbackObj.SetActive(true);

        RefreshUI();
    }

    void RemoveAwakenItem(int tierIndex, ItemRequirementUI itemReq)
    {
        if (itemReq.usedCount <= 0) return;

        itemReq.usedCount--;
        if (itemReq.usedCount <= 0 && itemReq.feedbackObj != null)
            itemReq.feedbackObj.SetActive(false);

        RefreshUI();
    }

    bool CanAwaken(EquipAwakenTier tier)
    {
        if (targetEquipment == null) return false;
        if (targetEquipment.awakening >= maxAwakening) return false;
        if (gameManager.Gold < tier.requiredGold) return false;

        foreach (var itemReq in tier.items)
        {
            if (itemReq.usedCount < itemReq.quantity)
                return false;
        }

        return true;
    }

    void TryAwaken(int tierIndex)
    {
        if (targetEquipment == null) return;
        if (tierIndex != targetEquipment.awakening) return;
        if (tierIndex >= awakenTiers.Count)
        {
            Debug.LogWarning($"[EquipmentUpgradeManager] No hay una entrada configurada para el despertar {tierIndex} en 'Awaken Tiers'.");
            return;
        }

        var tier = awakenTiers[tierIndex];
        if (!CanAwaken(tier))
        {
            Debug.Log("No tienes suficientes recursos para despertar este equipo.");
            return;
        }

        int oldAwakening = targetEquipment.awakening;

        gameManager.Gold -= tier.requiredGold;

        foreach (var itemReq in tier.items)
        {
            gameManager.playerInventory.RemoveItem(itemReq.item, itemReq.usedCount);
            itemReq.usedCount = 0;
            if (itemReq.feedbackObj != null) itemReq.feedbackObj.SetActive(false);
        }

        targetEquipment.awakening++;

        RefreshEquippedBonuses(targetEquipment.level, oldAwakening, targetEquipment.level, targetEquipment.awakening);

        gameManager.SaveInventory();
        gameManager.SaveEquipmentInventory();

        RefreshUI();
    }

    // ============================================================
    //   PREVIEW DE BONUS DE INSTANCIA
    //   (Igual que ApplyInstanceBonuses en EquipmentSlot:
    //    maxHealth += level*2, defense += awakening*3)
    // ============================================================
    void RefreshBonusPreview()
    {
        if (currentBonusText != null)
        {
            float curHP = targetEquipment.level * 2f;
            float curDEF = targetEquipment.awakening * 3f;
            currentBonusText.text = $"HP +{curHP}  DEF +{curDEF}";
        }

        if (previewBonusText != null)
        {
            int futureLevel = SimulateFutureLevel();
            float nextHP = futureLevel * 2f;
            float nextDEF = targetEquipment.awakening * 3f;
            previewBonusText.text = $"HP +{nextHP}  DEF +{nextDEF}";
        }
    }

    int SimulateFutureLevel()
    {
        int lvl = targetEquipment.level;
        int pointsLeft = totalPoints;

        while (lvl < maxEquipmentLevel && pointsLeft >= PointsNeededForLevel(lvl))
        {
            pointsLeft -= PointsNeededForLevel(lvl);
            lvl++;
        }

        return lvl;
    }

    // ============================================================
    //   REFRESCAR STATS DEL HÉROE SI EL EQUIPO YA ESTÁ PUESTO
    //   Al subir de nivel o despertar una pieza que YA está equipada,
    //   sus bonus de instancia (ApplyInstanceBonuses en EquipmentSlot)
    //   quedaron fijados en el momento de equipar. Aquí aplicamos solo
    //   la DIFERENCIA (delta) para no tener que desequipar/reequipar.
    // ============================================================
    void RefreshEquippedBonuses(int oldLevel, int oldAwakening, int newLevel, int newAwakening)
    {
        Hero hero = ownerHero;

        // Si no se pasó héroe propietario, buscamos si alguien del roster
        // tiene esta instancia equipada (por si se seleccionó desde el catálogo).
        if (hero == null && gameManager != null && gameManager.roster != null)
        {
            hero = gameManager.roster.FirstOrDefault(h =>
                h.equippedInstances != null && h.equippedInstances.Contains(targetEquipment));
        }

        if (hero == null) return; // no está equipado por nadie: nada que refrescar

        float deltaHP = (newLevel - oldLevel) * 2f;
        float deltaDEF = (newAwakening - oldAwakening) * 3f;

        hero.stats.maxHealth += deltaHP;
        hero.stats.defense += deltaDEF;

        hero.UpdateCombatClass();
        hero.SaveHeroData_Prefs();
        gameManager.RefreshEquippedSlots(hero);
    }
}