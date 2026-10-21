using System;
using UnityEngine;

public enum StatusEffectType
{
    Buff,
    Debuff,
    Poison,
    Bleed,
    Burn,
    Freeze,
    Petrify,
    Shock,
    Stun,
    Taunt,
    Counter,
    Reflect,
    Invincibility,
    Corrosion
}

[Serializable]
public class StatusEffect
{
    public string effectName;
    public Sprite icon;
    public int remainingTurns;

    public StatusEffectType type;
    public StatusEffectDefinition sourceDefinition;

    // Buffs / Debuffs
    public float attackDelta;
    public float defenseDelta;

    // Daño / curación por turno
    public float healPerTurn;
    public float damagePerTurn;

    // Flags especiales
    public bool disablesActions;   // Freeze, Petrify, Stun
    public bool preventsDamage;    // Invincibility
    public bool reflectDamage;     // Reflect stance
    public bool counterAttack;     // Counter stance
    public bool taunt;             // Taunt stance

    // ============================================================
    // 🔥 CONSTRUCTOR VACÍO (NECESARIO PARA CLONAR Y BUFFS MANUALES)
    // ============================================================
    public StatusEffect() { }

    // ============================================================
    // 🔥 CONSTRUCTOR DESDE StatusEffectDefinition
    // ============================================================
    public StatusEffect(StatusEffectDefinition def)
    {
        sourceDefinition = def;

        effectName = def.effectName;
        icon = def.icon;
        type = def.type;

        remainingTurns = def.durationTurns;

        attackDelta = def.attackDelta;
        defenseDelta = def.defenseDelta;

        healPerTurn = def.healPerTurn;
        damagePerTurn = def.damagePerTurn;

        disablesActions = def.disablesActions;
        preventsDamage = def.preventsDamage;
        reflectDamage = def.reflectDamage;
        counterAttack = def.counterAttack;
        taunt = def.taunt;
    }
    // ============================================================
    // 🔥 CONSTRUCTOR DESDE OTRO StatusEffect (CLON REAL)
    // ============================================================
    public StatusEffect(StatusEffect other)
    {
        effectName = other.effectName;
        icon = other.icon;
        remainingTurns = other.remainingTurns;

        type = other.type;
        sourceDefinition = other.sourceDefinition;

        attackDelta = other.attackDelta;
        defenseDelta = other.defenseDelta;

        healPerTurn = other.healPerTurn;
        damagePerTurn = other.damagePerTurn;

        disablesActions = other.disablesActions;
        preventsDamage = other.preventsDamage;
        reflectDamage = other.reflectDamage;
        counterAttack = other.counterAttack;
        taunt = other.taunt;
    }



    // ============================================================
    // 🔥 TICK EN HÉROE
    // ============================================================
    public void Tick(Hero targetHero)
    {
        if (remainingTurns <= 0)
            return;

        // Daño por turno
        if (damagePerTurn > 0)
            targetHero.ApplyDamage(damagePerTurn);

        // Curación por turno
        if (healPerTurn > 0)
            targetHero.RestoreHealth(healPerTurn);

        remainingTurns--;
    }

    // ============================================================
    // 🔥 TICK EN ENEMIGO
    // ============================================================
    public void Tick(Enemy targetEnemy)
    {
        if (remainingTurns <= 0)
            return;

        // Daño por turno con mitigación por defensa
        if (damagePerTurn > 0)
        {
            float mitigated = Mathf.Max(0, damagePerTurn - targetEnemy.stats.defense);
            targetEnemy.stats.currentHealth = Mathf.Max(0, targetEnemy.stats.currentHealth - mitigated);
        }

        // Curación por turno
        if (healPerTurn > 0)
        {
            targetEnemy.stats.currentHealth = Mathf.Min(targetEnemy.stats.maxHealth, targetEnemy.stats.currentHealth + healPerTurn);
        }

        remainingTurns--;
    }

}
