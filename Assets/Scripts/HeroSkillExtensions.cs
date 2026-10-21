// File: HeroSkillExtensions.cs
using System;
using System.Collections.Generic;
using UnityEngine;

public enum SkillEffectType { Damage, Heal, Buff, Debuff }
public enum SkillTargetType { SingleEnemy, AllEnemies, SingleAlly, AllAllies, Self }



[Serializable]
public class HeroSkillData
{
    public string skillName;
    [TextArea] public string description;
    public Sprite icon;
    public SkillEffectType effectType = SkillEffectType.Damage;
    public SkillTargetType targetType = SkillTargetType.SingleEnemy;

    // Base y escalado
    public float basePower = 10f;
    public float scalingAttack = 1f;
    public float scalingDefense = 0f;

    // Buff/Debuff
    public float buffAttackAdd = 0f;
    public float buffDefenseAdd = 0f;
    public int effectDurationTurns = 0;

    // Niveles (1–3)
    public float level1Multiplier = 1f;
    public float level2Multiplier = 1.35f;
    public float level3Multiplier = 1.8f;

    // Carga y cooldown
    public float maxCharge = 100f;
    public float chargePerTurn = 35f;
    public float cooldownTurns = 1f;

    // Restricciones por posición
    public List<int> usableFromPositions = new List<int> { 0, 1, 2, 3 };
    public List<int> targetPositions = new List<int> { 0, 1, 2, 3 };
}

[Serializable]
public class SkillRuntime
{
    public HeroSkillData data;
    public int currentLevel = 1;
    public float currentCharge = 0f;
    public int remainingCooldown = 0;

    public Sprite GetIconForCurrentLevel() => data.icon;

    public void AccumulateCharge()
    {
        currentCharge = Mathf.Min(data.maxCharge, currentCharge + data.chargePerTurn);
        currentLevel = currentCharge >= data.maxCharge * 0.75f ? 3 :
                       currentCharge >= data.maxCharge * 0.4f ? 2 : 1;
    }

    public void OnUsed()
    {
        currentCharge = 0f;
        currentLevel = 1;
        remainingCooldown = Mathf.RoundToInt(data.cooldownTurns);
    }

    public bool IsReady => remainingCooldown <= 0;

    public void TickCooldown()
    {
        if (remainingCooldown > 0) remainingCooldown--;
    }

    public float ComputePower(Hero caster)
    {
        float mult = currentLevel == 3 ? data.level3Multiplier :
                     currentLevel == 2 ? data.level2Multiplier : data.level1Multiplier;
        float scaled = data.basePower + caster.stats.attack * data.scalingAttack + caster.stats.defense * data.scalingDefense;
        return scaled * mult;
    }

    public float ComputePower(Enemy caster)
    {
        float mult = currentLevel == 3 ? data.level3Multiplier :
                     currentLevel == 2 ? data.level2Multiplier : data.level1Multiplier;
        float scaled = data.basePower + caster.stats.attack * data.scalingAttack + caster.stats.defense * data.scalingDefense;
        return scaled * mult;
    }

   


}
