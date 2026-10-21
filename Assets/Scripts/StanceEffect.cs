using UnityEngine;

[CreateAssetMenu(menuName = "Skills/Effects/Stance")]
public class StanceEffect : SkillEffectBase
{
    public enum StanceType
    {
        Taunt,
        Counter,
        Guard,
        Reflect,
        Evasion,
        Thorns,
        Fortify,
        Berserk,
        Charge,
        Focus
    }

    public StanceType stance;
    public int durationTurns = 1;
    public float value;

    public override void Apply(Hero caster, Hero target)
    {
        // Opcional: podrías aplicar directamente aquí,
        // pero ahora lo manejamos vía StatusEffectDefinition-like.
    }

    public override void Apply(Hero caster, Enemy target) { }

    // 🔥 Conversor a tu sistema legacy StatusEffect
    public StatusEffect ToStatusEffect()
    {
        var eff = new StatusEffect
        {
            effectName = stance.ToString(),
            icon = icon,
            remainingTurns = durationTurns,

            // Tipo correcto según la postura
            type =
                stance == StanceType.Taunt ? StatusEffectType.Taunt :
                stance == StanceType.Counter ? StatusEffectType.Counter :
                stance == StanceType.Reflect ? StatusEffectType.Reflect :
                stance == StanceType.Guard ? StatusEffectType.Invincibility :
                StatusEffectType.Buff,

            // Flags
            taunt = (stance == StanceType.Taunt),
            counterAttack = (stance == StanceType.Counter),
            reflectDamage = (stance == StanceType.Reflect),
            preventsDamage = (stance == StanceType.Guard),

            // Stats
            attackDelta = (stance == StanceType.Berserk || stance == StanceType.Charge) ? value : 0f,
            defenseDelta = (stance == StanceType.Fortify || stance == StanceType.Guard) ? value : 0f
        };

        return eff;
    }

}
