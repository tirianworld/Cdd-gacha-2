using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class SellItemUI : MonoBehaviour
{
    [Header("Item que se puede vender")]
    public Item_SO item;
    public int maxQuantity;

    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI quantityOwnedText;
    public Slider quantitySlider;
    public TextMeshProUGUI quantityToSellText;
    public TextMeshProUGUI sellValueText;
    public Button sellButton;
    public InventoryItemUI IIU;

    [Header("Managers")]
    public InventoryManager inventoryManager;
    public ShopManager shopManager;

    private void Start()
    {
        if (shopManager == null)
            shopManager = ShopManager.Instance;

        if (inventoryManager == null)
            inventoryManager = FindObjectOfType<InventoryManager>();

        // 🔥 Slider entre 0 y 1
        quantitySlider.minValue = 0f;
        quantitySlider.maxValue = 1f;
        quantitySlider.wholeNumbers = false;

        quantitySlider.onValueChanged.AddListener(OnSliderChanged);
        sellButton.onClick.AddListener(OnSellClicked);
    }
     void Update()
    {
        maxQuantity = IIU.quantity;

    }
    public void Setup(Item_SO itemToSell, int ownedAmount)
    {
        item = itemToSell;
        Refresh();
    }

    public void Refresh()
    {
        if (item == null) return;

        iconImage.sprite = item.icon;
        nameText.text = item.itemName;
        quantityOwnedText.text = $"{maxQuantity}";

        // 🔥 Por defecto vender todo
        quantitySlider.value = 1f;

        UpdateSellInfo();
    }

    private void OnSliderChanged(float value)
    {
        UpdateSellInfo();
    }

    private void UpdateSellInfo()
    {
        // 🔥 Cantidad real basada en porcentaje
        float percent = quantitySlider.value;
        int amountToSell = Mathf.RoundToInt(maxQuantity * percent);

        quantityToSellText.text = $" {amountToSell}";
        sellValueText.text = $"Valor: {amountToSell * item.sellValue}";
    }

    private void OnSellClicked()
    {
        int amountToSell = Mathf.RoundToInt(maxQuantity * quantitySlider.value);

        if (amountToSell <= 0)
            return;

        bool success = shopManager.Sell(item, amountToSell);

        if (success)
        {
            maxQuantity -= amountToSell;

            if (maxQuantity <= 0)
            {
                gameObject.SetActive(false);
                return;
            }

            Refresh();
        }
    }
}
