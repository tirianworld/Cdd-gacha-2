using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Effects/Damage Effect Definition")]
public class DamageEffectDefinition : ScriptableObject
{
    public string effectName;

    [Header("Icono opcional")]
    public Sprite icon;

    [Header("Animación en CombatSpriteController")]
    public string animationTrigger;

    [Header("Tipo avanzado de daño")]
    public DamageType damageType = DamageType.Normal;

    [Header("Escalado")]
    public float basePower = 10f;
    public float scalingAttack = 1f;
    public float scalingDefense = 0f;

    public enum DamageType
    {
        Normal,
        Pierce,
        Crit,
        Flood,
        WeakPoint,
        Rupture,
        Detonate,
        Charge,
        Spike,
        Shatter,
        Sever,
        PowerStrike,
        Amplify,
        Corrosion
    }

    public float ComputeDamage(Hero caster, Enemy target)
    {
        float power = basePower
            + caster.stats.attack * scalingAttack
            + caster.stats.defense * scalingDefense;

        switch (damageType)
        {
            case DamageType.Pierce:
                power *= 1.5f;
                break;

            case DamageType.Crit:
                power *= 2f;
                break;

            case DamageType.Flood:
                power *= (caster.stats.currentHealth / caster.stats.maxHealth);
                break;

            case DamageType.WeakPoint:
                power *= (1f + target.GetDebuffCount() * 0.3f);
                break;

            case DamageType.Rupture:
                power *= (1f + target.GetBuffCount() * 0.4f);
                break;

            case DamageType.Detonate:
                power *= (1f + EnemyHeroExtensions.GetTurnCount() * 0.2f);
                break;

            case DamageType.Charge:
                target.stats.defense = 0;
                break;

            case DamageType.Spike:
                power *= 2f;
                break;
        }

        return power;
    }

    // ⭐ NUEVO: efectos secundarios del daño
    public void ApplySecondaryEffects(Hero caster, Enemy target)
    {
        switch (damageType)
        {
            case DamageType.Corrosion:
                target.stats.defense = Mathf.Max(0, target.stats.defense - 5);
                break;

            case DamageType.Shatter:
               
                break;

            case DamageType.Amplify:
                caster.activeEffects.Add(new StatusEffect
                {
                    effectName = "Amplify Buff",
                    attackDelta = 10,
                    remainingTurns = 2
                });
                break;

            case DamageType.Sever:
                target.stats.currentHealth -= 10;
                break;

            case DamageType.PowerStrike:
                caster.RestoreHealth(10);
                break;
        }
    }
}
