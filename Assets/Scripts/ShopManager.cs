using UnityEngine;
using System.Linq;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public GameManager gameManager;
    public EquipmentManager equipmentManager;
    public InventoryManager playerInventory;

    [Header("Catálogo de la tienda")]
    public ShopProduct_SO[] products;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        if (gameManager == null) gameManager = GameManager.Instance;
        if (equipmentManager == null) equipmentManager = EquipmentManager.Instance;

    }

    // ----------- UTILIDAD: MONEDAS -----------

    public bool HasCurrency(ShopCurrency currency, int amount, int heroIDForHeroCoins = -1)
    {
        if (amount <= 0) return true;

        switch (currency)
        {
            case ShopCurrency.Gold:

                return gameManager.Gold >= amount;
            case ShopCurrency.MemoryCristals:
                return gameManager.MemoryCristals >= amount;
            case ShopCurrency.HeroCoin:
                if (heroIDForHeroCoins < 0)
                {
                    Debug.LogWarning("Se requiere heroID para comprobar HeroCoins.");
                    return false;
                }
                var entry = gameManager.GetHeroCoin(heroIDForHeroCoins);
                return entry != null && entry.amount >= amount;
        }
        return false;
    }

    public void SpendCurrency(ShopCurrency currency, int amount, int heroIDForHeroCoins = -1)
    {
        if (amount <= 0) return;

        switch (currency)
        {
            case ShopCurrency.Gold:
                gameManager.Gold -= amount;
                gameManager.UpdateMoney();
                Debug.Log("Antes: " + gameManager.Gold);
                gameManager.Gold -= amount;
                Debug.Log("Después: " + gameManager.Gold);

                break;
            case ShopCurrency.MemoryCristals:
                gameManager.MemoryCristals -= amount;
                gameManager.UpdateMoney();
                break;
            case ShopCurrency.HeroCoin:
                if (heroIDForHeroCoins < 0)
                {
                    Debug.LogWarning("Se requiere heroID para gastar HeroCoins.");
                    return;
                }
                var entry = gameManager.GetHeroCoin(heroIDForHeroCoins);
                if (entry != null)
                    entry.amount -= amount;

                break;
        }

        gameManager.SaveData();
    }
    public void SpendCurrencyButton()
    {
    }
    // ----------- API PRINCIPAL: COMPRAR -----------

    public bool CanBuy(ShopProduct_SO product)
    {
        if (product == null) return false;

        bool mainOK = HasCurrency(product.currency, product.costAmount, product.heroID);
        if (!mainOK) return false;

        if (product.hasSecondaryCost)
        {
            bool secOK = HasCurrency(product.secondaryCurrency, product.secondaryCostAmount, product.heroID);
            return secOK;
        }

        return true;
    }

    public bool Buy(ShopProduct_SO product)
    {
        if (product == null)
        {
            Debug.LogWarning("Producto nulo.");
            return false;
        }

        if (!CanBuy(product))
        {
            Debug.Log("No tienes suficientes recursos para comprar " + product.productName);
            return false;
        }

        // Cobrar
        SpendCurrency(product.currency, product.costAmount, product.heroID);
        if (product.hasSecondaryCost)
            SpendCurrency(product.secondaryCurrency, product.secondaryCostAmount, product.heroID);

        // Dar recompensa
        GrantProduct(product);

        Debug.Log("Compraste: " + product.productName);
        return true;
    }

    // ----------- ENTREGAR RECOMPENSA -----------

    private void GrantProduct(ShopProduct_SO product)
    {
        switch (product.productType)
        {
            case ShopProductType.Item:
                GrantItem(product);
                break;
            case ShopProductType.Equipment:
                GrantEquipment(product);
                break;
            case ShopProductType.Hero:
                GrantHero(product);
                break;
            case ShopProductType.Pack:
                GrantPack(product);
                break;
        }
    }

    private void GrantItem(ShopProduct_SO product)
    {
        if (product.item == null)
        {
            Debug.LogWarning("Producto de tipo Item pero sin Item_SO asignado.");
            return;
        }

        // Aquí decides cuántas unidades da la compra (ej: 1)
        gameManager.playerInventory.AddItem(product.item, 1);
        gameManager.SaveInventory();
    }

    private void GrantEquipment(ShopProduct_SO product)
    {
        if (product.equipment == null)
        {
            Debug.LogWarning("Producto de tipo Equipment pero sin Equipment_SO asignado.");
            return;
        }

        // Crear instancia individual del equipo
        EquipmentInstance inst = new EquipmentInstance(product.equipment);

        // Guardarla en el inventario global
        equipmentManager.AddToAccount(inst);
    }


    private void GrantHero(ShopProduct_SO product)
    {
        Hero hero = gameManager.roster.FirstOrDefault(h => h.heroID == product.heroID);
        if (hero == null)
        {
            Debug.LogError($"No se encontró héroe con ID {product.heroID} en el roster.");
            return;
        }

        if (!hero.isUnlocked)
        {
            hero.UnlockHero();
            hero.Stars = Mathf.Max(hero.Stars, 1);
            gameManager.SaveHeroStars(hero);
        }
        else
        {
            // Si ya está desbloqueado, decide la recompensa de dupes:
            // por ejemplo, dar HeroCoins:
            gameManager.AddHeroCoins(hero.heroID, 10);
        }
    }

    private void GrantPack(ShopProduct_SO product)
    {
        // Items del pack
        if (product.packItems != null && product.packItemCounts != null)
        {
            for (int i = 0; i < product.packItems.Length && i < product.packItemCounts.Length; i++)
            {
                if (product.packItems[i] != null && product.packItemCounts[i] > 0)
                {
                    gameManager.playerInventory.AddItem(product.packItems[i], product.packItemCounts[i]);
                }
            }
        }

        // Héroes del pack
        if (product.packHeroIDs != null)
        {
            foreach (int heroID in product.packHeroIDs)
            {
                var hero = gameManager.roster.FirstOrDefault(h => h.heroID == heroID);
                if (hero != null && !hero.isUnlocked)
                {
                    hero.UnlockHero();
                    hero.Stars = Mathf.Max(hero.Stars, 1);
                    gameManager.SaveHeroStars(hero);
                }
                else if (hero != null)
                {
                    // Dupe: quizá monedas de héroe
                    gameManager.AddHeroCoins(hero.heroID, 10);
                }
            }
        }

        // Monedas directas
        gameManager.Gold += product.packGold;
        gameManager.MemoryCristals += product.packMemoryCristals;

        gameManager.SaveInventory();
        gameManager.SaveData();
    }
    public bool Sell(Item_SO item, int amount)
    {
        if (!playerInventory.HasItem(item, amount))
            return false;

        playerInventory.RemoveItem(item, amount);
        gameManager.Gold += item.sellValue * amount;

        return true;
    }

}
