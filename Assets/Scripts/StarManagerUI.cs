using UnityEngine;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using UnityEngine.EventSystems;
using System.Collections;

[System.Serializable]
public class ItemRequirementUI
{
    public Item_SO item;
    public int quantity;

    [Header("UI")]
    public TMP_Text countText;     // "3/5"
    public TMP_Text addText;       // "quedan"
    public TMP_Text removeText;    // "añadidos"

    public Button useButton;
    public Button removeButton;
    public GameObject feedbackObj;

    [HideInInspector] public int usedCount = 0;
}

[System.Serializable]
public class StarRequirementUI
{
    public GameObject starParent;
    public List<ItemRequirementUI> items;
    public int requiredGold;

    public Button upgradeButton;
    public TMP_Text goldText;
}

public class StarManagerUI : MonoBehaviour
{
    [Header("Heroe objetivo")]
    public Hero targetHero;

    [Header("Inventario y recursos")]
    public GameManager gameManager;

    [Header("Requisitos por estrella")]
    public List<StarRequirementUI> starRequirements = new List<StarRequirementUI>();

    [Header("Botón para añadir todos los ítems")]
    public Button addAllButton;

    // HOLD ADD
    private bool isHoldingAdd = false;
    private float holdDelayAdd = 0.4f;
    private float minDelay = 0.05f;
    private float acceleration = 0.05f;
    private int holdIndexAdd = -1;
    private Coroutine holdCoroutineAdd;
    private bool addHoldJustStopped = false;

    // HOLD REMOVE
    private bool isHoldingRemove = false;
    private float holdDelayRemove = 0.4f;
    private int holdIndexRemove = -1;
    private Coroutine holdCoroutineRemove;
    private bool removeHoldJustStopped = false;
    [Header("Vista previa de Stats")]
    public TMP_Text currentHPText;
    public TMP_Text currentATKText;
    public TMP_Text currentDEFText;

    public TMP_Text previewHPText;
    public TMP_Text previewATKText;
    public TMP_Text previewDEFText;

    [Header("Preview de Estrellas")]
    public List<GameObject> previewStars = new List<GameObject>();

    [Header("Número de Estrella actual")]
    // Texto tipo "★ 3" que muestra el número de estrella actual del héroe.
    // Se actualiza solo cada vez que se sube de estrella (TryUpgradeStar -> RefreshUI).
    public TMP_Text starCountText;

    private void Start()
    {
        // Validate that Star Requirements list has enough entries for the hero's current stars
        if (starRequirements.Count <= targetHero.Stars)
        {
            Debug.LogWarning($"[StarManagerUI] Star Requirements list size ({starRequirements.Count}) is insufficient for hero stars ({targetHero.Stars}). Add missing entries in the Inspector.");
        }
        // Initialize UI button listeners and setup requirements
        SetupUI();
        RefreshUI();

        if (addAllButton != null)
            addAllButton.onClick.AddListener(AddAllItemsForCurrentStar);
    }
    public void Update()
    {
        RefreshUI();
    }
    void SetupUI()
    {
        for (int s = 0; s < starRequirements.Count; s++)
        {
            int starIndex = s;
            var req = starRequirements[s];

            foreach (var itemReq in req.items)
            {
                itemReq.usedCount = 0;

                // -------------------------
                // BOTÓN AÑADIR
                // -------------------------
                itemReq.useButton.onClick.AddListener(() => AddItem(starIndex, itemReq));

                EventTrigger addTrigger = itemReq.useButton.gameObject.AddComponent<EventTrigger>();

                var addDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                addDown.callback.AddListener((data) => StartHoldAdd(starIndex, itemReq));
                addTrigger.triggers.Add(addDown);

                var addUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                addUp.callback.AddListener((data) => StopHoldAdd());
                addTrigger.triggers.Add(addUp);

                // -------------------------
                // BOTÓN QUITAR
                // -------------------------
                itemReq.removeButton.onClick.AddListener(() => RemoveItem(starIndex, itemReq));

                EventTrigger removeTrigger = itemReq.removeButton.gameObject.AddComponent<EventTrigger>();

                var remDown = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
                remDown.callback.AddListener((data) => StartHoldRemove(starIndex, itemReq));
                removeTrigger.triggers.Add(remDown);

                var remUp = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
                remUp.callback.AddListener((data) => StopHoldRemove());
                removeTrigger.triggers.Add(remUp);

                if (itemReq.feedbackObj != null)
                    itemReq.feedbackObj.SetActive(false);
            }

            req.upgradeButton.onClick.AddListener(() => TryUpgradeStar(starIndex));
        }
    }

    // -----------------------------
    // REFRESH UI
    // -----------------------------
    private bool warnedInsufficientStarRequirements = false;

    void RefreshUI()
    {
        if (starRequirements.Count == 0) return;

        // -----------------------------------------------------------------
        // 🔥 FIX: antes se hacía "targetHero.Stars = clamp(...)", lo que
        // sobrescribía el valor REAL de Stars cada frame. Si la lista
        // "Star Requirements" tenía menos elementos de los necesarios,
        // esto deshacía la subida de estrella justo después de gastar
        // el oro y los ítems (se gastaba pero nunca subía).
        //
        // Ahora usamos un índice seguro SOLO para indexar la lista,
        // sin tocar el valor real de targetHero.Stars.
        // -----------------------------------------------------------------
        int safeIndex = Mathf.Clamp(targetHero.Stars, 0, starRequirements.Count - 1);

        if (targetHero.Stars != safeIndex && !warnedInsufficientStarRequirements)
        {
            Debug.LogWarning(
                $"[StarManagerUI] '{targetHero.heroName}' tiene Stars = {targetHero.Stars} " +
                $"pero la lista 'Star Requirements' solo tiene {starRequirements.Count} elemento(s). " +
                $"Añade más elementos a la lista (uno por cada nivel de estrella) en el Inspector."
            );
            warnedInsufficientStarRequirements = true;
        }

        // ----------------------------------------------------
        // ACTIVAR SOLO LA ESTRELLA ACTUAL
        // ----------------------------------------------------
        for (int i = 0; i < starRequirements.Count; i++)
        {
            var req = starRequirements[i];
            req.starParent.SetActive(i == safeIndex);

            foreach (var itemReq in req.items)
            {
                int total = gameManager.playerInventory.GetCount(itemReq.item);
                int available = total - itemReq.usedCount;

                // -----------------------------
                // TEXTO PRINCIPAL "usados / requeridos"
                // -----------------------------
                itemReq.countText.text = $"{itemReq.usedCount}/{itemReq.quantity}";

                // -----------------------------
                // TEXTO DE AÑADIR (quedan)
                // -----------------------------
                if (itemReq.addText != null)
                    itemReq.addText.text = available.ToString();

                // -----------------------------
                // TEXTO DE QUITAR (añadidos)
                // -----------------------------
                if (itemReq.removeText != null)
                    itemReq.removeText.text = itemReq.usedCount.ToString();

                // -----------------------------
                // BOTÓN QUITAR SOLO SI HAY ALGO AÑADIDO
                // -----------------------------
                itemReq.removeButton.gameObject.SetActive(itemReq.usedCount > 0);

                // -----------------------------
                // FEEDBACK
                // -----------------------------
                if (itemReq.feedbackObj != null)
                    itemReq.feedbackObj.SetActive(itemReq.usedCount > 0);
            }

            // -----------------------------
            // ORO REQUERIDO
            // -----------------------------
            req.goldText.text = $"Oro: {req.requiredGold}";

            // -----------------------------
            // BOTÓN UPGRADE
            // -----------------------------
            req.upgradeButton.interactable = CanUpgrade(req);
        }

        // ====================================================
        //                VISTA PREVIA DE STATS
        // ====================================================

        // -----------------------------
        // STATS ACTUALES
        // -----------------------------
        currentHPText.text = targetHero.stats.maxHealth.ToString();
        currentATKText.text = targetHero.stats.attack.ToString();
        currentDEFText.text = targetHero.stats.defense.ToString();

        // -----------------------------
        // NÚMERO DE ESTRELLA ACTUAL
        // -----------------------------
        if (starCountText != null)
            starCountText.text = targetHero.Stars.ToString();

        // -----------------------------
        // CALCULAR STATS FUTUROS
        // -----------------------------
        GetFutureStats(out float fHP, out float fATK, out float fDEF);

        previewHPText.text = fHP.ToString();
        previewATKText.text = fATK.ToString();
        previewDEFText.text = fDEF.ToString();

        previewHPText.color = Color.cyan;
        previewATKText.color = Color.cyan;
        previewDEFText.color = Color.cyan;

        // ====================================================
        //                PREVIEW DE ESTRELLAS
        // ====================================================

        bool willUpgrade = true;
        var currentReq = starRequirements[safeIndex];

        foreach (var itemReq in currentReq.items)
        {
            if (itemReq.usedCount < itemReq.quantity)
            {
                willUpgrade = false;
                break;
            }
        }

        UpdatePreviewStars(willUpgrade);
    }

    void GetFutureStats(out float fHP, out float fATK, out float fDEF)
    {
        // Copias temporales
        float hp = targetHero.stats.maxHealth;
        float atk = targetHero.stats.attack;
        float def = targetHero.stats.defense;

        // Si se completa la estrella actual → sube de estrella
        bool willUpgrade = true;

        int safeIndex = Mathf.Clamp(targetHero.Stars, 0, starRequirements.Count - 1);
        var req = starRequirements[safeIndex];
        foreach (var itemReq in req.items)
        {
            if (itemReq.usedCount < itemReq.quantity)
            {
                willUpgrade = false;
                break;
            }
        }

        int futureStars = targetHero.Stars + (willUpgrade ? 1 : 0);

        // Recalcular stats según la fórmula de tu Hero.cs
        int levelsToAdd = willUpgrade ? 5 : 0;

        for (int i = 0; i < levelsToAdd; i++)
        {
            hp = Mathf.Round(hp * 1.15f);
            atk = Mathf.Round(atk * 1.12f);
            def = Mathf.Round(def * 1.08f);
        }

        fHP = hp;
        fATK = atk;
        fDEF = def;
    }
    void UpdatePreviewStars(bool willUpgrade)
    {
        int futureStars = targetHero.Stars + (willUpgrade ? 1 : 0);

        // 🔥 FIX: "futureStars" es una CANTIDAD de estrellas conseguidas, no un
        // índice. Antes se clampaba a (Count - 1) y se comparaba con "i <=
        // futureStars", lo que encendía siempre una estrella de más: con un
        // héroe que aún tiene 0 estrellas ya se veía 1 estrella iluminada
        // (y con 2 estrellas se veían 3, etc.).
        //
        // Ahora se clampa como CANTIDAD (0..Count, no Count-1) y se usa
        // "i < starsToShow", de modo que con 0 estrellas el preview arranca
        // completamente apagado, y con el máximo se muestran todas.
        int starsToShow = Mathf.Clamp(futureStars, 0, previewStars.Count);

        for (int i = 0; i < previewStars.Count; i++)
            previewStars[i].SetActive(i < starsToShow);
    }

    // -----------------------------
    // RESET
    // -----------------------------
    void ResetUsedCounts(int starIndex)
    {
        var req = starRequirements[starIndex];
        foreach (var itemReq in req.items)
        {
            itemReq.usedCount = 0;
            if (itemReq.feedbackObj != null)
                itemReq.feedbackObj.SetActive(false);
        }
    }

    // -----------------------------
    // AÑADIR ITEM
    // -----------------------------
    void AddItem(int starIndex, ItemRequirementUI itemReq)
    {
        if (removeHoldJustStopped) { removeHoldJustStopped = false; return; }
        if (addHoldJustStopped) { addHoldJustStopped = false; return; }

        if (starIndex != targetHero.Stars) return;

        int total = gameManager.playerInventory.GetCount(itemReq.item);
        int available = total - itemReq.usedCount;

        if (available <= 0) return;
        if (itemReq.usedCount >= itemReq.quantity) return;

        itemReq.usedCount++;

        if (itemReq.feedbackObj != null)
            itemReq.feedbackObj.SetActive(true);

        RefreshUI();
    }

    // -----------------------------
    // QUITAR ITEM
    // -----------------------------
    void RemoveItem(int starIndex, ItemRequirementUI itemReq)
    {
        if (addHoldJustStopped) { addHoldJustStopped = false; return; }
        if (removeHoldJustStopped) { removeHoldJustStopped = false; return; }

        if (itemReq.usedCount <= 0) return;

        itemReq.usedCount--;

        if (itemReq.usedCount <= 0 && itemReq.feedbackObj != null)
            itemReq.feedbackObj.SetActive(false);

        RefreshUI();
    }

    // -----------------------------
    // HOLD ADD
    // -----------------------------
    void StartHoldAdd(int starIndex, ItemRequirementUI itemReq)
    {
        int total = gameManager.playerInventory.GetCount(itemReq.item);
        if (total <= itemReq.usedCount) return;

        isHoldingAdd = true;
        holdDelayAdd = 0.4f;

        holdIndexAdd = starRequirements[starIndex].items.IndexOf(itemReq);

        holdCoroutineAdd = StartCoroutine(HoldRoutineAdd(starIndex));
    }

    void StopHoldAdd()
    {
        isHoldingAdd = false;
        holdIndexAdd = -1;
        addHoldJustStopped = true;

        if (holdCoroutineAdd != null)
        {
            StopCoroutine(holdCoroutineAdd);
            holdCoroutineAdd = null;
        }
    }

    IEnumerator HoldRoutineAdd(int starIndex)
    {
        var req = starRequirements[starIndex];

        while (isHoldingAdd)
        {
            var itemReq = req.items[holdIndexAdd];

            int total = gameManager.playerInventory.GetCount(itemReq.item);
            int available = total - itemReq.usedCount;

            if (available > 0 && itemReq.usedCount < itemReq.quantity)
            {
                itemReq.usedCount++;
                if (itemReq.feedbackObj != null)
                    itemReq.feedbackObj.SetActive(true);

                RefreshUI();
            }
            else
            {
                StopHoldAdd();
                yield break;
            }

            holdDelayAdd = Mathf.Max(minDelay, holdDelayAdd - acceleration);
            yield return new WaitForSeconds(holdDelayAdd);
        }
    }

    // -----------------------------
    // HOLD REMOVE
    // -----------------------------
    void StartHoldRemove(int starIndex, ItemRequirementUI itemReq)
    {
        if (itemReq.usedCount <= 0) return;

        isHoldingRemove = true;
        holdDelayRemove = 0.4f;

        holdIndexRemove = starRequirements[starIndex].items.IndexOf(itemReq);

        holdCoroutineRemove = StartCoroutine(HoldRoutineRemove(starIndex));
    }

    void StopHoldRemove()
    {
        isHoldingRemove = false;
        holdIndexRemove = -1;
        removeHoldJustStopped = true;

        if (holdCoroutineRemove != null)
        {
            StopCoroutine(holdCoroutineRemove);
            holdCoroutineRemove = null;
        }
    }

    IEnumerator HoldRoutineRemove(int starIndex)
    {
        var req = starRequirements[starIndex];

        while (isHoldingRemove)
        {
            var itemReq = req.items[holdIndexRemove];

            if (itemReq.usedCount > 0)
            {
                itemReq.usedCount--;

                if (itemReq.usedCount <= 0 && itemReq.feedbackObj != null)
                    itemReq.feedbackObj.SetActive(false);

                RefreshUI();
            }
            else
            {
                StopHoldRemove();
                yield break;
            }

            holdDelayRemove = Mathf.Max(minDelay, holdDelayRemove - acceleration);
            yield return new WaitForSeconds(holdDelayRemove);
        }
    }

    // -----------------------------
    // AUTOFEED
    // -----------------------------
    void AddAllItemsForCurrentStar()
    {
        int s = targetHero.Stars;
        if (s < 0 || s >= starRequirements.Count) return;

        var req = starRequirements[s];

        foreach (var itemReq in req.items)
        {
            int total = gameManager.playerInventory.GetCount(itemReq.item);
            int available = total - itemReq.usedCount;

            int needed = itemReq.quantity - itemReq.usedCount;
            if (needed <= 0 || available <= 0) continue;

            int toUse = Mathf.Min(needed, available);
            itemReq.usedCount += toUse;

            if (itemReq.feedbackObj != null)
                itemReq.feedbackObj.SetActive(itemReq.usedCount > 0);
        }

        RefreshUI();
    }

    // -----------------------------
    // VALIDACIÓN
    // -----------------------------
    bool CanUpgrade(StarRequirementUI req)
    {
        if (gameManager.Gold < req.requiredGold)
            return false;

        foreach (var itemReq in req.items)
        {
            if (itemReq.usedCount < itemReq.quantity)
                return false;
        }

        return true;
    }

    // -----------------------------
    // UPGRADE
    // -----------------------------
    void TryUpgradeStar(int starIndex)
    {
        if (starIndex != targetHero.Stars) return;

        // 🔥 FIX: el guard anterior comprobaba "starIndex + 1 >= Count", es
        // decir, exigía que existiera la entrada de la SIGUIENTE estrella
        // para poder subir la ACTUAL. Eso bloqueaba cualquier subida al
        // último nivel configurado (y con una sola entrada en la lista,
        // bloqueaba TODAS las subidas, incluida la de 0 → 1 estrellas).
        //
        // Lo único que hace falta para subir es que exista la entrada de
        // la estrella ACTUAL (starIndex), que ya está garantizada porque
        // el botón solo se crea para índices válidos de la lista.
        if (starIndex >= starRequirements.Count)
        {
            Debug.LogWarning(
                $"[StarManagerUI] No hay una entrada configurada para la estrella {starIndex} " +
                $"en 'Star Requirements'. Añade un elemento en el Inspector para permitir esta subida."
            );
            return;
        }

        var req = starRequirements[starIndex];
        if (!CanUpgrade(req))
        {
            Debug.Log("No tienes suficientes recursos.");
            return;
        }

        gameManager.Gold -= req.requiredGold;

        foreach (var itemReq in req.items)
        {
            gameManager.playerInventory.RemoveItem(itemReq.item, itemReq.usedCount);
        }

        ResetUsedCounts(starIndex);

        gameManager.SaveInventory();
        gameManager.SaveData();

        targetHero.Stars++;

        // Aviso informativo (no bloqueante): si aún no hay una entrada en el
        // Inspector para el nuevo nivel de estrella, RefreshUI mostrará la
        // última entrada disponible hasta que se añada la que falta.
        if (targetHero.Stars >= starRequirements.Count)
        {
            Debug.LogWarning(
                $"[StarManagerUI] '{targetHero.heroName}' ha subido a {targetHero.Stars} estrellas, " +
                $"pero no hay una entrada configurada para ese nivel en 'Star Requirements'. " +
                $"Añade un elemento en el Inspector para poder mostrar/gestionar el siguiente nivel."
            );
        }

        GameManager.Instance.SaveHeroStars(targetHero);

        RefreshUI();
    }
}