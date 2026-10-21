
using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Effects/Damage")]
public class DamageEffect : SkillEffectBase
{
  
    public enum AdvancedDamageType
    {
        Normal,
        Pierce,      // ignora defensa
        Crit,        // daño crítico
        Flood,       // más daño según vida actual
        WeakPoint,   // más daño si el objetivo tiene debuffs
        Rupture,     // más daño si el objetivo tiene buffs
        Detonate,    // más daño según turnos activos
        Charge,      // ignora defensa completamente
        Spike,       // crítico x2
        Shatter,     // ignora resistencia
        Sever,       // aumenta probabilidad de crítico
        PowerStrike, // más daño según resistencia del enemigo
        Amplify,     // más daño según buffs del caster
        Corrosion    // daño por % de vida máxima
    }

    public AdvancedDamageType damageType;
    public float basePower;
    public float scalingAttack;
    public float scalingDefense;

    public override void Apply(Hero caster, Enemy target)
    {
        float power = basePower
            + caster.stats.attack * scalingAttack
            + caster.stats.defense * scalingDefense;

        switch (damageType)
        {
            case AdvancedDamageType.Pierce:
                power *= 1.5f;
                break;

            case AdvancedDamageType.Crit:
                power *= 2f;
                break;

            case AdvancedDamageType.Flood:
                power *= (caster.stats.currentHealth / caster.stats.maxHealth);
                break;

            case AdvancedDamageType.WeakPoint:
                int debuffs = target.GetDebuffCount();
                power *= (1f + debuffs * 0.3f);
                break;

            case AdvancedDamageType.Rupture:
                int buffs = target.GetBuffCount();
                power *= (1f + buffs * 0.4f);
                break;

            case AdvancedDamageType.Detonate:
                power *= (1f + EnemyHeroExtensions.GetTurnCount() * 0.2f);
                break;

            case AdvancedDamageType.Charge:
                target.stats.defense = 0;
                break;

            case AdvancedDamageType.Spike:
                power *= 2f; // crítico x2
                break;
        }

        target.ApplyDamage(power);
    }

    public override void Apply(Hero caster, Hero target) { }
}
