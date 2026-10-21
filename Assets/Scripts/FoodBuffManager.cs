using UnityEngine;
using System.Collections.Generic;

public class FoodBuffManager : MonoBehaviour
{
    public static FoodBuffManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    private Food_SO activeFood;
    private int remainingTurns;

    public void ConsumeFood(Food_SO food)
    {
        activeFood = food;
        remainingTurns = food.durationTurns;

        ApplyFoodEffect(food);
        Debug.Log($"🍲 {food.foodName} consumida. Efecto activo por {remainingTurns} turnos.");
    }

    public void OnTurnPassed()
    {
        if (activeFood == null) return;

        remainingTurns--;
        if (remainingTurns <= 0)
        {
            RemoveFoodEffect(activeFood);
            activeFood = null;
            Debug.Log("⏳ El efecto de comida ha expirado.");
        }
    }

    void ApplyFoodEffect(Food_SO food)
    {
        var heroes = GameManager.Instance.roster;

        foreach (var hero in heroes)
        {
            if (!hero.isUnlocked) continue;

            switch (food.effectType)
            {
                case FoodEffectType.IncreaseCC:
                    hero.stats.attack += food.effectValue;
                    hero.stats.defense += food.effectValue;
                    break;

                case FoodEffectType.IncreaseAttack:
                    hero.stats.attack += food.effectValue;
                    break;

                case FoodEffectType.IncreaseDefense:
                    hero.stats.defense += food.effectValue;
                    break;

                case FoodEffectType.IncreaseHP:
                    hero.stats.maxHealth += food.effectValue;
                    hero.stats.currentHealth += food.effectValue;
                    break;

                case FoodEffectType.IncreaseUltGauge:
                    // Puedes integrarlo con tu ultiCharge
                    break;

                case FoodEffectType.Custom:
                 //   hero.activeEffects.Add(new StatusEffect(food.customStatusEffect));
                    break;
            }
        }
    }

    void RemoveFoodEffect(Food_SO food)
    {
        var heroes = GameManager.Instance.roster;

        foreach (var hero in heroes)
        {
            if (!hero.isUnlocked) continue;

            switch (food.effectType)
            {
                case FoodEffectType.IncreaseCC:
                    hero.stats.attack -= food.effectValue;
                    hero.stats.defense -= food.effectValue;
                    break;

                case FoodEffectType.IncreaseAttack:
                    hero.stats.attack -= food.effectValue;
                    break;

                case FoodEffectType.IncreaseDefense:
                    hero.stats.defense -= food.effectValue;
                    break;

                case FoodEffectType.IncreaseHP:
                    hero.stats.maxHealth -= food.effectValue;
                    hero.stats.currentHealth = Mathf.Min(hero.stats.currentHealth, hero.stats.maxHealth);
                    break;
            }
        }
    }
}
