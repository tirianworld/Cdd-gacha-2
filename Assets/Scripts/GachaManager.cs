using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class GachaManager : MonoBehaviour
{
    public static GachaManager Instance;

    [Header("Banner activo")]
    public GachaBanner_SO activeBanner;

    [Header("Referencias")]
    public GameManager gameManager;
    private string SoftPityUsedKey => $"Gacha_{activeBanner.bannerId}_SoftPityUsed";

    // ==== EVENTOS PARA UI / FX ====
    public event Action OnSummonStarted;                       // Empieza animación de invocación
    public event Action<GachaPullResult[]> OnSummonFinished;   // Resultados para mostrar cartas
    public event Action<int, int, bool, bool> OnPityUpdated;   // steps, hardPity, softReached, hardReached

    // Claves PlayerPrefs
    private string StepsKey => $"Gacha_{activeBanner.bannerId}_Steps";
    private string RotationKey => $"Gacha_{activeBanner.bannerId}_RotationStep";

    [Header("FX por rareza")]
    public GameObject fxR;
    public GameObject fxSR;
    public GameObject fxSSR;

    [Header("FX por organización")]
    public GameObject fxCaldoDragon;
    public GameObject fxOtherFaction;



    public int GetRotationStep()
    {
        return PlayerPrefs.GetInt(RotationKey, 0);
    }

    private void SetRotationStep(int step)
    {
        PlayerPrefs.SetInt(RotationKey, step);
        PlayerPrefs.Save();
    }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameManager == null)
            gameManager = GameManager.Instance;
    }

    // =======================
    //      PITY / STEPS
    // =======================

    public int GetPitySteps()
    {
        return PlayerPrefs.GetInt(StepsKey, 0);
    }

    private void AddSteps(int count)
    {
        int steps = GetPitySteps();
        steps += count;

        PlayerPrefs.SetInt(StepsKey, steps);
        PlayerPrefs.Save();

        NotifyPityUI();
    }

    private void ResetSteps()
    {
        PlayerPrefs.SetInt(StepsKey, 0);
        PlayerPrefs.Save();
        NotifyPityUI();
    }

    private void NotifyPityUI()
    {
        if (activeBanner == null) return;

        int steps = GetPitySteps();
        bool softReached = steps >= activeBanner.softPitySteps;
        bool hardReached = steps >= activeBanner.hardPitySteps;

        OnPityUpdated?.Invoke(
            steps,
            activeBanner.hardPitySteps,
            softReached,
            hardReached
        );
    }

    // =======================
    //       CURRENCY
    // =======================

    private bool HasCurrencyForSingle()
    {
        return gameManager != null && gameManager.MemoryCristals >= activeBanner.costSingle;
    }

    private bool HasCurrencyForMulti()
    {
        return gameManager != null && gameManager.MemoryCristals >= activeBanner.costMulti;
    }

    private void SpendCurrencySingle()
    {
        gameManager.MemoryCristals -= activeBanner.costSingle;
    }

    private void SpendCurrencyMulti()
    {
        gameManager.MemoryCristals -= activeBanner.costMulti;
    }

    public bool CanPullSingle() => activeBanner != null && HasCurrencyForSingle();
    public bool CanPullMulti() => activeBanner != null && HasCurrencyForMulti();

    // =======================
    //   API: TIRADA SIMPLE
    // =======================

    public GachaPullResult PullSingle(bool triggerFX = true)
    {
        if (activeBanner.useRotation && activeBanner.rotation != null) return PullRotationStep(triggerFX);
        if (activeBanner == null)
        {
            Debug.LogWarning("No hay banner activo.");
            return default;
        }
        if (!HasCurrencyForSingle())
        {
            Debug.LogWarning("No tienes suficientes Cristales del Recuerdo.");
            return default;
        }

        SpendCurrencySingle();
        AddSteps(1);

        if (triggerFX)
            OnSummonStarted?.Invoke();

        var result = InternalRoll(false);
        ActivateFX(result);

        OnSummonFinished?.Invoke(new[] { result });

        gameManager.SaveData();
        return result;
    }
    private bool GetSoftPityUsed()
    {
        return PlayerPrefs.GetInt(SoftPityUsedKey, 0) == 1;
    }

    private void SetSoftPityUsed(bool value)
    {
        PlayerPrefs.SetInt(SoftPityUsedKey, value ? 1 : 0);
        PlayerPrefs.Save();
    }

    private GachaPullResult PullRotationStep(bool triggerFX)
    {
        int stepIndex = GetRotationStep();
        var rotation = activeBanner.rotation;

        if (rotation == null || rotation.steps.Count == 0)
            return default;

        var step = rotation.steps[stepIndex];

        // 1. Comprobar coste
        if (gameManager.MemoryCristals < step.cost)
        {
            Debug.LogWarning("No tienes recursos para este step.");
            return default;
        }

        gameManager.MemoryCristals -= step.cost;

        // 2. Activar FX
        if (triggerFX)
            OnSummonStarted?.Invoke();

        // 3. Tirada normal
        bool dummy = false;
        var result = InternalRoll(false, 0, ref dummy);

        // 4. Aplicar garantías del step
        if (step.guaranteedSR && result.rarity == GachaRarity.R)
            result.rarity = GachaRarity.SR;

        if (step.guaranteedSSR)
            result.rarity = GachaRarity.SSR;

        if (step.guaranteedRateUp)
            result.isRateUp = true;

        // 5. Avanzar step
        stepIndex++;
        if (stepIndex >= rotation.steps.Count)
            stepIndex = 0; // reinicia rotación

        SetRotationStep(stepIndex);

        // 6. Enviar resultado a la UI
        ActivateFX(result);
        OnSummonFinished?.Invoke(new[] { result });

        gameManager.SaveData();
        return result;
    }

    // =======================
    //     API: MULTI
    // =======================

    public List<GachaPullResult> PullMulti(bool triggerFX = true)
    {
        if (activeBanner == null)
        {
            Debug.LogWarning("No hay banner activo.");
            return null;
        }
        if (!HasCurrencyForMulti())
        {
            Debug.LogWarning("No tienes suficientes Cristales del Recuerdo para el multi.");
            return null;
        }

        SpendCurrencyMulti();
        AddSteps(activeBanner.multiSize);

        if (triggerFX)
            OnSummonStarted?.Invoke();

        List<GachaPullResult> results = new List<GachaPullResult>();
        bool hasSRorBetter = false;

        for (int i = 0; i < activeBanner.multiSize; i++)
        {
            var res = InternalRoll(true, i, ref hasSRorBetter);

            ActivateFX(res);

            results.Add(res);
        }


        OnSummonFinished?.Invoke(results.ToArray());
        gameManager.SaveData();

        return results;
    }

    // =======================
    //  LÓGICA DE UNA CARTA
    // =======================

    private GachaPullResult InternalRoll(bool isMulti)
    {
        bool dummy = false;
        return InternalRoll(isMulti, 0, ref dummy);
    }

    private GachaPullResult InternalRoll(bool isMulti, int slotIndex, ref bool hasSRorBetter)
    {
        bool forcedSSRBySoftPity = false;
        bool forcedRateUpByHardPity = false;

        var rarity = RollRarityWithPity(ref forcedSSRBySoftPity, ref forcedRateUpByHardPity);

        // Garantizar SR en multi
        if (isMulti && slotIndex == activeBanner.multiSize - 1 && activeBanner.guaranteeSRInMulti && !hasSRorBetter)
        {
            if (rarity == GachaRarity.R)
                rarity = GachaRarity.SR;
        }

        if (rarity == GachaRarity.SR || rarity == GachaRarity.SSR)
            hasSRorBetter = true;

        var heroEntry = RollHeroFromPool(rarity, forcedRateUpByHardPity);
        var result = ResolveHeroReward(heroEntry, rarity, forcedSSRBySoftPity || forcedRateUpByHardPity);

        if (forcedRateUpByHardPity)
            ResetSteps();
       


        return result;
    }

    private GachaRarity RollRarityWithPity(ref bool forcedSSRBySoft, ref bool forcedRateUpByHard)
    {
        int steps = GetPitySteps();

        // HARD PITY (600)
        if (steps >= activeBanner.hardPitySteps && activeBanner.rateUpHeroID >= 0)
        {
            forcedRateUpByHard = true;
            return GachaRarity.SSR;
        }

        // SOFT PITY (300) — solo si NO se ha usado aún
        if (steps >= activeBanner.softPitySteps && !GetSoftPityUsed())
        {
            forcedSSRBySoft = true;
            SetSoftPityUsed(true); // marcar como usado
            return GachaRarity.SSR;
        }

        // Tirada normal
        float roll = UnityEngine.Random.value * 100f;

        if (roll < activeBanner.rateSSR)
            return GachaRarity.SSR;
        else if (roll < activeBanner.rateSSR + activeBanner.rateSR)
            return GachaRarity.SR;
        else
            return GachaRarity.R;
    }




    private GachaHeroEntry RollHeroFromPool(GachaRarity rarity, bool forceRateUp)
    {
        var candidates = activeBanner.heroes
            .Where(h => h.rarity == rarity)
            .ToList();

        if (candidates.Count == 0)
        {
            Debug.LogError($"No hay héroes configurados para la rareza {rarity}.");
            return default;
        }

        // HARD PITY: forzar el héroe rate-up
        if (forceRateUp && activeBanner.rateUpHeroID >= 0)
        {
            var rateUp = candidates.FirstOrDefault(h => h.heroID == activeBanner.rateUpHeroID);
            if (rateUp != null)
                return rateUp;
        }

        // SSR normal con probabilidad de ser rate-up
        if (rarity == GachaRarity.SSR && activeBanner.rateUpHeroID >= 0)
        {
            if (UnityEngine.Random.value < activeBanner.rateUpSSROverride)
            {
                var rateUp = candidates.FirstOrDefault(h => h.heroID == activeBanner.rateUpHeroID);
                if (rateUp != null)
                    return rateUp;
            }
        }

        // Ruleta ponderada
        float totalWeight = candidates.Sum(c => c.weight);
        float roll = UnityEngine.Random.Range(0f, totalWeight);
        float accum = 0f;

        foreach (var c in candidates)
        {
            accum += c.weight;
            if (roll <= accum)
                return c;
        }

        return candidates[candidates.Count - 1];
    }

    private GachaPullResult ResolveHeroReward(GachaHeroEntry entry, GachaRarity rarity, bool triggeredPity)
    {
        Hero resultHero = null;
        bool isNew = false;
        bool isRateUp = entry.heroID == activeBanner.rateUpHeroID || entry.isRateUp;

        if (gameManager != null)
        {
            resultHero = gameManager.roster.FirstOrDefault(h => h.heroID == entry.heroID);

            if (resultHero == null)
            {
                Debug.LogWarning($"No se encontró Hero con heroID {entry.heroID} en el roster.");
            }
            else
            {
                if (!resultHero.isUnlocked)
                {
                    // Héroe nuevo
                    isNew = true;
                    resultHero.UnlockHero();
                    resultHero.Stars = Mathf.Max(resultHero.Stars, 1);
                    gameManager.SaveHeroStars(resultHero);
                }
                else
                {
                    // Duplicado
                    GachaRewardResolver.ResolveDuplicate(resultHero, rarity, gameManager);
                }

                // Registrar SIEMPRE en historial (nuevo o duplicado)
                gameManager.RegisterGachaHero(entry.heroID);
            }
        }

        return new GachaPullResult
        {
            hero = resultHero,
            rarity = rarity,
            isNew = isNew,
            isRateUp = isRateUp,
            triggeredPity = triggeredPity
        };
    }
    private void ActivateFX(GachaPullResult res)
    {
        // Apagar todos primero
        if (fxR) fxR.SetActive(false);
        if (fxSR) fxSR.SetActive(false);
        if (fxSSR) fxSSR.SetActive(false);
        if (fxCaldoDragon) fxCaldoDragon.SetActive(false);
        if (fxOtherFaction) fxOtherFaction.SetActive(false);

        // Activar por rareza
        switch (res.rarity)
        {
            case GachaRarity.R:
                if (fxR) fxR.SetActive(true);
                break;

            case GachaRarity.SR:
                if (fxSR) fxSR.SetActive(true);
                break;

            case GachaRarity.SSR:
                if (fxSSR) fxSSR.SetActive(true);
                break;
        }

        // Activar por organización
        if (res.hero != null)
        {
            if (res.hero.alianza == Alianza.CaldoDeDragón)
            {
                if (fxCaldoDragon) fxCaldoDragon.SetActive(true);
            }
            else
            {
                if (fxOtherFaction) fxOtherFaction.SetActive(true);
            }
        }
    }

}
