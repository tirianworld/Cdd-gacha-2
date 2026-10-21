using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class FoodUIEntry
{
    public Food_SO food;
    public GameObject slotObject;
}

public class FoodUI : MonoBehaviour
{
    [Header("Catálogo de comidas")]
    public List<FoodUIEntry> allFoods;

    [Header("Panel comida seleccionada")]
    public Image selectedFoodIcon;

    [Header("Ingredientes UI")]
    public Image ingredientImage1;
    public Image ingredientImage2;
    public Image ingredientImage3;

    public TextMeshProUGUI ingredientText1;
    public TextMeshProUGUI ingredientText2;
    public TextMeshProUGUI ingredientText3;

    public Button craftButton;

    Food_SO currentSelectedFood;

    void Start()
    {
        PopulateFoodGrid();
    }

    void PopulateFoodGrid()
    {
        foreach (var entry in allFoods)
        {
            if (entry.food == null || entry.slotObject == null)
                continue;

            GameObject slot = entry.slotObject;
            Food_SO food = entry.food;

            slot.SetActive(true);

            var icon = slot.transform.Find("Icon")?.GetComponent<Image>();
            var nameText = slot.transform.Find("Name")?.GetComponent<TextMeshProUGUI>();

            if (icon != null) icon.sprite = food.icon;
            if (nameText != null) nameText.text = food.foodName;

            Button button = slot.GetComponent<Button>();

            if (button != null)
            {
                Food_SO capturedFood = food;

                button.onClick.RemoveAllListeners();
                button.onClick.AddListener(() => OnFoodSelected(capturedFood));
            }
        }
    }

    void OnFoodSelected(Food_SO food)
    {
        currentSelectedFood = food;
        UpdateSelectedFoodUI();
    }

    void UpdateSelectedFoodUI()
    {
        if (currentSelectedFood == null) return;

        bool canCraft = FoodCraftingManager.Instance.CanCraft(currentSelectedFood);

        selectedFoodIcon.sprite = currentSelectedFood.icon;
        selectedFoodIcon.color = canCraft ? Color.white : Color.gray;

        UpdateIngredient(0, ingredientImage1, ingredientText1);
        UpdateIngredient(1, ingredientImage2, ingredientText2);
        UpdateIngredient(2, ingredientImage3, ingredientText3);

        craftButton.interactable = canCraft;
    }

    void UpdateIngredient(int index, Image img, TextMeshProUGUI txt)
    {
        if (index >= currentSelectedFood.requiredIngredients.Count)
        {
            img.enabled = false;
            txt.text = "";
            return;
        }

        Item_SO item = currentSelectedFood.requiredIngredients[index];

        int required = currentSelectedFood.ingredientAmounts[index];
        int owned = GameManager.Instance.playerInventory.GetCount(item);

        img.enabled = true;
        img.sprite = item.icon;

        txt.text = owned + "/" + required;

        bool hasEnough = owned >= required;

        img.color = hasEnough ? Color.white : Color.gray;
        txt.color = hasEnough ? Color.white : Color.gray;
    }

    public void TryCookSelected()
    {
        if (currentSelectedFood == null) return;

        if (FoodCraftingManager.Instance.Craft(currentSelectedFood))
        {
            UpdateSelectedFoodUI();
        }
    }

    public void TryConsumeSelected()
    {
        if (currentSelectedFood == null) return;

        FoodBuffManager.Instance.ConsumeFood(currentSelectedFood);
    }
}