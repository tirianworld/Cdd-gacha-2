using UnityEngine;
using System.Collections.Generic;

public enum FoodEffectType
{
    IncreaseCC,
    IncreaseAttack,
    IncreaseDefense,
    IncreaseHP,
    IncreaseUltGauge,
    IncreaseGoldGain,
    ReduceStaminaCost,
    Custom
}

[CreateAssetMenu(menuName = "SDP/Food", fileName = "NewFood")]
public class Food_SO : ScriptableObject
{
    [Header("Información")]
    public string foodName;
    [TextArea] public string description;
    public Sprite icon;

    [Header("Ingredientes (3)")]
    public List<Item_SO> requiredIngredients = new List<Item_SO>(3);
    public List<int> ingredientAmounts = new List<int>(3);

    [Header("Efecto")]
    public FoodEffectType effectType;
    public float effectValue = 0f;
    public int durationTurns = 3;

    [Header("Buff personalizado opcional")]
    public StatusEffectDefinition customStatusEffect;
}