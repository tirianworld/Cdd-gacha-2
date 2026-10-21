using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class XPManager : MonoBehaviour
{
    [Header("Botones de pociones")]
    public Button[] potionButtons; // índice 0 = 1 estrella, índice 4 = 5 estrellas
    public TMP_Text[] potionCountsText;
    public TMP_Text[] potionSelectedText;
    [Header("Botones para quitar pociones")]
    public Button[] removePotionButtons; // 5 botones, uno por cada tipo
    private bool holdJustStopped = false;
    private bool addHoldJustStopped = false;
    private bool removeHoldJustStopped = false;

    [Header("Botón de mejora")]
    public Button upgradeButton;
    public TMP_Text totalXPText;
    public TMP_Text goldCostText;

    [Header("Datos")]
    public int[] potionCounts = new int[5]; // inventario actual
    private int[] selectedPotions = new int[5]; // seleccionadas para usar
    private int[] potionXPValues = new int[5] { 100, 300, 800, 1500, 3000 }; // valores por estrella
    private int totalXP = 0;
    private int goldCost = 0;
    private float holdGraceTimer = 0f;
    private const float holdGraceDuration = 2.5f;


    [Header("GameObjects de feedback por rango")]
    public GameObject[] potionFeedbackObjects;
    // índice 0 = feedback para poción 1 estrella, índice 4 = feedback para poción 5 estrellas

    private const string PotionKeyPrefix = "PotionCount_";
    private const string HeroLevelKeyPrefix = "HeroLevel_";
    [Header("Héroe objetivo")]
    public Hero targetHero;
    private bool isHolding = false;
    private float holdDelay = 0.4f;
    private float minDelay = 0.05f;
    private float acceleration = 0.05f;
    private int holdIndex = -1;
    private Coroutine holdCoroutine;
    [Header("Vista previa de Stats")]
    public TMP_Text currentHPText;
    public TMP_Text currentATKText;
    public TMP_Text currentDEFText;

    public TMP_Text previewHPText;
    public TMP_Text previewATKText;
    public TMP_Text previewDEFText;

    [Header("XP UI")]
    public Slider xpSlider;
    public TMP_Text levelText;

    void Start()
    {
        LoadPotionInventory();
        LoadHeroLevel();
        RefreshUI();

        // -------------------------
        // BOTONES DE AÑADIR POCIONES
        // -------------------------
        for (int i = 0; i < potionButtons.Length; i++)
        {
            int index = i;

            // Click normal
            potionButtons[i].onClick.AddListener(() => AddPotion(index));

            // Mantener pulsado
            EventTrigger trigger = potionButtons[i].gameObject.AddComponent<EventTrigger>();

            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener((data) => StartHold(index));
            trigger.triggers.Add(down);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener((data) => StopHold());
            trigger.triggers.Add(up);
        }

        // -------------------------
        // BOTONES DE QUITAR POCIONES
        // -------------------------
        for (int i = 0; i < removePotionButtons.Length; i++)
        {
            int index = i;

            // Click normal
           

            // Mantener pulsado
            EventTrigger trigger = removePotionButtons[i].gameObject.AddComponent<EventTrigger>();
            var click = new EventTrigger.Entry { eventID = EventTriggerType.PointerClick };
            click.callback.AddListener((data) => RemovePotion(index));
            trigger.triggers.Add(click);

            var down = new EventTrigger.Entry { eventID = EventTriggerType.PointerDown };
            down.callback.AddListener((data) => StartRemoveHold(index));
            trigger.triggers.Add(down);

            var up = new EventTrigger.Entry { eventID = EventTriggerType.PointerUp };
            up.callback.AddListener((data) => StopRemoveHold());
            trigger.triggers.Add(up);
        }

        upgradeButton.onClick.AddListener(ApplyUpgrade);
    }



    void AddPotion(int index)
    {
        if (removeHoldJustStopped) 
        {
            removeHoldJustStopped = false; return;
        }
        if (addHoldJustStopped)
        {
            addHoldJustStopped = false; return; // Evita el click fantasma
        }
                                                // 🚫 No hay más pociones disponibles en inventario
            if (selectedPotions[index] >= potionCounts[index])
            return;

        int remainingXP = GetRemainingXPToMax();
        if (remainingXP <= 0) return;

        if (potionXPValues[index] > remainingXP)
            return;

        // ✔ Añadir poción seleccionada (solo marcamos selección)
        selectedPotions[index]++;
        totalXP += potionXPValues[index];

        // Coste oro
        if (targetHero != null)
        {
            int nivel = targetHero.stats.level;
            goldCost += Mathf.RoundToInt((potionXPValues[index] / 2f) * (1f + nivel * 0.1f));
        }
        else
        {
            goldCost += potionXPValues[index] / 2;
        }

        // Feedback
        if (potionFeedbackObjects != null && potionFeedbackObjects.Length > index && potionFeedbackObjects[index] != null)
            potionFeedbackObjects[index].SetActive(true);

        RefreshUI();
    }


    System.Collections.IEnumerator DisableAfterSeconds(GameObject obj, float seconds)
    {
        yield return new WaitForSeconds(seconds);
        if (obj != null) obj.SetActive(false);
    }

    void ApplyUpgrade()
    {
        if (targetHero == null)
        {
            Debug.LogWarning("No hay héroe asignado.");
            return;
        }

        int maxLevel = targetHero.GetMaxLevel();

        if (targetHero.stats.level >= maxLevel)
        {
            Debug.LogWarning($"{targetHero.heroName} ya está en el nivel máximo ({maxLevel}). No se gastarán recursos.");
            totalXP = 0;
            goldCost = 0;
            // Opcional: resetear selección
            for (int i = 0; i < 5; i++)
                selectedPotions[i] = 0;

            RefreshUI();
            return;
        }

        if (GameManager.Instance.Gold < goldCost)
        {
            Debug.LogWarning("No hay suficiente oro.");
            return;
        }

        GameManager.Instance.Gold -= goldCost;
        targetHero.GainXP(totalXP);

        SaveHeroLevel(targetHero);

        // 🔥 AHORA SÍ, descontamos del inventario lo que estaba seleccionado
        for (int i = 0; i < 5; i++)
        {
            potionCounts[i] -= selectedPotions[i];
            selectedPotions[i] = 0;
        }

        SavePotionInventory();

        totalXP = 0;
        goldCost = 0;
        RefreshUI();

        foreach (var obj in potionFeedbackObjects)
            if (obj != null) obj.SetActive(false);
    }



    void SaveHeroLevel(Hero hero)
    {
        string key = $"{HeroLevelKeyPrefix}{hero.heroID}";
        PlayerPrefs.SetInt(key, hero.stats.level);
        PlayerPrefs.Save();
    }

    public void LoadHeroLevel()
    {
        if (targetHero == null) return;
        string key = $"{HeroLevelKeyPrefix}{targetHero.heroID}";
        int savedLevel = PlayerPrefs.GetInt(key, targetHero.stats.level);
        targetHero.stats.level = savedLevel;
        targetHero.UpdateCombatClass();
    }
    void StartHold(int index)
    {
        if (potionCounts[index] <= 0) return;

        isHolding = true;
        holdIndex = index;
        holdDelay = 0.4f;

        holdCoroutine = StartCoroutine(HoldRoutine());
    }


    void StopHold()
    {
        isHolding = false;
        holdIndex = -1;
        addHoldJustStopped = true;

        if (holdCoroutine != null)
        {
            StopCoroutine(holdCoroutine);
            holdCoroutine = null;
        }
    }




    void SavePotionInventory()
    {
        for (int i = 0; i < 5; i++)
        {
            PlayerPrefs.SetInt(PotionKeyPrefix + i, potionCounts[i]);
        }
        PlayerPrefs.Save();
    }

    void LoadPotionInventory()
    {
        for (int i = 0; i < 5; i++)
        {
            potionCounts[i] = PlayerPrefs.GetInt(PotionKeyPrefix + i, potionCounts[i]);
        }
    }

    void RefreshUI()
    {
        for (int i = 0; i < 5; i++)
        {
            int available = potionCounts[i] - selectedPotions[i];
            if (available < 0) available = 0;

            potionCountsText[i].text = available.ToString();
            potionSelectedText[i].text = selectedPotions[i].ToString();

            bool needsXP = GetRemainingXPToMax() > 0;
            bool canAdd = available > 0 && needsXP;

            // Cambia visual + interactuable
            SetButtonVisualState(potionButtons[i], canAdd);

            removePotionButtons[i].gameObject.SetActive(selectedPotions[i] > 0);
        }

        totalXPText.text = $" {totalXP}";
        goldCostText.text = $"{goldCost}";

        bool isMax = targetHero != null && targetHero.stats.level >= targetHero.GetMaxLevel();
        upgradeButton.interactable = totalXP > 0 && GameManager.Instance.Gold >= goldCost && !isMax;
        // ----------------------------
        // STATS ACTUALES
        // ----------------------------
        currentHPText.text = targetHero.stats.maxHealth.ToString();
        currentATKText.text = targetHero.stats.attack.ToString();
        currentDEFText.text = targetHero.stats.defense.ToString();

        // ----------------------------
        // STATS FUTUROS
        // ----------------------------
        GetFutureStats(out int fLevel, out float fHP, out float fATK, out float fDEF);

        previewHPText.text = fHP.ToString();
        previewATKText.text = fATK.ToString();
        previewDEFText.text = fDEF.ToString();

        // Colores (azul para preview)
        previewHPText.color = Color.cyan;
        previewATKText.color = Color.cyan;
        previewDEFText.color = Color.cyan;
        // ----------------------------
        // SLIDER DE XP Y NIVEL
        // ----------------------------
        int lvl = targetHero.stats.level;
        int xpActual = targetHero.stats.xp;
        int xpToLevel = targetHero.stats.xpToLevel;

        int xpConPociones = Mathf.Min(xpActual + totalXP, xpToLevel);

        xpSlider.maxValue = xpToLevel;
        xpSlider.value = xpConPociones;

        levelText.text = $"Nivel {lvl}";

    }



    public void AddFivePotionsEach()
    {
        for (int i = 0; i < potionCounts.Length; i++)
        {
            potionCounts[i] += 5;
        }
        SavePotionInventory();
        RefreshUI();
        Debug.Log("Se añadieron 5 pociones de cada tipo al inventario.");
    }
    IEnumerator HoldRoutine()
    {
        while (isHolding)
        {
            // 🔥 Detectar si el dedo se movió a otro botón
            int hovered = GetPotionButtonUnderPointer();
            if (hovered != -1 && hovered != holdIndex)
                holdIndex = hovered;

            if (holdIndex < 0)
            {
                StopHold();
                yield break;
            }

            int remainingXP = GetRemainingXPToMax();
            if (remainingXP <= 0)
            {
                StopHold();
                yield break;
            }

            bool added = false;

            // Intentar añadir del rango actual
            if (potionCounts[holdIndex] > selectedPotions[holdIndex] &&
                potionXPValues[holdIndex] <= remainingXP)
            {
                AddPotion(holdIndex);
                added = true;
                holdGraceTimer = 0f;

            }
            else
            {
                // Buscar una sola poción inferior
                for (int i = holdIndex - 1; i >= 0; i--)
                {
                    if (potionCounts[i] > selectedPotions[i] &&
                        potionXPValues[i] <= remainingXP)
                    {
                        AddPotion(i);
                        added = true;
                        holdGraceTimer = 0f;
                        break;
                    }
                }
            }

            if (!added)
            {
                holdGraceTimer += holdDelay;

                // 🔥 Si aún no han pasado 2.5s, NO parar
                if (holdGraceTimer < holdGraceDuration)
                {
                    yield return new WaitForSeconds(holdDelay);
                    continue;
                }

                // 🔥 Si ya pasaron 2.5s sin poder añadir nada → parar
                StopHold();
                yield break;
            }


            holdDelay = Mathf.Max(minDelay, holdDelay - acceleration);
            yield return new WaitForSeconds(holdDelay);
        }
    }


    int FindNextLowerPotion(int start)
    {
        for (int i = start - 1; i >= 0; i--)
        {
            if (potionCounts[i] > 0)
                return i;
        }
        return -1;
    }

    int GetRemainingXPToMax()
    {
        if (targetHero == null) return int.MaxValue;

        int maxLevel = targetHero.GetMaxLevel();

        // Si ya está en el nivel máximo → no necesita XP
        if (targetHero.stats.level >= maxLevel)
            return 0;

        // XP total necesario para llegar al nivel máximo
        int xpNeeded = 0;
        int currentLevel = targetHero.stats.level;
        int currentXP = targetHero.stats.xp;

        // Sumar XP faltante del nivel actual
        xpNeeded += targetHero.stats.xpToLevel - currentXP;

        // Sumar XP de niveles intermedios
        for (int lvl = currentLevel + 1; lvl < maxLevel; lvl++)
        {
            xpNeeded += 100 + (lvl - 1) * 50;
        }

        // 🔥 RESTAR EL XP YA ACUMULADO POR LAS POCIONES SELECCIONADAS
        xpNeeded -= totalXP;

        // Nunca devolver negativo
        return Mathf.Max(0, xpNeeded);
    }
    void RemovePotion(int index)
    {
        if (removeHoldJustStopped)
        {
            removeHoldJustStopped = false;
            return;
        }

        if (selectedPotions[index] <= 0) return;

        StopHold();
        StopRemoveHold();

        RemovePotionInternal(index);
        RefreshUI();
    }


    private Coroutine removeHoldCoroutine;
    private bool removing = false;

    void StartRemoveHold(int index)
    {
        if (selectedPotions[index] <= 0) return;

        removing = true;
        holdIndex = index;
        holdDelay = 0.4f;

        removeHoldCoroutine = StartCoroutine(RemoveHoldRoutine());
    }

    void StopRemoveHold()
    {
        removing = false;
        holdIndex = -1;

        // En vez de activar el flag directamente:
        StartCoroutine(BlockAddPotionOneFrame());

        if (removeHoldCoroutine != null)
        {
            StopCoroutine(removeHoldCoroutine);
            removeHoldCoroutine = null;
        }
    }
    IEnumerator BlockAddPotionOneFrame()
    {
        removeHoldJustStopped = true;
        yield return null; // esperar 1 frame
        removeHoldJustStopped = false;
    }



    IEnumerator RemoveHoldRoutine()
    {
        while (removing)
        {
            // 🔥 Detectar si el dedo se movió a otro botón de quitar
            int hovered = GetRemoveButtonUnderPointer();
            if (hovered != -1 && hovered != holdIndex)
                holdIndex = hovered;

            if (holdIndex < 0)
            {
                StopRemoveHold();
                yield break;
            }

            if (selectedPotions[holdIndex] <= 0)
            {
                StopRemoveHold();
                yield break;
            }

            RemovePotionInternal(holdIndex);
            RefreshUI();

            holdDelay = Mathf.Max(minDelay, holdDelay - acceleration);
            yield return new WaitForSeconds(holdDelay);
        }
    }

    void RemovePotionInternal(int index)
    {
        selectedPotions[index]--;
  

        totalXP -= potionXPValues[index];
        if (totalXP < 0) totalXP = 0;

        int nivel = targetHero != null ? targetHero.stats.level : 1;
        goldCost -= Mathf.RoundToInt((potionXPValues[index] / 2f) * (1f + nivel * 0.1f));
        if (goldCost < 0) goldCost = 0;
    }

    public void AutoFeedAll()
    {
        int remainingXP = GetRemainingXPToMax();
        if (remainingXP <= 0) return;

        for (int i = 4; i >= 0; i--) // de 5★ a 1★
        {
            while (potionCounts[i] - selectedPotions[i] > 0 &&
                   potionXPValues[i] <= remainingXP)
            {
                selectedPotions[i]++;
                totalXP += potionXPValues[i];

                int nivel = targetHero != null ? targetHero.stats.level : 1;
                goldCost += Mathf.RoundToInt((potionXPValues[i] / 2f) * (1f + nivel * 0.1f));

                remainingXP -= potionXPValues[i];
            }
        }

        RefreshUI();
    }
    void SetButtonVisualState(Button btn, bool enabled)
    {
        CanvasGroup cg = btn.GetComponent<CanvasGroup>();
        if (cg == null) return;

        if (enabled)
        {
            cg.alpha = 1f;
            // 🔥 Solo permitir interacción si NO estamos manteniendo pulsado
            if (!isHolding)
            {
                cg.interactable = true;
                cg.blocksRaycasts = true;
            }
        }
        else
        {
            cg.alpha = 0.4f; // gris
                             // 🔥 Si estoy manteniendo pulsado, NO desactivar el botón
            if (!isHolding)
            {
                cg.interactable = false;
                cg.blocksRaycasts = false;
            }
        }
    }
    int GetPotionButtonUnderPointer()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            for (int i = 0; i < potionButtons.Length; i++)
            {
                if (r.gameObject == potionButtons[i].gameObject)
                    return i;
            }
        }

        return -1;
    }
    int GetRemoveButtonUnderPointer()
    {
        PointerEventData eventData = new PointerEventData(EventSystem.current);
        eventData.position = Input.mousePosition;

        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        foreach (var r in results)
        {
            for (int i = 0; i < removePotionButtons.Length; i++)
            {
                if (r.gameObject == removePotionButtons[i].gameObject)
                    return i;
            }
        }

        return -1;
    }
    private void GetFutureStats(out int futureLevel, out float futureHP, out float futureATK, out float futureDEF)
    {
        if (targetHero == null)
        {
            futureLevel = 1;
            futureHP = futureATK = futureDEF = 0;
            return;
        }

        // Copias temporales
        int lvl = targetHero.stats.level;
        int xp = targetHero.stats.xp + totalXP;

        float hp = targetHero.stats.maxHealth;
        float atk = targetHero.stats.attack;
        float def = targetHero.stats.defense;

        int maxLevel = targetHero.GetMaxLevel();

        // Simular level-ups
        while (lvl < maxLevel && xp >= (100 + (lvl - 1) * 50))
        {
            xp -= (100 + (lvl - 1) * 50);
            lvl++;

            hp = Mathf.Round(hp * 1.15f);
            atk = Mathf.Round(atk * 1.12f);
            def = Mathf.Round(def * 1.08f);
        }

        futureLevel = lvl;
        futureHP = hp;
        futureATK = atk;
        futureDEF = def;
    }



}
