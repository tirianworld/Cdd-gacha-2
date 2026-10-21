using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;
using System.Linq;

[System.Serializable]
public class SkinSlot
{
    [Header("Identidad")]
    public string skinName;
    public Sprite skinSprite;

    [Header("Estado")]
    public bool unlocked;
    public bool equipped;

    [Header("Bonos de estadísticas (porcentaje)")]
    [Tooltip("Bono de vida máxima en %, sobre las stats base actuales del héroe. Ej: 10 = +10%, -20 = -20%.")]
    public float healthBonusPercent = 0f;
    [Tooltip("Bono de ataque en %, sobre las stats base actuales del héroe.")]
    public float attackBonusPercent = 0f;
    [Tooltip("Bono de defensa en %, sobre las stats base actuales del héroe.")]
    public float defenseBonusPercent = 0f;

    [Header("Cambio de raza (opcional)")]
    [Tooltip("Si está activo, mientras esta skin esté equipada el héroe cuenta como estas razas en vez de las suyas.")]
    public bool overrideRace = false;
    public List<HeroRace> raceOverride = new List<HeroRace>();
    public List<string> subRaceOverride = new List<string>();

    [Header("Eventos (asignables en el Inspector)")]
    [Tooltip("Se dispara justo cuando esta skin pasa a estar equipada. Aquí puedes enganchar VFX, sonidos, bonos extra, cambios de clase, etc.")]
    public UnityEvent OnSkinEquipped;
    [Tooltip("Se dispara justo cuando esta skin deja de estar equipada.")]
    public UnityEvent OnSkinUnequipped;
}

public class SkinManager : MonoBehaviour
{
    public Hero hero;                     // Héroe asignado
    public SkinSlot[] skins;              // Array de skins (estado + bonos + eventos)

    // 🔥 Bonos actualmente aplicados al héroe, para poder revertirlos al cambiar de skin
    private float appliedHealthBonus = 0f;
    private float appliedAttackBonus = 0f;
    private float appliedDefenseBonus = 0f;

    // 🔥 Raza "real" del héroe, capturada antes de que ninguna skin la sobreescriba
    private List<HeroRace> baseRaces;
    private List<string> baseSubRaces;
    private bool raceOverrideActive = false;

    private void Start()
    {
        if (hero == null)
        {
            Debug.LogError("SkinManager no tiene Hero asignado.");
            return;
        }

        // Guardamos la raza original del héroe antes de aplicar ninguna skin
        baseRaces = new List<HeroRace>(hero.races);
        baseSubRaces = new List<string>(hero.subRaces);

        LoadSkins();
    }

    public void UnlockSkin(int index)
    {
        if (index < 0 || index >= skins.Length) return;

        skins[index].unlocked = true;
        SaveSkins();
    }

    public void EquipSkin(int index)
    {
        if (index < 0 || index >= skins.Length) return;
        if (!skins[index].unlocked)
        {
            Debug.LogWarning("Skin no desbloqueada.");
            return;
        }

        // Avisar a la skin anterior (si había una distinta) de que se desequipa
        var previous = skins.FirstOrDefault(s => s.equipped);
        if (previous != null && previous != skins[index])
            previous.OnSkinUnequipped?.Invoke();

        // Solo una equipada
        for (int i = 0; i < skins.Length; i++)
            skins[i].equipped = false;

        skins[index].equipped = true;

        if (hero != null)
            hero.currentSkinIndex = index;

        ApplySkinEffects();
        SaveSkins();
    }

    public void UnequipAllSkins()
    {
        var previous = skins.FirstOrDefault(s => s.equipped);
        if (previous != null)
            previous.OnSkinUnequipped?.Invoke();

        foreach (var skin in skins)
            skin.equipped = false;

        ApplySkinEffects();
        SaveSkins();
    }

    // 🔥 Aplica al héroe los bonos de estadísticas, la raza y dispara los eventos
    //    de la skin actualmente equipada, revirtiendo antes lo de la skin anterior.
    private void ApplySkinEffects()
    {
        if (hero == null) return;

        // Revertir bono de estadísticas de la skin anterior (evita que se acumulen)
        hero.stats.maxHealth -= appliedHealthBonus;
        hero.stats.attack -= appliedAttackBonus;
        hero.stats.defense -= appliedDefenseBonus;

        appliedHealthBonus = 0f;
        appliedAttackBonus = 0f;
        appliedDefenseBonus = 0f;

        // Revertir el cambio de raza de la skin anterior, si lo había
        RevertRaceOverride();

        foreach (var skin in skins)
        {
            if (skin.equipped)
            {
                // En este punto hero.stats ya está "limpio" (sin el bono de la skin anterior),
                // así que el porcentaje se calcula sobre las stats base actuales del héroe
                // (incluye subidas de nivel u otros bonos externos, pero no bonos de otras skins).
                appliedHealthBonus = hero.stats.maxHealth * (skin.healthBonusPercent / 100f);
                appliedAttackBonus = hero.stats.attack * (skin.attackBonusPercent / 100f);
                appliedDefenseBonus = hero.stats.defense * (skin.defenseBonusPercent / 100f);

                hero.stats.maxHealth += appliedHealthBonus;
                hero.stats.attack += appliedAttackBonus;
                hero.stats.defense += appliedDefenseBonus;

                if (skin.skinSprite != null)
                    hero.portrait = skin.skinSprite;

                ApplyRaceOverride(skin);

                skin.OnSkinEquipped?.Invoke();
            }
        }

        hero.stats.currentHealth = Mathf.Min(hero.stats.currentHealth, hero.stats.maxHealth);
        hero.UpdateCombatClass();
    }

    private void ApplyRaceOverride(SkinSlot skin)
    {
        if (!skin.overrideRace) return;

        hero.races = new List<HeroRace>(skin.raceOverride);
        hero.subRaces = new List<string>(skin.subRaceOverride);
        raceOverrideActive = true;
    }

    private void RevertRaceOverride()
    {
        if (!raceOverrideActive) return;

        if (baseRaces != null) hero.races = new List<HeroRace>(baseRaces);
        if (baseSubRaces != null) hero.subRaces = new List<string>(baseSubRaces);

        raceOverrideActive = false;
    }

    public void SaveSkins()
    {
        int id = hero.heroID;

        for (int i = 0; i < skins.Length; i++)
        {
            PlayerPrefs.SetInt($"Hero_{id}_Skin_{i}_Unlocked", skins[i].unlocked ? 1 : 0);
            PlayerPrefs.SetInt($"Hero_{id}_Skin_{i}_Equipped", skins[i].equipped ? 1 : 0);
        }

        PlayerPrefs.Save();
    }

    public void LoadSkins()
    {
        int id = hero.heroID;

        for (int i = 0; i < skins.Length; i++)
        {
            skins[i].unlocked = PlayerPrefs.GetInt($"Hero_{id}_Skin_{i}_Unlocked", 0) == 1;
            skins[i].equipped = PlayerPrefs.GetInt($"Hero_{id}_Skin_{i}_Equipped", 0) == 1;
        }
        EnsureDefaultSkin();

        // Aplicar al héroe los efectos de la skin que haya quedado equipada tras la carga
        ApplySkinEffects();

        int equippedIndex = System.Array.FindIndex(skins, s => s.equipped);
        if (equippedIndex >= 0)
            hero.currentSkinIndex = equippedIndex;
    }

    private void EnsureDefaultSkin()
    {
        bool anyEquipped = false;

        for (int i = 0; i < skins.Length; i++)
        {
            if (skins[i].equipped)
            {
                anyEquipped = true;
                break;
            }
        }

        // Solo activar skin 0 si NUNCA se ha equipado ninguna skin antes
        // (es decir, solo en la primera partida)
        if (!anyEquipped && skins.Length > 0)
        {
            // Pero solo si TODAS están locked
            bool allLocked = true;
            for (int i = 0; i < skins.Length; i++)
            {
                if (skins[i].unlocked)
                {
                    allLocked = false;
                    break;
                }
            }

            if (allLocked)
            {
                skins[0].unlocked = true;
                skins[0].equipped = true;
            }
        }
    }
}