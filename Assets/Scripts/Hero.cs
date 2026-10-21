using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;
using System;

[System.Serializable]
public class HeroSkill
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;

    public SkillTargetType targetType = SkillTargetType.SingleEnemy;
    public SkillEffectType effectType = SkillEffectType.Damage;

    public float basePower = 10f;
    public float scalingAttack = 1f;
    public float scalingDefense = 0f;

    public List<int> usableFromPositions = new List<int> { 0, 1, 2, 3 };
    public List<int> targetPositions = new List<int> { 0, 1, 2, 3 };

    public string animationTrigger;

    [Header("Nuevo sistema de efectos")]
    public StatusEffectDefinition statusEffect;
    public StanceEffect stanceEffect;

    // Estos eventos son los que se mezclaban
    public UnityEvent OnSkillPrepared;
    public UnityEvent OnSkillExecuted;
    public UnityEvent OnSkillFinished;

    [Header("Nuevo sistema de daño")]
    public DamageEffectDefinition damageEffect;

    [Header("Nuevo sistema de soporte")]
    public float healAmount = 0f;
    public float buffAttack = 0f;
    public float buffDefense = 0f;
    public int buffDuration = 0;

    [Header("VFX")]
    public List<GameObject> visualPrefabs;

    [Header("Sprites por Tier")]
    public Sprite tier1Sprite;
    public Sprite tier2Sprite;
    public Sprite tier3Sprite;

    // 🔥 MÉTODO PARA CLONAR LA HABILIDAD
    public HeroSkill Clone()
    {
        HeroSkill clone = (HeroSkill)this.MemberwiseClone();

        // Creamos listas nuevas para que no compartan referencias de posición
        clone.usableFromPositions = new List<int>(this.usableFromPositions);
        clone.targetPositions = new List<int>(this.targetPositions);
        clone.visualPrefabs = new List<GameObject>(this.visualPrefabs);

        // 🔥 LO MÁS IMPORTANTE: Creamos eventos nuevos para que sean únicos de este Héroe
        clone.OnSkillPrepared = new UnityEvent();
        clone.OnSkillExecuted = new UnityEvent();
        clone.OnSkillFinished = new UnityEvent();

        return clone;
    }
}
public enum HeroAttribute
{
    Divino,
    Profano,
    Natural,
    Arcano
}
public enum Class
{
    Saber,
    Archer,
    Lancer,
    Rider,
    Caster,
    Assassin,
    Berserker,
    Ruler,
    Avenger,
    AlterEgo,
    MoonCancer,
    Foreigner,
    Pretender,
    Beast
}

[System.Serializable]
public class HeroStats
{
    public int level = 1;
    public float maxHealth = 100f;
    public float currentHealth = 100f;
    public float attack = 10f;
    public float defense = 5f;

    public float stamina = 100f;
    public float maxStamina = 100f;

    public int xp = 0;
    public int xpToLevel => 100 + (level - 1) * 50;
}

public enum HeroClass
{
    Guerrero, Paladin, Barbaro, Clerigo, Mago, Hechicero, Monje, Brujo, Bardo,
    Chaman, Druida, BlackGuard, Picaro, Gunslinguer, BloodHunter, Explorador
}

public enum Alianza
{
    CaldoDeDragón, ConsejoOmega, Magor, ElTribunal, PortadoresDeMarca, Moonhaven,
    Antiguos, MercadoNegro, Compañia, VaPorLibre
}

public enum HeroRace
{
    Human, Elf, Dwarf, Halfling, Gnome, HalfElf, HalfOrc, Dragonborn, Tiefling,
    Aarakocra, Genasi, Goliath, Tabaxi, Kenku, Lizardfolk, Tortle, Firbolg, Triton,
    YuanTi, Kobold, Goblin, Hobgoblin, Bugbear, Eladrin, SeaElf, ShadarKai, Duergar,
    DeepGnome, Githyanki, Githzerai, Aasimar, Warforged, Changeling, Kalashtar,
    Shifter, Centaur, Minotaur, SimicHybrid, Vedalken, Leonin, Satyr, Owlin, Harengon,
    Birdfolk, Humblefolk, Vulpin, DESCONOCIDO
}

public static class RaceDatabase
{
    public static readonly Dictionary<HeroRace, List<string>> SubRaces =
        new Dictionary<HeroRace, List<string>>()
    {
        { HeroRace.DESCONOCIDO, new List<string>{ "Ninguna" } },
        { HeroRace.Human, new List<string>{ "Standard", "Variant" } },
        { HeroRace.Elf, new List<string>{ "High","Wood","Dark (Drow)","Eladrin Spring","Eladrin Summer","Eladrin Autumn","Eladrin Winter","Sea Elf","Shadar-Kai" } },
        { HeroRace.Dwarf, new List<string>{ "Hill","Mountain","Duergar" } },
        { HeroRace.Halfling, new List<string>{ "Lightfoot","Stout","Ghostwise","Lotusden" } },
        { HeroRace.Gnome, new List<string>{ "Forest","Rock","Deep (Svirfneblin)" } },
        { HeroRace.HalfElf, new List<string>{ "High","Wood","Drow","Moon","Sun","Aquatic" } },
        { HeroRace.Dragonborn, new List<string>{ "Black","Blue","Brass","Bronze","Copper","Gold","Green","Red","Silver","White","Amethyst","Crystal","Emerald","Sapphire","Topaz" } },
        { HeroRace.Tiefling, new List<string>{ "Asmodeus","Baalzebul","Dispater","Fierna","Glasya","Levistus","Mammon","Mephistopheles","Zariel" } },
        { HeroRace.Genasi, new List<string>{ "Air","Earth","Fire","Water" } },
        { HeroRace.Shifter, new List<string>{ "Beasthide","Cliffwalk","Longstride","Swiftstride","Wildhunt" } },
        { HeroRace.Aasimar, new List<string>{ "Protector","Scourge","Fallen" } },
        { HeroRace.Birdfolk, new List<string>{ "Corvid","Gallus","Hawk","Owl","Raptor" } },
        { HeroRace.Humblefolk, new List<string>{ "Deer","Hedgehog","Mouse","Raccoon","Fox" } }

    };
}

public enum CombatClassLevel
{
    MuyFacil, Principiante, IntermedioFacil, IntermedioDificil,
    Dificil, MuyDificil, Insano, TeLaMamas
}

public enum HeroReligion
{
    Ateo, Sombriana, Piedad, CultoALosDragones, LosTresSables, LunaOscura, LuzSolar
}

public class Hero : MonoBehaviour
{
    public HeroStatus status = new HeroStatus();
    public Sprite portrait;
    public Sprite combatSprite;
    public string heroName = "New Hero";
    public int heroID;
    public bool isUnlocked = false;
    public HeroAttribute attribute;
    public List<HeroRace> races = new List<HeroRace>();
    public List<string> subRaces = new List<string>();
    public List<HeroClass> heroClasses = new List<HeroClass>();
    public int BondLevel;
    public HeroReligion heroReligion;
    public Alianza alianza;
    public int Stars;
    public List<Class> fateClasses = new List<Class>();
    public List<HeroSkill> skills = new List<HeroSkill>();

    public List<StatusEffect> activeEffects = new List<StatusEffect>();
    [Header("Support System")]
    public Hero supportHero; // El campo solicitado para el héroe de apoyo
    public EquipmentManager EM;
    public int UltimateLevel = 1;

    public Animator heroAnimator;
    public CombatClassLevel CC;

    public HeroStats stats = new HeroStats();
    public List<EquipmentInstance> equippedInstances = new List<EquipmentInstance>();



    // Dentro de la clase Hero
    // Habilidades otorgadas por equipo (solo lectura)
    [SerializeField]
    private List<HeroSkill> equipmentGrantedSkills = new List<HeroSkill>();

    // 🔥 El índice/portada de la skin activa lo gestiona SkinManager;
    // Hero solo expone estos campos para que otros sistemas (UI, guardado en nube) los lean.
    public int currentSkinIndex = 0;

    public List<Sprite> combatSprites = new List<Sprite>();
    public GameObject[] combatSpriteObjects = new GameObject[4];

    public List<string> tags = new List<string>();
    public int position = 0;
    public bool skipNextTurn = false;
    public bool IsExhausted => stats.stamina <= 0 || stats.currentHealth <= 0;
    private GameManager gm;

    private void Start()
    {
        stats.currentHealth = Mathf.Min(stats.currentHealth, stats.maxHealth);
        stats.stamina = Mathf.Min(stats.stamina, stats.maxStamina);
        UpdateCombatClass();

        gm = GameObject.Find("GameManager").GetComponent<GameManager>();
        foreach (Hero hero in gm.roster)
        {
            // Cada héroe intentará cargar su support usando la lista global
            hero.LoadSavedSupport(gm.roster);
        }




    }

    public void AddStatusEffect(StatusEffect eff)
    {
        StatusEffect clone = new StatusEffect
        {
            effectName = eff.effectName,
            icon = eff.icon,
            type = eff.type,
            remainingTurns = eff.remainingTurns,
            attackDelta = eff.attackDelta,
            defenseDelta = eff.defenseDelta,
            healPerTurn = eff.healPerTurn,
            damagePerTurn = eff.damagePerTurn,
            disablesActions = eff.disablesActions,
            preventsDamage = eff.preventsDamage,
            reflectDamage = eff.reflectDamage,
            counterAttack = eff.counterAttack,
            taunt = eff.taunt,

            // 🔥 NECESARIO PARA SHOCK
            sourceDefinition = eff.sourceDefinition
        };

        activeEffects.Add(clone);
    }


    public void TickStatusEffects()
    {
        int shockStacks = 0;
        float shockPercent = 0.15f; // valor por defecto

        // 1) Contar stacks y leer porcentaje desde el ScriptableObject
        foreach (var eff in activeEffects)
        {
            if (eff.effectName == "Shock")
            {
                shockStacks++;

                if (eff.sourceDefinition is ShockEffectDefinition shockDef)
                    shockPercent = shockDef.damagePercentPerStack;
            }
        }
        Debug.Log("efectos");
        // 2) Aplicar efectos
        foreach (var eff in activeEffects.ToArray())
        {
            // 🔥 SHOCK (solo una vez por turno)
            if (eff.effectName == "Shock")
            {
                float baseDamage = stats.attack * shockPercent;
                float totalDamage = baseDamage * shockStacks;

                float mitigated = Mathf.Max(0, totalDamage - stats.defense);
                stats.currentHealth = Mathf.Max(0, stats.currentHealth - mitigated);
            }

            // Daño por turno normal
            if (eff.damagePerTurn > 0)
            {
                float mitigated = Mathf.Max(0, eff.damagePerTurn - stats.defense);
                stats.currentHealth = Mathf.Max(0, stats.currentHealth - mitigated);
            }

            // Curación por turno
            if (eff.healPerTurn > 0)
            {
                stats.currentHealth = Mathf.Min(stats.maxHealth, stats.currentHealth + eff.healPerTurn);
            }

            // Stun / petrify / freeze
            if (eff.disablesActions)
                skipNextTurn = true;

            eff.remainingTurns--;

            if (eff.remainingTurns <= 0)
                activeEffects.Remove(eff);
        }
    }

    public void ClearStatusEffects()
    {
        activeEffects.Clear();
        status = new HeroStatus();
        skipNextTurn = false;
    }



    private void Update()
    {
        UpdateCombatClass();
    }

    public float ApplyDamage(float amount)
    {
        // FULL COUNTER: acumula daño recibido
        if (status.fullCounter)
        {
            status.fullCounterDamageAccum += amount;
        }


        stats.currentHealth -= amount;

        if (stats.currentHealth < 0)
            stats.currentHealth = 0;

        return amount; // ← CombatManager lo leerá
    }


    public void UpdateCombatClass()
    {
        float score = (stats.attack + stats.defense + stats.maxHealth * 0.2f + stats.stamina * 0.1f) * stats.level;

        if (score < 100) CC = CombatClassLevel.MuyFacil;
        else if (score < 250) CC = CombatClassLevel.Principiante;
        else if (score < 500) CC = CombatClassLevel.IntermedioFacil;
        else if (score < 800) CC = CombatClassLevel.IntermedioDificil;
        else if (score < 1200) CC = CombatClassLevel.Dificil;
        else if (score < 1800) CC = CombatClassLevel.MuyDificil;
        else if (score < 2000) CC = CombatClassLevel.Insano;
        else CC = CombatClassLevel.TeLaMamas;
    }

    public void UnlockHero() => isUnlocked = true;
    public void LockHero() => isUnlocked = false;

    public void RestoreHealth(float amount)
    {
        stats.currentHealth = Mathf.Min(stats.maxHealth, stats.currentHealth + amount);
    }

    public void UseStamina(float amount)
    {
        stats.stamina = Mathf.Max(0, stats.stamina - amount);
    }

    public void Rest()
    {
        stats.stamina = stats.maxStamina;
        stats.currentHealth = Mathf.Min(stats.maxHealth, stats.currentHealth + stats.maxHealth * 0.25f);
    }

    public void GainXP(int amount)
    {
        int maxLevel = GetMaxLevel();

        if (stats.level >= maxLevel)
        {
            stats.xp = stats.xpToLevel;
            Debug.Log($"{heroName} ya alcanzó el nivel máximo ({maxLevel}).");
            return;
        }

        stats.xp += amount;
        while (stats.xp >= stats.xpToLevel && stats.level < maxLevel)
        {
            stats.xp -= stats.xpToLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        int maxLevel = GetMaxLevel();
        if (stats.level >= maxLevel)
        {
            Debug.Log($"{heroName} ya está en el nivel máximo ({maxLevel}).");
            return;
        }

        stats.level += 1;
        stats.maxHealth = Mathf.Round(stats.maxHealth * 1.15f);
        stats.attack = Mathf.Round(stats.attack * 1.12f);
        stats.defense = Mathf.Round(stats.defense * 1.08f);
        stats.currentHealth = stats.maxHealth;
        stats.stamina = stats.maxStamina;
        Debug.Log($"{heroName} subió a nivel {stats.level}!");
        UpdateCombatClass();
    }

    public int GetMaxLevel()
    {
        if (Stars <= 0) return 45;
        if (Stars >= 5) return 75;
        return 45 + (Stars * 5);
    }

    public void UpgradeUltimate(GameManager gm)
    {
        var coin = gm.GetHeroCoin(heroID);

        if (coin.amount >= 5)
        {
            coin.amount -= 5;
            UltimateLevel++;
            gm.SaveData();
            Debug.Log($"{heroName} sube su ulti a nivel {UltimateLevel}");
        }
        else
        {
            Debug.Log("No tienes suficientes monedas de héroe.");
        }
    }

    public void Equip(EquipmentInstance inst)
    {
        if (inst == null) return;

        equippedInstances.Add(inst);

        stats.maxHealth += inst.baseData.healthBonus;
        stats.attack += inst.baseData.attackBonus;
        stats.defense += inst.baseData.defenseBonus;

        if (inst.baseData.grantedHeroSkill != null)
            AddEquipmentSkill(inst.baseData.grantedHeroSkill);

        UpdateCombatClass();
    }

    // 🔥 Revierte TODOS los bonos aplicados al equipar (base + bonos de la
    // instancia por nivel/estrellas/despertar) y quita la skill otorgada.
    // Se usa tanto al desequipar manualmente como al reemplazar un equipo
    // (equipar encima de uno ya puesto), para que los stats no se desincronicen.
    public void Unequip(EquipmentInstance inst)
    {
        if (inst == null) return;

        if (equippedInstances.Remove(inst))
        {
            stats.maxHealth -= inst.baseData.healthBonus;
            stats.attack -= inst.baseData.attackBonus;
            stats.defense -= inst.baseData.defenseBonus;

            stats.maxHealth -= inst.level * 2f;
            stats.attack -= inst.stars * 1.5f;
            stats.defense -= inst.awakening * 3f;

            if (inst.baseData.grantedHeroSkill != null)
                RemoveEquipmentSkill(inst.baseData.grantedHeroSkill);

            stats.currentHealth = Mathf.Min(stats.currentHealth, stats.maxHealth);
            UpdateCombatClass();
        }
    }

    private void OnDisable()
    {
        SaveHeroData();
        SaveHeroData_Prefs();
    }

    private void OnApplicationQuit()
    {
        SaveHeroData();
        SaveHeroData_Prefs();

        if (GooglePlayCloudSave.Instance != null)
            SaveHeroCloud();
    }

    // ============================================================
    // DEBUG
    // ============================================================
    // Dentro de la clase Hero : MonoBehaviour

    [Header("Estados aplicados (Debug)")]
    public List<SimpleStatus> debugStatuses = new List<SimpleStatus>();

    // ... otros métodos ...

    public void UpdateDebugStatus(string name, int turns, float value)
    {
        // Buscamos si ya existe ese estado para actualizarlo o añadirlo
        SimpleStatus existing = debugStatuses.Find(s => s.name == name);
        if (existing != null)
        {
            existing.turns = turns;
            existing.value = value;
        }
        else
        {
            debugStatuses.Add(new SimpleStatus(name, turns, value));
        }
    }
    public void AddEquipmentSkill(HeroSkill skill)
    {
        if (skill == null) return;

        HeroSkill clone = skill.Clone(); // 🔥 importante: evitar referencias compartidas
        equipmentGrantedSkills.Add(clone);
        skills.Add(clone);

        Debug.Log($"{heroName} obtuvo la habilidad extra: {skill.skillName}");
    }
    public void RemoveEquipmentSkill(HeroSkill skill)
    {
        if (skill == null) return;

        // Buscar la instancia clonada dentro de equipmentGrantedSkills
        HeroSkill found = equipmentGrantedSkills.Find(s => s.skillName == skill.skillName);
        if (found != null)
        {
            equipmentGrantedSkills.Remove(found);
            skills.Remove(found);
            Debug.Log($"{heroName} perdió la habilidad extra: {skill.skillName}");
        }
    }
    public void Add_Afinity(int affinity)
    {
        BondLevel += affinity;
    }
    public void SaveHeroData()
    {
        PlayerPrefs.SetInt($"Hero_{heroID}_BondLevel", BondLevel);
        PlayerPrefs.SetInt($"Hero_{heroID}_Level", stats.level);
        PlayerPrefs.SetFloat($"Hero_{heroID}_MaxHealth", stats.maxHealth);
        PlayerPrefs.SetFloat($"Hero_{heroID}_Attack", stats.attack);
        PlayerPrefs.SetFloat($"Hero_{heroID}_Defense", stats.defense);
        PlayerPrefs.SetFloat($"Hero_{heroID}_CurrentHealth", stats.currentHealth);
        PlayerPrefs.SetFloat($"Hero_{heroID}_Stamina", stats.stamina);

        PlayerPrefs.Save();
        GameManager.Instance?.SyncAccountSaveToGooglePlay();
    }

    public void LoadHeroData()
    {
        BondLevel = PlayerPrefs.GetInt($"Hero_{heroID}_BondLevel", BondLevel);
        stats.level = PlayerPrefs.GetInt($"Hero_{heroID}_Level", stats.level);
        stats.maxHealth = PlayerPrefs.GetFloat($"Hero_{heroID}_MaxHealth", stats.maxHealth);
        stats.attack = PlayerPrefs.GetFloat($"Hero_{heroID}_Attack", stats.attack);
        stats.defense = PlayerPrefs.GetFloat($"Hero_{heroID}_Defense", stats.defense);
        stats.currentHealth = PlayerPrefs.GetFloat($"Hero_{heroID}_CurrentHealth", stats.currentHealth);
        stats.stamina = PlayerPrefs.GetFloat($"Hero_{heroID}_Stamina", stats.stamina);

        UpdateCombatClass();
    }

    public void SaveHeroCloud()
    {
        // Protección: si el sistema de guardado en la nube no está listo, abortar
        if (GooglePlayCloudSave.Instance == null)
        {
            Debug.LogWarning($"[Cloud] No se puede guardar héroe {heroID}: CloudSave no inicializado.");
            return;
        }

        HeroCloudData data = new HeroCloudData();
        data.saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();

        data.heroID = heroID;

        data.level = stats.level;
        data.maxHealth = stats.maxHealth;
        data.attack = stats.attack;
        data.defense = stats.defense;
        data.currentHealth = stats.currentHealth;
        data.stamina = stats.stamina;

        data.bondLevel = BondLevel;
        data.stars = Stars;
        data.skinIndex = currentSkinIndex;

        // Guardar IDs de equipo equipado
        data.equippedIDs.Clear();
        foreach (var inst in equippedInstances)
        {
            if (inst != null && !string.IsNullOrEmpty(inst.uniqueID))
                data.equippedIDs.Add(inst.uniqueID);
        }

        // Raza, subraza, clases
        data.races = new List<HeroRace>(races);
        data.subRaces = new List<string>(subRaces);
        data.classes = new List<HeroClass>(heroClasses);

        data.religion = heroReligion;
        data.alianza = alianza;

        string json = JsonUtility.ToJson(data);

        GooglePlayCloudSave.Instance.SaveToCloud("Hero_" + heroID, json);

        Debug.Log($"[Cloud] Héroe {heroID} guardado en la nube.");
    }



    public void LoadHeroCloud()
    {
        GooglePlayCloudSave.Instance.LoadFromCloud("Hero_" + heroID, (json) =>
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.LogWarning($"[Cloud] No existe guardado para héroe {heroID}");
                return;
            }

            HeroCloudData data = JsonUtility.FromJson<HeroCloudData>(json);

            // Restaurar stats
            stats.level = data.level;
            stats.maxHealth = data.maxHealth;
            stats.attack = data.attack;
            stats.defense = data.defense;
            stats.currentHealth = data.currentHealth;
            stats.stamina = data.stamina;

            BondLevel = data.bondLevel;
            Stars = data.stars;
            currentSkinIndex = data.skinIndex;

            // Restaurar razas, clases, etc.
            races = new List<HeroRace>(data.races);
            subRaces = new List<string>(data.subRaces);
            heroClasses = new List<HeroClass>(data.classes);

            heroReligion = data.religion;
            alianza = data.alianza;

            // Restaurar equipo equipado
            equippedInstances.Clear();

            foreach (string id in data.equippedIDs)
            {
                var entry = GameManager.Instance.equipmentInventory
                    .FirstOrDefault(e => e.instance.uniqueID == id);

                if (entry != null)
                {
                    equippedInstances.Add(entry.instance);

                    stats.maxHealth += entry.instance.baseData.healthBonus;
                    stats.attack += entry.instance.baseData.attackBonus;
                    stats.defense += entry.instance.baseData.defenseBonus;

                    if (entry.instance.baseData.grantedHeroSkill != null)
                        AddEquipmentSkill(entry.instance.baseData.grantedHeroSkill);
                }
            }

            stats.currentHealth = Mathf.Min(stats.currentHealth, stats.maxHealth);

            Debug.Log($"[Cloud] Héroe {heroID} cargado desde la nube.");
        });
    }



    public void SaveHeroData_Prefs()
    {
        long timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds();
        PlayerPrefs.SetString($"Hero_{heroID}_Timestamp", timestamp.ToString());

        // ============================
        // 1. Guardar datos base del héroe
        // ============================
        PlayerPrefs.SetInt($"Hero_{heroID}_BondLevel", BondLevel);
        PlayerPrefs.SetInt($"Hero_{heroID}_Level", stats.level);
        PlayerPrefs.SetFloat($"Hero_{heroID}_MaxHealth", stats.maxHealth);
        PlayerPrefs.SetFloat($"Hero_{heroID}_Attack", stats.attack);
        PlayerPrefs.SetFloat($"Hero_{heroID}_Defense", stats.defense);
        PlayerPrefs.SetFloat($"Hero_{heroID}_CurrentHealth", stats.currentHealth);
        PlayerPrefs.SetFloat($"Hero_{heroID}_Stamina", stats.stamina);

        // ============================
        // 2. Guardar equipo equipado
        // ============================
        PlayerPrefs.SetInt($"Hero_{heroID}_EquippedCount", equippedInstances.Count);

        for (int i = 0; i < equippedInstances.Count; i++)
        {
            EquipmentInstance inst = equippedInstances[i];

            // Seguridad: si por alguna razón no tiene ID, generar uno
            if (string.IsNullOrEmpty(inst.uniqueID))
            {
                inst.uniqueID = System.Guid.NewGuid().ToString();
                Debug.LogWarning($"[Hero {heroID}] Equipo sin ID detectado, ID generado automáticamente.");
            }

            PlayerPrefs.SetString($"Hero_{heroID}_Eq_{i}_ID", inst.uniqueID);
        }

        PlayerPrefs.Save();

        Debug.Log($"[Hero {heroID}] Datos guardados correctamente ({equippedInstances.Count} equipos).");
        GameManager.Instance?.SyncAccountSaveToGooglePlay();
    }



    public void LoadHeroData_Prefs()
    {
        // ============================
        // 1. Cargar datos base del héroe
        // ============================
        BondLevel = PlayerPrefs.GetInt($"Hero_{heroID}_BondLevel", BondLevel);
        stats.level = PlayerPrefs.GetInt($"Hero_{heroID}_Level", stats.level);
        stats.maxHealth = PlayerPrefs.GetFloat($"Hero_{heroID}_MaxHealth", stats.maxHealth);
        stats.attack = PlayerPrefs.GetFloat($"Hero_{heroID}_Attack", stats.attack);
        stats.defense = PlayerPrefs.GetFloat($"Hero_{heroID}_Defense", stats.defense);
        stats.currentHealth = PlayerPrefs.GetFloat($"Hero_{heroID}_CurrentHealth", stats.currentHealth);
        stats.stamina = PlayerPrefs.GetFloat($"Hero_{heroID}_Stamina", stats.stamina);

        // ============================
        // 2. Resetear equipo equipado
        // ============================
        equippedInstances.Clear();

        // Resetear stats ANTES de aplicar equipo
        float baseMaxHealth = stats.maxHealth;
        float baseAttack = stats.attack;
        float baseDefense = stats.defense;

        stats.maxHealth = baseMaxHealth;
        stats.attack = baseAttack;
        stats.defense = baseDefense;

        // ============================
        // 3. Cargar IDs de equipo equipado
        // ============================
        int count = PlayerPrefs.GetInt($"Hero_{heroID}_EquippedCount", 0);

        for (int i = 0; i < count; i++)
        {
            string id = PlayerPrefs.GetString($"Hero_{heroID}_Eq_{i}_ID", "");

            // Si el ID está vacío → equipo antiguo sin ID → ignorar
            if (string.IsNullOrEmpty(id))
            {
                Debug.LogWarning($"[Hero {heroID}] Equipo sin ID encontrado, ignorado.");
                continue;
            }

            // Buscar equipo en inventario global
            var entry = GameManager.Instance.equipmentInventory
                .FirstOrDefault(e => e.instance.uniqueID == id);

            if (entry == null)
            {
                Debug.LogWarning($"[Hero {heroID}] No se encontró equipo con ID {id} en inventario.");
                continue;
            }

            EquipmentInstance inst = entry.instance;

            // Añadir a la lista del héroe
            equippedInstances.Add(inst);

            // Tira los bonus del equipo teniendo en cuenta la afinidad elemental del héroe
            // (si coincide con la del equipo, hay sinergia y el rango se ensancha)
            inst.baseData.RollBonuses(this.attribute);

            // Aplicar stats del equipo
            stats.maxHealth += inst.baseData.healthBonus;
            stats.attack += inst.baseData.attackBonus;
            stats.defense += inst.baseData.defenseBonus;

            // Aplicar habilidades otorgadas
            if (inst.baseData.grantedHeroSkill != null)
                AddEquipmentSkill(inst.baseData.grantedHeroSkill);
        }

        // ============================
        // 4. Ajustar vida actual y recalcular clase
        // ============================
        stats.currentHealth = Mathf.Min(stats.currentHealth, stats.maxHealth);
        UpdateCombatClass();

        Debug.Log($"[Hero {heroID}] Equipo cargado correctamente ({equippedInstances.Count} items).");
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            SaveHeroData();
            SaveHeroData_Prefs();
        }
    }
    public void LoadHeroHybrid()
    {
        LoadHeroData_Prefs();

        long localTimestamp = long.Parse(PlayerPrefs.GetString($"Hero_{heroID}_Timestamp", "0"));

        // 🔥 Protección: si el sistema de nube no está listo, abortar
        if (GooglePlayCloudSave.Instance == null)
        {
            Debug.LogWarning($"[Hybrid] CloudSave no inicializado. Solo datos locales para héroe {heroID}");
            return;
        }

        GooglePlayCloudSave.Instance.LoadFromCloud("Hero_" + heroID, (json) =>
        {
            if (string.IsNullOrEmpty(json))
            {
                Debug.Log($"[Hybrid] No hay datos en la nube para héroe {heroID}");
                return;
            }

            HeroCloudData cloud = JsonUtility.FromJson<HeroCloudData>(json);

            if (cloud.saveTimestamp <= localTimestamp)
            {
                Debug.Log($"[Hybrid] Datos locales más nuevos. Se mantiene PlayerPrefs para héroe {heroID}");
                return;
            }

            Debug.Log($"[Hybrid] Datos de nube más nuevos. Sobrescribiendo héroe {heroID}");

            ApplyCloudData(cloud);

            SaveHeroData_Prefs();
            GameManager.Instance?.RefreshEquippedSlots(this);
        });
    }

    private void ApplyCloudData(HeroCloudData data)
    {
        stats.level = data.level;
        stats.maxHealth = data.maxHealth;
        stats.attack = data.attack;
        stats.defense = data.defense;
        stats.currentHealth = data.currentHealth;
        stats.stamina = data.stamina;

        BondLevel = data.bondLevel;
        Stars = data.stars;
        currentSkinIndex = data.skinIndex;

        races = new List<HeroRace>(data.races);
        subRaces = new List<string>(data.subRaces);
        heroClasses = new List<HeroClass>(data.classes);

        heroReligion = data.religion;
        alianza = data.alianza;

        equippedInstances.Clear();

        foreach (string id in data.equippedIDs)
        {
            var entry = GameManager.Instance.equipmentInventory
                .FirstOrDefault(e => e.instance.uniqueID == id);

            if (entry != null)
            {
                equippedInstances.Add(entry.instance);

                // Tira los bonus del equipo teniendo en cuenta la afinidad elemental del héroe
                entry.instance.baseData.RollBonuses(this.attribute);

                stats.maxHealth += entry.instance.baseData.healthBonus;
                stats.attack += entry.instance.baseData.attackBonus;
                stats.defense += entry.instance.baseData.defenseBonus;

                if (entry.instance.baseData.grantedHeroSkill != null)
                    AddEquipmentSkill(entry.instance.baseData.grantedHeroSkill);
            }
        }

        stats.currentHealth = Mathf.Min(stats.currentHealth, stats.maxHealth);
    }
    public float GetSupportMultiplier()
    {
        if (supportHero == null) return 0f;
        int matches = 0;
        if (this.attribute == supportHero.attribute) matches++;
        if (this.heroReligion == supportHero.heroReligion) matches++;
        if (this.alianza == supportHero.alianza) matches++;
        if (this.races.Any(r => supportHero.races.Contains(r))) matches++;

        if (matches >= 3) return 0.25f; // 25% si coinciden 3 o más
        if (matches >= 1) return 0.20f; // 20% si coincide al menos 1
        return 0.10f;                  // 10% base
    }
    public HeroStats GetEffectiveStats()
    {
        // Creamos una copia de las estadísticas base del héroe actual
        HeroStats effectiveStats = new HeroStats
        {
            maxHealth = this.stats.maxHealth,
            attack = this.stats.attack,
            defense = this.stats.defense
        };

        if (supportHero != null)
        {
            float multiplier = GetSupportMultiplier();

            // Aumenta según las stats del support y el multiplicador calculado
            effectiveStats.maxHealth += supportHero.stats.maxHealth * multiplier;
            effectiveStats.attack += supportHero.stats.attack * multiplier;
            effectiveStats.defense += supportHero.stats.defense * multiplier;
        }

        return effectiveStats;
    }
    public void AssignSupportHero(Hero newSupport)
    {
        if (newSupport == null || newSupport == this) return;

        supportHero = newSupport;

        // 1. Guardar en PlayerPrefs usando un ID único para este héroe
        // Guardamos el ID del soporte vinculado a la clave de este héroe
        PlayerPrefs.SetInt("SupportForHero_" + this.heroID, supportHero.heroID);
        PlayerPrefs.Save();
        GameManager.Instance?.SyncAccountSaveToGooglePlay();

        // 2. Mejorar los stats
        ApplySupportBonuses();

        Debug.Log($"Soporte asignado: {supportHero.heroName}. Stats actualizados para {this.heroName}");
    }

    public void ApplySupportBonuses()
    {
        if (supportHero == null) return;

        float multiplier = GetSupportMultiplier();

        // Aplicamos la mejora a las estadísticas base (maxHealth, attack, defense) [1, 2]
        // Nota: Es recomendable tener variables de 'baseStats' y 'effectiveStats' 
        // para no sobreescribir permanentemente los valores originales cada vez.

        this.stats.maxHealth += supportHero.stats.maxHealth * multiplier;
        this.stats.attack += supportHero.stats.attack * multiplier;
        this.stats.defense += supportHero.stats.defense * multiplier;
    }
    public void LoadSavedSupport(List<Hero> allHeroes)
    {
        string key = "SupportForHero_" + this.heroID;
        if (PlayerPrefs.HasKey(key))
        {
            int savedSupportID = PlayerPrefs.GetInt(key);
            // Buscamos el héroe en la lista global (ej. el roster del GameManager) [3]
            supportHero = allHeroes.Find(h => h.heroID == savedSupportID);

            if (supportHero != null)
            {
                ApplySupportBonuses();
            }
        }
    }
}