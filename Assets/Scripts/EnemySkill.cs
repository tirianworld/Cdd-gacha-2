using UnityEngine;

[System.Serializable]
public class EnemySkill
{
    public string skillName;
    public float baseDamage;
    public float scalingAttack;

    // ⭐ NUEVO ⭐
    public GameObject visualPrefab;   // efecto visual del ataque del enemigo
}
