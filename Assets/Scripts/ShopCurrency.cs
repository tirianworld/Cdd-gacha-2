using UnityEngine;

public enum ShopCurrency
{
    Gold,
    MemoryCristals,
    HeroCoin  // Para cosas que cuestan heroCoins
}

public enum ShopProductType
{
    Item,
    Equipment,
    Hero,
    Pack
}

[CreateAssetMenu(fileName = "NewShopProduct", menuName = "Shop/Shop Product")]
public class ShopProduct_SO : ScriptableObject
{
    [Header("Datos básicos")]
    public string productId = "prod_001";
    public string productName = "Nuevo producto";
    [TextArea] public string description;

    public ShopProductType productType;
    public Sprite icon;

    [Header("Coste principal")]
    public ShopCurrency currency;
    public int costAmount = 0;

    [Header("Coste alternativo (opcional)")]
    public bool hasSecondaryCost = false;
    public ShopCurrency secondaryCurrency;
    public int secondaryCostAmount = 0;

    [Header("Referencia al contenido")]
    public Item_SO item;                   // Para productType = Item
    public Equipment_SO equipment;        // Para productType = Equipment
    public int heroID;                    // Para productType = Hero

    [Header("Pack (varios elementos)")]
    public Item_SO[] packItems;
    public int[] packItemCounts;

    public int[] packHeroIDs;
    public int packGold;
    public int packMemoryCristals;
}
