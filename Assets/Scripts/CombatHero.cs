using UnityEngine;
using System.Collections.Generic;

public class CombatHero
{
    public Hero baseHero; // referencia al héroe real
    public float currentHealth;
    public float attack;
    public float defense;

    public List<StatusEffect> activeEffects = new List<StatusEffect>();

    public CombatHero(Hero hero)
    {
        baseHero = hero;
        currentHealth = hero.stats.currentHealth;
        attack = hero.stats.attack;
        defense = hero.stats.defense;
    }

    public void ApplyDamage(float amount)
    {
        float mitigated = Mathf.Max(0, amount - defense);
        currentHealth -= mitigated;
        if (currentHealth < 0) currentHealth = 0;
    }

    public void RestoreHealth(float amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, baseHero.stats.maxHealth);
    }
}
