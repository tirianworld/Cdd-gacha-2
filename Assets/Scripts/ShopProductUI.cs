using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ShopProductUI : MonoBehaviour
{
    public ShopProduct_SO product;

    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI nameText;
    public TextMeshProUGUI descText;
    public TextMeshProUGUI costText;
    public Button buyButton;
    public ShopManager shopmanager;

    private void Start()
    {
        Refresh();
        buyButton.onClick.AddListener(OnBuyClicked);
        if (shopmanager == null)
            shopmanager = ShopManager.Instance;
      
    }
    private void Update()
    {
        buyButton.interactable = shopmanager.CanBuy(product);

    }
    public void Refresh()
    {
        if (product == null) return;

        if (iconImage != null) iconImage.sprite = product.icon;
        if (nameText != null) nameText.text = product.productName;
        if (descText != null) descText.text = product.description;

        if (costText != null)
        {
            string main = $"{product.costAmount}";
            if (product.hasSecondaryCost)
                main += $" + {product.secondaryCostAmount}";
            costText.text = main;
        }

        buyButton.interactable = shopmanager.CanBuy(product);
    }

    private void OnBuyClicked()
    {
        if (shopmanager.Buy(product))
            Refresh();
    }

    private void OnEnable()
    {
        Refresh();
    }
}
