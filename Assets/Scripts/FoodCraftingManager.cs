using UnityEngine;

public class FoodCraftingManager : MonoBehaviour
{
    public static FoodCraftingManager Instance;

    private void Awake()
    {
        Instance = this;
    }

    public bool CanCraft(Food_SO food)
    {
        int count = Mathf.Min(food.requiredIngredients.Count, food.ingredientAmounts.Count);

        for (int i = 0; i < count; i++)
        {
            var item = food.requiredIngredients[i];
            int amount = food.ingredientAmounts[i];

            if (GameManager.Instance.playerInventory.GetCount(item) < amount)
                return false;
        }

        return true;
    }

    public bool HasIngredient(Food_SO food, int index)
    {
        if (index >= food.requiredIngredients.Count || index >= food.ingredientAmounts.Count)
            return false;

        var item = food.requiredIngredients[index];
        int amount = food.ingredientAmounts[index];

        return GameManager.Instance.playerInventory.GetCount(item) >= amount;
    }

    public bool Craft(Food_SO food)
    {
        if (!CanCraft(food))
            return false;

        int count = Mathf.Min(food.requiredIngredients.Count, food.ingredientAmounts.Count);

        for (int i = 0; i < count; i++)
        {
            var item = food.requiredIngredients[i];
            int amount = food.ingredientAmounts[i];

            GameManager.Instance.playerInventory.RemoveItem(item, amount);
        }

        Debug.Log($"Comida creada: {food.foodName}");
        return true;
    }
}