using UnityEngine;

[CreateAssetMenu(menuName = "Combat/Status Effect")]
public class StatusEffectDefinition : ScriptableObject
{
    public string effectName;
    public Sprite icon;
    public StatusEffectType type;

    [Header("Duración")]
    public int durationTurns = 1;

    [Header("Modificadores")]
    public float attackDelta;
    public float defenseDelta;
    public float damagePerTurn;
    public float healPerTurn;

    [Header("Flags especiales")]
    public bool disablesActions;
    public bool preventsDamage;
    public bool reflectDamage;
    public bool counterAttack;
    public bool taunt;
}
