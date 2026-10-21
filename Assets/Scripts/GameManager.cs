using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Linq;
[System.Serializable]
public class EquipmentSaveData
{
    public List<EquipmentInstance> equippedInstances;
}
[System.Serializable]
public class EquipmentInventoryEntry
{
    public EquipmentInstance instance;
}


public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    private bool isApplyingAccountCloudSave;
    private bool isBootstrappingSave = true;
    private bool isSavingAccountGroup;
    public PatrolManager_v2 patrolManager;
    public TMP_InputField Inputfield;
    public List<Hero> roster = new List<Hero>();
    public EquipmentInventoryUI EIUI;
    [Header("Recursos")]
    public int Gold = 0;
    public int MemoryCristals = 0;
    public int AstralShards = 0;
    public string PlayerName = "";
    [Header("Valores Guardados Recursos")]
    public int LastGold = 0;
    public int LastMemoryCristals = 0;
    public int LastAstralShards = 0;
    public string LastPlayerName = "";

    [Header("Texto Recursos")]
    public TextMeshProUGUI GoldText;
    public TextMeshProUGUI MemoryCristalsText;
    public TextMeshProUGUI AstralShardsText;
    public TextMeshProUGUI PlayerNameText;
    [Header("Inventario del jugador")]
    public Inventory playerInventory;
    [Header("Items")]
    public List<Item_SO> allItems;
    [Header("Comidas")]
    public List<Food_SO> allFoods;
    [Header("Equipamiento")]
    public List<EquipmentInventoryEntry> equipmentInventory = new List<EquipmentInventoryEntry>();
    public List<EquipmentInstance> equippedInstances = new List<EquipmentInstance>();


    [Header("Equipamiento disponible")]
    public List<Equipment_SO> allEquipment;


    [Header("Monedas de héroe")]
    [Header("Catálogo de HeroCoins")]
    public List<HeroCoin_SO> heroCoinCatalog = new List<HeroCoin_SO>();
    [Header("Historial de gacha")]
    public List<GachaHistoryEntry> gachaHistory = new List<GachaHistoryEntry>();

    [System.Serializable]
    public class GachaHistoryEntry
    {
        public int heroID;
        public int count;
    }



    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }
    public void LoadEquipmentInventory()
    {
        equipmentInventory.Clear();

        int count = PlayerPrefs.GetInt("EquipmentCount", 0);

        for (int i = 0; i < count; i++)
        {
            string id = PlayerPrefs.GetString($"Equip_{i}_ID", "");
            string name = PlayerPrefs.GetString($"Equip_{i}_Name", "");
            int level = PlayerPrefs.GetInt($"Equip_{i}_Level", 1);
            int stars = PlayerPrefs.GetInt($"Equip_{i}_Stars", 0);
            int awakening = PlayerPrefs.GetInt($"Equip_{i}_Awakening", 0);

            var equipment = allEquipment.FirstOrDefault(eq => eq.itemName == name);
            if (equipment == null) continue;

            // 🔥 REPARACIÓN: si no tiene ID, generar uno nuevo
            if (string.IsNullOrEmpty(id))
            {
                id = System.Guid.NewGuid().ToString();
                PlayerPrefs.SetString($"Equip_{i}_ID", id);
            }

            EquipmentInstance inst = new EquipmentInstance(equipment);
            inst.uniqueID = id;
            inst.level = level;
            inst.stars = stars;
            inst.awakening = awakening;

            equipmentInventory.Add(new EquipmentInventoryEntry { instance = inst });
        }

        PlayerPrefs.Save(); // 🔥 Guardar IDs reparados
    }



    private void Start()
    {
        LoadData();
        LoadInventory(allItems);
        SaveInventory();
        LoadHeroCoins();
        LoadGachaHistory();
        LoadEquipmentInventory();
        LoadEquipment();
        foreach (var hero in roster)
            LoadHeroStars(hero);

        if (patrolManager != null)
            patrolManager.OnPatrolCompleted += HandlePatrolResult;

        // 🔥 1 PRIMERO: Que los héroes carguen su equipo
        foreach (var hero in roster)
            hero.LoadHeroHybrid();

        // (Se quitó la llamada a EIUI.RefreshUI() aquí — ese componente no se está usando.)

        // 🔥 3: Refrescar los paneles de "equipo puesto" de cada héroe con los datos recién cargados
        RefreshAllEquippedSlots();

        LoadAccountSaveFromGooglePlay();
        isBootstrappingSave = false;
    }


    private void Update()
    {
        UpdateMoney();


    }
    public void ChangePlayerName(TMP_InputField input)
    {
        input = Inputfield;

        PlayerName = input.text;
        PlayerNameText.text = PlayerName;

        SaveData(); // Guarda el nuevo nombre en PlayerPrefs
        Debug.Log($"Nuevo nombre del jugador: {PlayerName}");
    }
    [System.Serializable]
    public class HeroCoinInventoryEntry
    {
        public HeroCoin_SO coin;
        public int amount;
    }

    [Header("Inventario del jugador (HeroCoins)")]
    public List<HeroCoinInventoryEntry> heroCoinInventory = new List<HeroCoinInventoryEntry>();

    public HeroCoinInventoryEntry GetHeroCoin(int heroID)
    {
        // Buscar la moneda en el catálogo
        var coinSO = heroCoinCatalog.FirstOrDefault(c => c.heroID == heroID);
        if (coinSO == null)
        {
            Debug.LogError($"No existe HeroCoin_SO para heroID {heroID}");
            return null;
        }

        // Buscar en el inventario del jugador
        var entry = heroCoinInventory.FirstOrDefault(e => e.coin == coinSO);
        if (entry == null)
        {
            entry = new HeroCoinInventoryEntry { coin = coinSO, amount = 0 };
            heroCoinInventory.Add(entry);
        }

        return entry;
    }

    public void UpdateMoney()
    {

        if (LastGold != Gold)
        {
            GoldText.text = Gold.ToString();
            LastGold = Gold;
        }

        if (LastMemoryCristals != MemoryCristals)
        {
            MemoryCristalsText.text = MemoryCristals.ToString();
            LastMemoryCristals = MemoryCristals;
        }

        if (LastAstralShards != AstralShards)
        {
            AstralShardsText.text = AstralShards.ToString();
            LastAstralShards = AstralShards;
        }

        if (LastPlayerName != PlayerName)
        {
            PlayerNameText.text = PlayerName;
            LastPlayerName = PlayerName;
        }
    }
    public void AddHeroCoins(int heroID, int amount)
    {
        // Buscar la moneda en el catálogo
        var coinSO = heroCoinCatalog.FirstOrDefault(c => c.heroID == heroID);
        if (coinSO == null)
        {
            Debug.LogError($"No existe HeroCoin_SO para heroID {heroID}");
            return;
        }

        // Buscar en el inventario
        var entry = heroCoinInventory.FirstOrDefault(e => e.coin == coinSO);
        if (entry == null)
        {
            entry = new HeroCoinInventoryEntry { coin = coinSO, amount = 0 };
            heroCoinInventory.Add(entry);
        }

        entry.amount += amount;
        SaveData();

        Debug.Log($"Añadidas {amount} monedas de {coinSO.coinName}. Total: {entry.amount}");
    }
    public void SaveHeroCoins()
    {
        PlayerPrefs.SetInt("HeroCoinCount", heroCoinInventory.Count);

        for (int i = 0; i < heroCoinInventory.Count; i++)
        {
            var entry = heroCoinInventory[i];
            PlayerPrefs.SetInt($"HeroCoin_{i}_HeroID", entry.coin.heroID);
            PlayerPrefs.SetInt($"HeroCoin_{i}_Amount", entry.amount);
        }

        PlayerPrefs.Save();
        SyncAccountSaveToGooglePlay();
    }
    public void LoadHeroCoins()
    {
        heroCoinInventory.Clear();

        int count = PlayerPrefs.GetInt("HeroCoinCount", 0);

        for (int i = 0; i < count; i++)
        {
            int heroID = PlayerPrefs.GetInt($"HeroCoin_{i}_HeroID", -1);
            int amount = PlayerPrefs.GetInt($"HeroCoin_{i}_Amount", 0);

            if (heroID < 0) continue;

            // Buscar el ScriptableObject correspondiente
            var coinSO = heroCoinCatalog.FirstOrDefault(c => c.heroID == heroID);
            if (coinSO == null)
            {
                Debug.LogError($"No existe HeroCoin_SO para heroID {heroID}");
                continue;
            }

            heroCoinInventory.Add(new HeroCoinInventoryEntry
            {
                coin = coinSO,
                amount = amount
            });
        }
    }


    private void HandlePatrolResult(PatrolResult res, PatrolSlot slot)
    {
        Gold += res.goldEarned;
        SaveData();

        Debug.Log($"Patrulla completada: Oro +{res.goldEarned}, XP total +{res.xpEarned}");

        foreach (var l in res.log)
            Debug.Log(l);

        if (slot != null && slot.mission != null)
            Debug.Log($"Misión completada: {slot.mission.missionName}");
    }

    // -----------------------------------------
    //        GUARDAR Y CARGAR DATOS
    // -----------------------------------------
    public void RegisterGachaHero(int heroID)
    {
        var entry = gachaHistory.FirstOrDefault(e => e.heroID == heroID);

        if (entry == null)
        {
            entry = new GachaHistoryEntry { heroID = heroID, count = 1 };
            gachaHistory.Add(entry);
        }
        else
        {
            entry.count++;
        }

        SaveGachaHistory();
    }
    public void SaveGachaHistory()
    {
        PlayerPrefs.SetInt("GachaHistoryCount", gachaHistory.Count);

        for (int i = 0; i < gachaHistory.Count; i++)
        {
            PlayerPrefs.SetInt($"GachaHistory_{i}_HeroID", gachaHistory[i].heroID);
            PlayerPrefs.SetInt($"GachaHistory_{i}_Count", gachaHistory[i].count);
        }

        PlayerPrefs.Save();
        SyncAccountSaveToGooglePlay();
    }
    public void LoadGachaHistory()
    {
        gachaHistory.Clear();

        int count = PlayerPrefs.GetInt("GachaHistoryCount", 0);

        for (int i = 0; i < count; i++)
        {
            int heroID = PlayerPrefs.GetInt($"GachaHistory_{i}_HeroID", -1);
            int amount = PlayerPrefs.GetInt($"GachaHistory_{i}_Count", 0);

            if (heroID < 0) continue;

            gachaHistory.Add(new GachaHistoryEntry
            {
                heroID = heroID,
                count = amount
            });
        }
    }

    public void SaveInventory()
    {
        PlayerPrefs.DeleteKey("InventoryCount");
        PlayerPrefs.SetInt("InventoryCount", playerInventory.items.Count);

        for (int i = 0; i < playerInventory.items.Count; i++)
        {
            var entry = playerInventory.items[i];

            if (entry.item != null)
            {
                PlayerPrefs.SetString($"Inv_{i}_Type", "Item");
                PlayerPrefs.SetString($"Inv_{i}_Name", entry.item.itemName);
            }


            PlayerPrefs.SetInt($"Inv_{i}_Count", entry.count);
        }

        PlayerPrefs.Save();
        SyncAccountSaveToGooglePlay();
    }



    public void LoadInventory(List<Item_SO> allItems)
    {
        playerInventory.Clear();
        int count = PlayerPrefs.GetInt("InventoryCount", 0);

        for (int i = 0; i < count; i++)
        {
            string type = PlayerPrefs.GetString($"Inv_{i}_Type", "");
            string name = PlayerPrefs.GetString($"Inv_{i}_Name", "");
            int amount = PlayerPrefs.GetInt($"Inv_{i}_Count", 0);

            if (type == "Item")
            {
                var item = allItems.Find(it => it.itemName == name);
                if (item != null)
                    playerInventory.AddItem(item, amount);
            }

        }
    }
    public void RemoveOneFromInventory(Item_SO item)
    {
        if (item == null) return;

        // Buscar entrada en el inventario
        var entry = playerInventory.items.FirstOrDefault(e => e.item == item);

        if (entry == null)
        {
            Debug.LogWarning($"El item {item.itemName} no está en el inventario.");
            return;
        }

        // Restar uno
        entry.count--;

        // Si llega a 0, eliminarlo del inventario
        if (entry.count <= 0)
        {
            playerInventory.items.Remove(entry);
        }

        // Guardar cambios
        SaveInventory();
        Debug.Log($"Se ha consumido 1x {item.itemName}. Restante: {entry.count}");
    }



    public void SaveData()
    {
        PlayerPrefs.SetInt("Gold", Gold);
        PlayerPrefs.SetInt("MemoryCristals", MemoryCristals);
        PlayerPrefs.SetInt("AstralShards", AstralShards);
        PlayerPrefs.SetString("PlayerName", PlayerName);

        // Si quieres guardar héroes complejos, aquí va un JSON.
        // Por ahora solo guardo la cantidad.
        PlayerPrefs.SetInt("HeroCount", roster.Count);
        foreach (var hero in roster)
        {
            PlayerPrefs.SetInt($"HeroLevel_{hero.heroID}", hero.stats.level);
            PlayerPrefs.SetInt($"HeroStars_{hero.heroID}", hero.Stars);
        }
        PlayerPrefs.Save();
        bool wasSavingAccountGroup = isSavingAccountGroup;
        isSavingAccountGroup = true;
        SaveHeroCoins();
        SaveGachaHistory();
        isSavingAccountGroup = wasSavingAccountGroup;

        SyncAccountSaveToGooglePlay();

    }

    public void LoadData()
    {
        Gold = PlayerPrefs.GetInt("Gold", 0);
        MemoryCristals = PlayerPrefs.GetInt("MemoryCristals", 0);
        AstralShards = PlayerPrefs.GetInt("AstralShards", 0);
        PlayerName = PlayerPrefs.GetString("PlayerName", "");

        LastGold = Gold;
        LastMemoryCristals = MemoryCristals;
        LastAstralShards = AstralShards;

        GoldText.text = Gold.ToString();
        MemoryCristalsText.text = MemoryCristals.ToString();
        AstralShardsText.text = AstralShards.ToString();

        foreach (var hero in roster)
        {
            int savedLevel = PlayerPrefs.GetInt($"HeroLevel_{hero.heroID}", hero.stats.level);
            hero.stats.level = savedLevel;
            // Load persisted star count
            hero.Stars = PlayerPrefs.GetInt($"HeroStars_{hero.heroID}", hero.Stars);
            hero.UpdateCombatClass();
        }
    }


    // Utility: auto-assign available heroes up to count
    public List<Hero> AutoAssignHeroes(int count)
    {
        List<Hero> outList = new List<Hero>();
        foreach (var h in roster)
        {
            if (outList.Count >= count) break;
            if (h != null && !h.IsExhausted) outList.Add(h);
        }
        return outList;
    }
    public void SaveHeroStars(Hero hero)
    {
        // Use the same key as SaveData for consistency
        PlayerPrefs.SetInt($"HeroStars_{hero.heroID}", hero.Stars);
        PlayerPrefs.Save();
        SyncAccountSaveToGooglePlay();
    }

    public void LoadHeroStars(Hero hero)
    {
        hero.Stars = PlayerPrefs.GetInt($"HeroStars_{hero.heroID}", hero.Stars);
    }
    private void OnApplicationQuit()
    {
        isSavingAccountGroup = true;
        SaveData();
        SaveInventory();
        SaveHeroCoins();
        SaveGachaHistory();
        SaveEquipmentInventory();


        foreach (var hero in roster)
            SaveHeroStars(hero);
        isSavingAccountGroup = false;

        SyncAccountSaveToGooglePlay(force: true);

    }
    public void ADDEQUIPMENT(Equipment_SO equipment)
    {
        AddEquipment(equipment, 1);
        // (Se quitó la llamada a EIUI.RefreshUI() aquí — ese componente no se está usando.)
    }

    public static class EquipmentUIRefresher
    {
        public static void RefreshAll()
        {
            foreach (var slot in GameObject.FindObjectsOfType<EquipmentSlot>())
                slot.RefreshSlot();
        }
    }

    // 🔥 Refresca el/los EquippedEquipmentSlot (paneles de "equipo puesto") de un héroe.
    // Se necesita llamar a esto cada vez que se equipa algo, porque EquippedEquipmentSlot
    // solo se refresca solo en Start/OnEnable o cuando el propio Unequip() lo dispara.
    public void RefreshEquippedSlots(Hero hero)
    {
        if (hero == null) return;

        foreach (var slot in FindObjectsOfType<EquippedEquipmentSlot>())
        {
            if (slot.targetHero == hero)
                slot.RefreshSlot();
        }
    }

    // Refresca TODOS los EquippedEquipmentSlot de la escena. Se usa al arrancar,
    // porque el orden de Start() entre objetos no está garantizado en Unity:
    // el slot puede pintar su UI ANTES de que GameManager termine de cargar
    // el equipo de cada héroe desde PlayerPrefs, y entonces parece que "no se guardó".
    public void RefreshAllEquippedSlots()
    {
        foreach (var slot in FindObjectsOfType<EquippedEquipmentSlot>())
            slot.RefreshSlot();
    }

    public void AddEquipment(Equipment_SO equipment, int amount)
    {
        if (equipment == null) return;

        for (int i = 0; i < amount; i++)
        {
            EquipmentInstance newInstance = new EquipmentInstance(equipment);

            // 🔥 ESTA LÍNEA ES CLAVE: Sin ID, el héroe no encuentra nada al cargar
            newInstance.uniqueID = System.Guid.NewGuid().ToString();

            equipmentInventory.Add(new EquipmentInventoryEntry { instance = newInstance });
        }
        SaveEquipmentInventory();
    }
    public void SaveEquipmentInventory()
    {
        PlayerPrefs.SetInt("EquipmentCount", equipmentInventory.Count);

        for (int i = 0; i < equipmentInventory.Count; i++)
        {
            var inst = equipmentInventory[i].instance;

            PlayerPrefs.SetString($"Equip_{i}_ID", inst.uniqueID);
            PlayerPrefs.SetString($"Equip_{i}_Name", inst.baseData.itemName);
            PlayerPrefs.SetInt($"Equip_{i}_Level", inst.level);
            PlayerPrefs.SetInt($"Equip_{i}_Stars", inst.stars);
            PlayerPrefs.SetInt($"Equip_{i}_Awakening", inst.awakening);
        }

        PlayerPrefs.Save();
        SyncAccountSaveToGooglePlay();
    }

    private void OnApplicationPause(bool paused)
    {
        if (paused)
        {
            isSavingAccountGroup = true;
            SaveData();
            SaveEquipmentInventory();

            foreach (var hero in roster)
                hero.SaveHeroData_Prefs();
            isSavingAccountGroup = false;

            SyncAccountSaveToGooglePlay(force: true);
        }
    }
    public void SaveEquipment()
    {
        // Creamos el contenedor con los datos actuales [1]
        EquipmentSaveData dataToSave = new EquipmentSaveData();
        dataToSave.equippedInstances = this.equippedInstances;

        // Convertimos el objeto a una cadena de texto JSON [3]
        string json = JsonUtility.ToJson(dataToSave);

        // Guardamos la cadena en PlayerPrefs [2]
        PlayerPrefs.SetString("SavedEquipment", json);
        PlayerPrefs.Save();
    }

    public void LoadEquipment()
    {
        if (PlayerPrefs.HasKey("SavedEquipment"))
        {
            // Recuperamos la cadena de texto [2]
            string json = PlayerPrefs.GetString("SavedEquipment");

            // Reconvertimos el texto a nuestra clase contenedora [3]
            EquipmentSaveData loadedData = JsonUtility.FromJson<EquipmentSaveData>(json);

            // Asignamos la lista recuperada al GameManager [1]
            this.equippedInstances = loadedData.equippedInstances;
        }
    }

    public void SyncAccountSaveToGooglePlay(bool force = false)
    {
        if (isApplyingAccountCloudSave) return;
        if (isSavingAccountGroup && !force) return;
        if (isBootstrappingSave && !force) return;

        AccountCloudSave.Save(this);
    }

    private void LoadAccountSaveFromGooglePlay()
    {
        AccountCloudSave.Load(this, cloudSave =>
        {
            if (cloudSave == null)
            {
                SyncAccountSaveToGooglePlay(force: true);
                return;
            }

            long localTimestamp = AccountCloudSave.LocalTimestamp;

            if (cloudSave.saveTimestamp <= localTimestamp)
            {
                SyncAccountSaveToGooglePlay(force: true);
                return;
            }

            isApplyingAccountCloudSave = true;
            AccountCloudSave.Apply(this, cloudSave);
            SaveData();
            SaveInventory();
            SaveHeroCoins();
            SaveGachaHistory();
            SaveEquipmentInventory();

            foreach (var hero in roster)
            {
                if (hero == null) continue;
                hero.SaveHeroData_Prefs();
            }

            isApplyingAccountCloudSave = false;
            UpdateMoney();
            // (Se quitó la llamada a EIUI.RefreshUI() aquí — ese componente no se está usando.)
        });
    }



}