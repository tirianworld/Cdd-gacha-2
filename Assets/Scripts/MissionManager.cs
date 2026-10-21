using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

public class PersistentMissionManager : MonoBehaviour
{
    [Header("Eventos")]
    public UnityEvent onMissionStarted;
    public UnityEvent onMissionEnded;

    [Header("UI")]
    public Slider progressBar;
    public Text missionLabel;

    [Header("Datos de misión")]
    public PatrolMission_SO missionBase;
    private DateTime missionStartTime;
    private DateTime missionEndTime;
    private bool missionActive = false;

    [Header("Héroes asignados")]
    public Hero Hero1;
    public Hero Hero2;
    public Hero Hero3;
    public Hero Hero4;
    public HeroAssignmentManager HAM;

    private List<Hero> assignedHeroes = new List<Hero>();
    private string SaveKey => $"MissionData_{missionBase?.missionID ?? 0}";

    private void Start()
    {
        LoadMission();
    }

    private void Update()
    {
        if (!missionActive && HAM != null)
        {
            Hero1 = HAM.Hero1;
            Hero2 = HAM.Hero2;
            Hero3 = HAM.Hero3;
            Hero4 = HAM.Hero4;
        }

        if (missionActive)
        {
            double totalSeconds = (missionEndTime - missionStartTime).TotalSeconds;
            double elapsedSeconds = (DateTime.UtcNow - missionStartTime).TotalSeconds;
            float progress = Mathf.Clamp01((float)(elapsedSeconds / totalSeconds));

            if (progressBar != null)
                progressBar.value = progress;

            if (DateTime.UtcNow >= missionEndTime)
                EndMission();
        }
    }

    // ---------------------------------------------------------
    // NUEVO CÁLCULO DE PROBABILIDAD (COMPATIBLE CON MULTI-RAZA)
    // ---------------------------------------------------------
    private float CalculateSuccessChance(PatrolMission_SO mission, List<Hero> heroes)
    {
        float baseChance = 0f;

        // 1. Probabilidad base según dificultad
        switch (mission.missionCombatClass)
        {
            case CombatClassLevel.MuyFacil: baseChance = 80f; break;
            case CombatClassLevel.Principiante: baseChance = 65f; break;
            case CombatClassLevel.IntermedioFacil: baseChance = 55f; break;
            case CombatClassLevel.IntermedioDificil: baseChance = 45f; break;
            case CombatClassLevel.Dificil: baseChance = 35f; break;
            case CombatClassLevel.MuyDificil: baseChance = 25f; break;
            case CombatClassLevel.Insano: baseChance = 15f; break;
            case CombatClassLevel.TeLaMamas: baseChance = 5f; break;
        }

        // 2. Ajuste por stats de héroes
        foreach (var h in heroes)
        {
            float statScore = (h.stats.attack + h.stats.defense + h.stats.maxHealth * 0.2f + h.stats.stamina * 0.1f) * h.stats.level;
            baseChance += statScore * 0.02f;
        }

        // 3. Afinidades positivas (MULTI-RAZA)
        foreach (var h in heroes)
        {
            foreach (var r in h.races)
                if (mission.optimalRaces.Contains(r))
                    baseChance += 8f;

            foreach (var c in h.heroClasses)
                if (mission.optimalClasses.Contains(c))
                    baseChance += 6f;

            if (mission.optimalReligions.Contains(h.heroReligion))
                baseChance += 10f;
        }

        // 4. Penalizaciones por conflictos internos (MULTI-RAZA)
        for (int i = 0; i < heroes.Count; i++)
        {
            for (int j = i + 1; j < heroes.Count; j++)
            {
                var h1 = heroes[i];
                var h2 = heroes[j];

                // Conflictos de religión
                if (RelationshipDatabase.ReligionConflicts.TryGetValue((h1.heroReligion, h2.heroReligion), out float relPenalty))
                    baseChance += relPenalty;

                // Conflictos de raza (multi-raza)
                foreach (var r1 in h1.races)
                {
                    foreach (var r2 in h2.races)
                    {
                        if (RelationshipDatabase.RaceConflicts.TryGetValue((r1, r2), out float racePenalty))
                            baseChance += racePenalty;
                    }
                }

                // Conflictos de clase
                foreach (var c1 in h1.heroClasses)
                    foreach (var c2 in h2.heroClasses)
                        if (RelationshipDatabase.ClassConflicts.TryGetValue((c1, c2), out float classPenalty))
                            baseChance += classPenalty;
            }
        }

        // 5. Normalización
        baseChance = Mathf.Clamp(baseChance, 5f, 95f);
        return baseChance;
    }

    // ---------------------------------------------------------
    // REQUISITOS ESTRICTOS (COMPATIBLE CON MULTI-RAZA)
    // ---------------------------------------------------------
    private bool MeetsStrictRequirements(PatrolMission_SO mission, List<Hero> heroes)
    {
        // RAZAS requeridas (MULTI-RAZA)
        foreach (var r in mission.requiredRaces)
        {
            if (!heroes.Any(h => h.races.Contains(r)))
            {
                Debug.LogWarning($"[Misión {mission?.missionName}] Falta al menos un héroe de raza requerida: {r}.");
                return false;
            }
        }

        // CLASES estrictas requeridas
        foreach (var c in mission.requiredClassesStrict)
        {
            if (!heroes.Any(h => h.heroClasses.Contains(c)))
            {
                Debug.LogWarning($"[Misión {mission?.missionName}] Falta al menos un héroe de clase requerida: {c}.");
                return false;
            }
        }

        // RELIGIONES requeridas
        foreach (var rel in mission.requiredReligions)
        {
            if (!heroes.Any(h => h.heroReligion == rel))
            {
                Debug.LogWarning($"[Misión {mission?.missionName}] Falta al menos un héroe con religión requerida: {rel}.");
                return false;
            }
        }

        // Requisitos suaves
        if (mission.requiredClasses != null && mission.requiredClasses.Count > 0)
        {
            foreach (var reqClass in mission.requiredClasses)
            {
                if (!heroes.Any(h => h.heroClasses.Contains(reqClass)))
                {
                    Debug.LogWarning($"[Misión {mission?.missionName}] Falta un héroe de clase {reqClass} (requisito suave).");
                    return false;
                }
            }
        }

        return true;
    }

    // ---------------------------------------------------------
    // MISIONES
    // ---------------------------------------------------------
    public void StartMission()
    {
        assignedHeroes = new List<Hero> { HAM.Hero1, HAM.Hero2, HAM.Hero3, HAM.Hero4 }
            .Where(h => h != null)
            .Distinct()
            .ToList();

        if (assignedHeroes.Count < missionBase.requiredTeamSize)
        {
            Debug.LogWarning($"[Misión {missionBase?.missionName}] Necesitas {missionBase.requiredTeamSize} héroes distintos para iniciar la misión.");
            assignedHeroes.Clear();
            return;
        }

        if (!MeetsStrictRequirements(missionBase, assignedHeroes))
        {
            assignedHeroes.Clear();
            return;
        }

        bool algunoOcupado = assignedHeroes.Any(h => h.GetComponent<DispatchedMarker>() != null);
        if (algunoOcupado)
        {
            Debug.LogWarning($"[Misión {missionBase?.missionName}] No puedes iniciar: uno o más héroes ya están desplegados.");
            assignedHeroes.Clear();
            return;
        }

        float successChance = CalculateSuccessChance(missionBase, assignedHeroes);
        float roll = UnityEngine.Random.Range(0f, 100f);

        if (roll > successChance)
        {
            Debug.LogWarning($"[Misión {missionBase?.missionName}] Falló el intento. Probabilidad era {successChance:F1}%. Vuelve a intentarlo con mejor composición.");
            assignedHeroes.Clear();
            return;
        }

        float difficultyScore = 0f;
        foreach (var h in assignedHeroes)
        {
            float eqBonus = 0f;
            foreach (var inst in h.equippedInstances)
            {
                var eq = inst.baseData;
                eqBonus += eq.attackBonus + eq.defenseBonus + eq.healthBonus * 0.1f;
            }


            difficultyScore += h.stats.level * 2f + h.stats.attack + h.stats.defense + eqBonus;
        }

        float baseDuration = missionBase != null ? missionBase.durationSeconds : 60f;
        float missionDuration = Mathf.Clamp(baseDuration * (100f / Mathf.Max(1f, difficultyScore)), 30f, baseDuration);

        missionStartTime = DateTime.UtcNow;
        missionEndTime = missionStartTime.AddSeconds(missionDuration);
        missionActive = true;

        foreach (var h in assignedHeroes)
            if (h.GetComponent<DispatchedMarker>() == null)
                h.gameObject.AddComponent<DispatchedMarker>();

        if (missionLabel != null)
            missionLabel.text = $"Misión: {missionBase?.missionName ?? "Desconocida"}";

        SaveMission();

        Debug.Log($"[Misión {missionBase?.missionName}] Iniciada. Éxito anticipado (roll {roll:F1} <= {successChance:F1}). Termina en {missionEndTime} UTC.");
        onMissionStarted?.Invoke();
    }

    private void EndMission()
    {
        missionActive = false;
        if (progressBar != null)
            progressBar.value = 1f;

        foreach (var h in assignedHeroes)
        {
            if (h == null) continue;
            var marker = h.GetComponent<DispatchedMarker>();
            if (marker != null) Destroy(marker);
        }

        Hero heroToUnlock = GameManager.Instance.roster
            .FirstOrDefault(h => !h.isUnlocked);

        if (heroToUnlock != null)
        {
            heroToUnlock.isUnlocked = true;
            Debug.Log($"🎉 Héroe {heroToUnlock.heroName} desbloqueado tras la misión!");
        }

        assignedHeroes.Clear();
        PlayerPrefs.DeleteKey(SaveKey + "_End");
        PlayerPrefs.DeleteKey(SaveKey + "_Start");
        PlayerPrefs.Save();

        onMissionEnded?.Invoke();
    }

    // ---------------------------------------------------------
    // GUARDADO Y RESTAURACIÓN
    // ---------------------------------------------------------
    private void SaveMission()
    {
        PlayerPrefs.SetString(SaveKey + "_End", missionEndTime.ToBinary().ToString());
        PlayerPrefs.SetString(SaveKey + "_Start", missionStartTime.ToBinary().ToString());
        PlayerPrefs.Save();
    }

    private void LoadMission()
    {
        if (PlayerPrefs.HasKey(SaveKey + "_End") && PlayerPrefs.HasKey(SaveKey + "_Start"))
        {
            long binaryEnd = Convert.ToInt64(PlayerPrefs.GetString(SaveKey + "_End"));
            long binaryStart = Convert.ToInt64(PlayerPrefs.GetString(SaveKey + "_Start"));
            missionEndTime = DateTime.FromBinary(binaryEnd);
            missionStartTime = DateTime.FromBinary(binaryStart);

            missionActive = true;

            Debug.Log($"🔄 [Misión {missionBase?.missionName}] restaurada desde guardado.");
        }
    }
}
