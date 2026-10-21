using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class SimpleStatus
{
    public string name;
    public int turns;
    public float value;

    public SimpleStatus(string name, int turns, float value)
    {
        this.name = name;
        this.turns = turns;
        this.value = value;
    }
}

public class Enemy : MonoBehaviour
{
    [Header("Datos del enemigo")]
    public string enemyName;
    public EnemyStats stats;
    public List<EnemySkill> skills = new List<EnemySkill>();
    public Sprite portrait;

    [Header("Habilidades tipo HeroSkill (nuevo)")]
    public List<HeroSkill> heroSkills = new List<HeroSkill>();

    [Header("Estados aplicados (nuevo sistema)")]
    public List<SimpleStatus> debugStatuses = new List<SimpleStatus>();

    [Header("Compatibilidad con sistema antiguo")]
    public List<StatusEffect> activeEffects = new List<StatusEffect>();
    public bool skipNextTurn = false;
    public EnemyStatus status = new EnemyStatus();

    [Header("Eventos")]
    public System.Action OnEnemyDeath;
    public CombatManager combatManager;

    [Header("Eventos Unity")]
    public UnityEvent onEnemyDefeated;

    private void OnEnable()
    {
        if (combatManager != null)
        {
            combatManager.enemy = this;
            Debug.Log($"Enemy '{enemyName}' se ha registrado en CombatManager.");
        }
        else
        {
            Debug.LogWarning("No se encontró CombatManager al activar el enemigo.");
        }
    }

    void Start()
    {
        if (stats == null)
            stats = new EnemyStats();
    }

    public int GetDebuffCount()
    {
        return activeEffects.FindAll(e => e.type == StatusEffectType.Debuff).Count;
    }

    public int GetBuffCount()
    {
        return activeEffects.FindAll(e => e.type == StatusEffectType.Buff).Count;
    }

    public bool IsAlive => stats.currentHealth > 0;

    // ============================================================
    // APLICAR DAÑO
    // ============================================================
    public float ApplyDamage(float amount)
    {
        if (amount < 0) amount = 0;

        if (activeEffects.Exists(e => e.preventsDamage))
            return 0;

        stats.currentHealth -= amount;

        if (stats.currentHealth < 0)
            stats.currentHealth = 0;

        if (stats.currentHealth == 0)
        {
            Debug.Log($"Enemy '{enemyName}' ha sido derrotado.");

            OnEnemyDeath?.Invoke();
            onEnemyDefeated?.Invoke();
        }

        return amount;
    }

    // ============================================================
    // AÑADIR EFECTO (SISTEMA ANTIGUO)
    // ============================================================
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
            sourceDefinition = eff.sourceDefinition
        };

        activeEffects.Add(clone);
    }

    // ============================================================
    // TICK DE EFECTOS (SISTEMA ANTIGUO)
    // ============================================================
    public void TickStatusEffects()
    {
        int shockStacks = 0;
        float shockPercent = 0.15f;

        foreach (var eff in activeEffects)
        {
            if (eff.effectName == "Shock")
            {
                shockStacks++;

                if (eff.sourceDefinition is ShockEffectDefinition shockDef)
                    shockPercent = shockDef.damagePercentPerStack;
            }
        }

        if (shockStacks > 0)
        {
            float baseDamage = stats.attack * shockPercent;
            float totalDamage = baseDamage * shockStacks;
            float mitigated = Mathf.Max(0, totalDamage - stats.defense);

            stats.currentHealth = Mathf.Max(0, stats.currentHealth - mitigated);
        }

        foreach (var eff in activeEffects.ToArray())
        {
            if (eff.damagePerTurn > 0)
            {
                float mitigated = Mathf.Max(0, eff.damagePerTurn - stats.defense);
                stats.currentHealth = Mathf.Max(0, stats.currentHealth - mitigated);
            }

            if (eff.healPerTurn > 0)
            {
                stats.currentHealth = Mathf.Min(stats.maxHealth, stats.currentHealth + eff.healPerTurn);
            }

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
        status = new EnemyStatus();
        skipNextTurn = false;
    }

    // ============================================================
    // NUEVO: USAR HERO SKILL
    // ============================================================
    public void UseHeroSkill(HeroSkill skill, Hero target)
    {
        if (skill == null) return;

        // Evento: la skill se prepara
        skill.OnSkillPrepared?.Invoke();

        float damage = skill.basePower
                     + stats.attack * skill.scalingAttack
                     - target.stats.defense * skill.scalingDefense;

        damage = Mathf.Max(0, damage);

        // Aplicar daño
        target.ApplyDamage(damage);

        // Evento: la skill se ejecuta (impacto)
        skill.OnSkillExecuted?.Invoke();

        // Aplicar efectos
        if (skill.statusEffect != null)
            target.AddStatusEffect(new StatusEffect(skill.statusEffect));

        if (skill.healAmount > 0)
            stats.currentHealth = Mathf.Min(stats.maxHealth, stats.currentHealth + skill.healAmount);

        // Evento: la skill termina
        skill.OnSkillFinished?.Invoke();
    }


    public void UpdateDebugStatus(string name, int turns, float value)
    {
        var existing = debugStatuses.Find(s => s.name == name);

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
    public HeroSkill ChooseSmartSkill(Hero hero)
    {
        if (heroSkills == null || heroSkills.Count == 0)
            return null;

        // 🔒 Evitar skill sellada
        List<HeroSkill> usableSkills = new List<HeroSkill>(heroSkills);

        if (status.sealedSkillActive && status.sealedSkill != null)
            usableSkills.Remove(status.sealedSkill);

        if (usableSkills.Count == 0)
            return null;

        // 2. Curación si está bajo de vida
        if (stats.currentHealth < stats.maxHealth * 0.35f)
        {
            var healSkill = usableSkills.Find(s => s.healAmount > 0);
            if (healSkill != null)
                return healSkill;
        }

        // 3. Skill para rematar
        var killSkill = usableSkills.Find(s => s.basePower > 20);
        if (killSkill != null && hero.stats.currentHealth < killSkill.basePower * 1.5f)
            return killSkill;

        // 4. Buffs → ataque fuerte
        if (GetBuffCount() > 0)
        {
            var strongAttack = usableSkills.Find(s => s.scalingAttack > 0.8f);
            if (strongAttack != null)
                return strongAttack;
        }

        // 5. Aleatoria
        return usableSkills[Random.Range(0, usableSkills.Count)];
    }
}

    [System.Serializable]
public class EnemyStats
{
    public float maxHealth = 100;
    public float currentHealth = 100;
    public float attack = 10;
    public float defense = 5;
}
