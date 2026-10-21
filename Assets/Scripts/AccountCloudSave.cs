using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[Serializable]
public class AccountCloudSaveData
{
    public int version = 1;
    public long saveTimestamp;

    public int gold;
    public int memoryCristals;
    public int astralShards;
    public string playerName;

    public List<AccountInventoryItemSave> inventory = new List<AccountInventoryItemSave>();
    public List<AccountHeroCoinSave> heroCoins = new List<AccountHeroCoinSave>();
    public List<AccountGachaHistorySave> gachaHistory = new List<AccountGachaHistorySave>();
    public List<AccountEquipmentSave> equipmentInventory = new List<AccountEquipmentSave>();
    public List<AccountHeroSave> heroes = new List<AccountHeroSave>();
}

[Serializable]
public class AccountInventoryItemSave
{
    public string itemName;
    public int count;
}

[Serializable]
public class AccountHeroCoinSave
{
    public int heroID;
    public int amount;
}

[Serializable]
public class AccountGachaHistorySave
{
    public int heroID;
    public int count;
}

[Serializable]
public class AccountEquipmentSave
{
    public string uniqueID;
    public string itemName;
    public int level;
    public int stars;
    public int awakening;
}

[Serializable]
public class AccountHeroSkinSave
{
    public string skinName;
    public bool unlocked;
    public bool equipped;
    public float healthBonus;
    public float attackBonus;
    public float defenseBonus;
}

[Serializable]
public class AccountHeroSave
{
    public int heroID;
    public bool isUnlocked;
    public int level;
    public float maxHealth;
    public float attack;
    public float defense;
    public float currentHealth;
    public float stamina;
    public int bondLevel;
    public int stars;
    public int skinIndex;
    public int supportHeroID = -1;

    public List<string> equippedIDs = new List<string>();
    public List<AccountHeroSkinSave> skins = new List<AccountHeroSkinSave>();
    public List<HeroRace> races = new List<HeroRace>();
    public List<string> subRaces = new List<string>();
    public List<HeroClass> classes = new List<HeroClass>();

    public HeroReligion religion;
    public Alianza alianza;
}

public static class AccountCloudSave
{
    public const string FileName = "account_save_data.json";
    private const string LocalTimestampKey = "AccountSaveTimestamp";

    public static bool CanUseCloud =>
        GooglePlayCloudSave.Instance != null && Social.localUser.authenticated;

    public static long LocalTimestamp =>
        long.TryParse(PlayerPrefs.GetString(LocalTimestampKey, "0"), out long value) ? value : 0;

    public static void MarkLocalSaved(long timestamp)
    {
        PlayerPrefs.SetString(LocalTimestampKey, timestamp.ToString());
        PlayerPrefs.Save();
    }

    public static AccountCloudSaveData Build(GameManager gameManager)
    {
        AccountCloudSaveData data = new AccountCloudSaveData
        {
            saveTimestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            gold = gameManager.Gold,
            memoryCristals = gameManager.MemoryCristals,
            astralShards = gameManager.AstralShards,
            playerName = gameManager.PlayerName
        };

        if (gameManager.playerInventory != null)
        {
            foreach (var entry in gameManager.playerInventory.items)
            {
                if (entry?.item == null || entry.count <= 0) continue;

                data.inventory.Add(new AccountInventoryItemSave
                {
                    itemName = entry.item.itemName,
                    count = entry.count
                });
            }
        }

        foreach (var entry in gameManager.heroCoinInventory)
        {
            if (entry?.coin == null) continue;

            data.heroCoins.Add(new AccountHeroCoinSave
            {
                heroID = entry.coin.heroID,
                amount = entry.amount
            });
        }

        foreach (var entry in gameManager.gachaHistory)
        {
            data.gachaHistory.Add(new AccountGachaHistorySave
            {
                heroID = entry.heroID,
                count = entry.count
            });
        }

        foreach (var entry in gameManager.equipmentInventory)
        {
            var inst = entry?.instance;
            if (inst?.baseData == null) continue;

            if (string.IsNullOrEmpty(inst.uniqueID))
                inst.uniqueID = Guid.NewGuid().ToString();

            data.equipmentInventory.Add(new AccountEquipmentSave
            {
                uniqueID = inst.uniqueID,
                itemName = inst.baseData.itemName,
                level = inst.level,
                stars = inst.stars,
                awakening = inst.awakening
            });
        }

        foreach (var hero in gameManager.roster)
        {
            if (hero == null) continue;

            AccountHeroSave heroSave = new AccountHeroSave
            {
                heroID = hero.heroID,
                isUnlocked = hero.isUnlocked,
                level = hero.stats.level,
                maxHealth = hero.stats.maxHealth,
                attack = hero.stats.attack,
                defense = hero.stats.defense,
                currentHealth = hero.stats.currentHealth,
                stamina = hero.stats.stamina,
                bondLevel = hero.BondLevel,
                stars = hero.Stars,
                skinIndex = hero.currentSkinIndex,
                supportHeroID = hero.supportHero != null ? hero.supportHero.heroID : -1,
                races = new List<HeroRace>(hero.races),
                subRaces = new List<string>(hero.subRaces),
                classes = new List<HeroClass>(hero.heroClasses),
                religion = hero.heroReligion,
                alianza = hero.alianza
            };

            foreach (var inst in hero.equippedInstances)
            {
                if (inst != null && !string.IsNullOrEmpty(inst.uniqueID))
                    heroSave.equippedIDs.Add(inst.uniqueID);
            }

            {
               

            }

            data.heroes.Add(heroSave);
        }

        return data;
    }

    public static void Save(GameManager gameManager)
    {
        if (!CanUseCloud) return;

        AccountCloudSaveData data = Build(gameManager);
        string json = JsonUtility.ToJson(data);
        GooglePlayCloudSave.Instance.SaveToCloud(FileName, json);
        MarkLocalSaved(data.saveTimestamp);
    }

    public static void Load(GameManager gameManager, Action<AccountCloudSaveData> onLoaded)
    {
        if (!CanUseCloud)
        {
            onLoaded?.Invoke(null);
            return;
        }

        GooglePlayCloudSave.Instance.LoadFromCloud(FileName, json =>
        {
            if (string.IsNullOrEmpty(json))
            {
                onLoaded?.Invoke(null);
                return;
            }

            try
            {
                onLoaded?.Invoke(JsonUtility.FromJson<AccountCloudSaveData>(json));
            }
            catch (Exception ex)
            {
                Debug.LogError("[AccountCloudSave] No se pudo leer el guardado de cuenta: " + ex.Message);
                onLoaded?.Invoke(null);
            }
        });
    }

    public static void Apply(GameManager gameManager, AccountCloudSaveData data)
    {
        if (gameManager == null || data == null) return;

        gameManager.Gold = data.gold;
        gameManager.MemoryCristals = data.memoryCristals;
        gameManager.AstralShards = data.astralShards;
        gameManager.PlayerName = data.playerName ?? "";

        ApplyInventory(gameManager, data);
        ApplyHeroCoins(gameManager, data);
        ApplyGachaHistory(gameManager, data);
        ApplyEquipmentInventory(gameManager, data);
        ApplyHeroes(gameManager, data);

        MarkLocalSaved(data.saveTimestamp);
    }

    private static void ApplyInventory(GameManager gameManager, AccountCloudSaveData data)
    {
        if (gameManager.playerInventory == null) return;

        gameManager.playerInventory.Clear();

        foreach (var saved in data.inventory)
        {
            if (saved == null || string.IsNullOrEmpty(saved.itemName) || saved.count <= 0) continue;

            var item = gameManager.allItems.FirstOrDefault(it => it != null && it.itemName == saved.itemName);
            if (item != null)
                gameManager.playerInventory.AddItem(item, saved.count);
        }
    }

    private static void ApplyHeroCoins(GameManager gameManager, AccountCloudSaveData data)
    {
        gameManager.heroCoinInventory.Clear();

        foreach (var saved in data.heroCoins)
        {
            if (saved == null || saved.heroID < 0) continue;

            var coin = gameManager.heroCoinCatalog.FirstOrDefault(c => c != null && c.heroID == saved.heroID);
            if (coin == null) continue;

            gameManager.heroCoinInventory.Add(new GameManager.HeroCoinInventoryEntry
            {
                coin = coin,
                amount = saved.amount
            });
        }
    }

    private static void ApplyGachaHistory(GameManager gameManager, AccountCloudSaveData data)
    {
        gameManager.gachaHistory.Clear();

        foreach (var saved in data.gachaHistory)
        {
            if (saved == null || saved.heroID < 0) continue;

            gameManager.gachaHistory.Add(new GameManager.GachaHistoryEntry
            {
                heroID = saved.heroID,
                count = saved.count
            });
        }
    }

    private static void ApplyEquipmentInventory(GameManager gameManager, AccountCloudSaveData data)
    {
        gameManager.equipmentInventory.Clear();

        foreach (var saved in data.equipmentInventory)
        {
            if (saved == null || string.IsNullOrEmpty(saved.itemName)) continue;

            var equipment = gameManager.allEquipment.FirstOrDefault(eq => eq != null && eq.itemName == saved.itemName);
            if (equipment == null) continue;

            EquipmentInstance inst = new EquipmentInstance(equipment)
            {
                uniqueID = string.IsNullOrEmpty(saved.uniqueID) ? Guid.NewGuid().ToString() : saved.uniqueID,
                level = saved.level,
                stars = saved.stars,
                awakening = saved.awakening
            };

            gameManager.equipmentInventory.Add(new EquipmentInventoryEntry { instance = inst });
        }
    }

    private static void ApplyHeroes(GameManager gameManager, AccountCloudSaveData data)
    {
        foreach (var saved in data.heroes)
        {
            if (saved == null) continue;

            var hero = gameManager.roster.FirstOrDefault(h => h != null && h.heroID == saved.heroID);
            if (hero == null) continue;

            hero.isUnlocked = saved.isUnlocked;
            hero.stats.level = saved.level;
            hero.stats.maxHealth = saved.maxHealth;
            hero.stats.attack = saved.attack;
            hero.stats.defense = saved.defense;
            hero.stats.currentHealth = Mathf.Min(saved.currentHealth, saved.maxHealth);
            hero.stats.stamina = saved.stamina;
            hero.BondLevel = saved.bondLevel;
            hero.Stars = saved.stars;
            hero.currentSkinIndex = saved.skinIndex;
            hero.races = new List<HeroRace>(saved.races);
            hero.subRaces = new List<string>(saved.subRaces);
            hero.heroClasses = new List<HeroClass>(saved.classes);
            hero.heroReligion = saved.religion;
            hero.alianza = saved.alianza;

            hero.equippedInstances.Clear();
            foreach (string equippedID in saved.equippedIDs)
            {
                var entry = gameManager.equipmentInventory
                    .FirstOrDefault(e => e?.instance != null && e.instance.uniqueID == equippedID);

                if (entry != null)
                    hero.equippedInstances.Add(entry.instance);
            }

         

            hero.supportHero = saved.supportHeroID >= 0
                ? gameManager.roster.FirstOrDefault(h => h != null && h.heroID == saved.supportHeroID)
                : null;

            hero.UpdateCombatClass();
        }
    }
}
