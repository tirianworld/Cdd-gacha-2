using UnityEngine;

public abstract class SkillEffectBase : ScriptableObject
{
    public string effectName;
    public Sprite icon;

    public abstract void Apply(Hero caster, Hero target);
    public abstract void Apply(Hero caster, Enemy target);
}
