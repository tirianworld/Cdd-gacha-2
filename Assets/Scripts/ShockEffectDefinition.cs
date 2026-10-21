using UnityEngine;

[CreateAssetMenu(menuName = "StatusEffects/Shock")]
public class ShockEffectDefinition : StatusEffectDefinition
{
    [Header("Shock Settings")]
    public float damagePercentPerStack = 0.15f; // 15% del ataque por stack
    public bool canStack = true;
}
