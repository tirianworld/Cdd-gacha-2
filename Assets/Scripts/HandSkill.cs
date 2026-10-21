using UnityEngine;

[System.Serializable]
public class HandSkill
{
    public HeroSkill baseSkill;
    public int tier = 1;

    public HandSkill(HeroSkill skill)
    {
        baseSkill = skill;
        tier = 1;
    }
}
