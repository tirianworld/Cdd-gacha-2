using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class EquipmentManager : MonoBehaviour
{
    public static EquipmentManager Instance;

    [Header("Inventario global (instancias individuales)")]
    public List<EquipmentInstance> accountEquipment = new List<EquipmentInstance>();

    private const string AccountKey = "AccountEquipment_";
    private const string HeroEquipKeyPrefix = "HeroEquip_";

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // ============================================================
    // INVENTARIO GLOBAL (INSTANCIAS)
    // ============================================================

    public void AddToAccount(EquipmentInstance inst)
    {
        if (inst == null) return;
        accountEquipment.Add(inst);
        SaveAccountEquipment();
    }

    public void SaveAccountEquipment()
    {
        PlayerPrefs.SetInt("AccountEquipmentCount", accountEquipment.Count);

        for (int i = 0; i < accountEquipment.Count; i++)
        {
            var inst = accountEquipment[i];

            PlayerPrefs.SetString($"{AccountKey}{i}_Name", inst.baseData.itemName);
            PlayerPrefs.SetInt($"{AccountKey}{i}_Level", inst.level);
            PlayerPrefs.SetInt($"{AccountKey}{i}_Stars", inst.stars);
            PlayerPrefs.SetInt($"{AccountKey}{i}_Awakening", inst.awakening);
        }

        PlayerPrefs.Save();
    }

    public void LoadAccountEquipment()
    {
        accountEquipment.Clear();

        int count = PlayerPrefs.GetInt("AccountEquipmentCount", 0);

        for (int i = 0; i < count; i++)
        {
            string name = PlayerPrefs.GetString($"{AccountKey}{i}_Name", "");
            int level = PlayerPrefs.GetInt($"{AccountKey}{i}_Level", 1);
            int stars = PlayerPrefs.GetInt($"{AccountKey}{i}_Stars", 0);
            int awakening = PlayerPrefs.GetInt($"{AccountKey}{i}_Awakening", 0);

            Equipment_SO baseEq = Resources.Load<Equipment_SO>($"Equipment/{name}");
            if (baseEq == null) continue;

            EquipmentInstance inst = new EquipmentInstance(baseEq);
            inst.level = level;
            inst.stars = stars;
            inst.awakening = awakening;

            accountEquipment.Add(inst);
        }
    }

    // ============================================================
    // EQUIPO POR HÉROE (INSTANCIAS)
    // ============================================================

    public void EquipHero(Hero hero, EquipmentInstance inst)
    {
        if (hero == null || inst == null) return;

        hero.Equip(inst);

        string key = $"{HeroEquipKeyPrefix}{hero.heroID}_{inst.baseData.type}";

        PlayerPrefs.SetString($"{key}_Name", inst.baseData.itemName);
        PlayerPrefs.SetInt($"{key}_Level", inst.level);
        PlayerPrefs.SetInt($"{key}_Stars", inst.stars);
        PlayerPrefs.SetInt($"{key}_Awakening", inst.awakening);

        PlayerPrefs.Save();
    }

    public void UnequipHero(Hero hero, Equipment_SO.Type type)
    {
        string key = $"{HeroEquipKeyPrefix}{hero.heroID}_{type}";

        string name = PlayerPrefs.GetString($"{key}_Name", "");
        if (string.IsNullOrEmpty(name)) return;

        Equipment_SO baseEq = Resources.Load<Equipment_SO>($"Equipment/{name}");
        if (baseEq == null) return;

        EquipmentInstance inst = hero.equippedInstances
            .FirstOrDefault(i => i.baseData == baseEq);

        if (inst != null)
            hero.Unequip(inst);

        PlayerPrefs.DeleteKey($"{key}_Name");
        PlayerPrefs.DeleteKey($"{key}_Level");
        PlayerPrefs.DeleteKey($"{key}_Stars");
        PlayerPrefs.DeleteKey($"{key}_Awakening");

        PlayerPrefs.Save();
    }

    public EquipmentInstance GetHeroEquipment(Hero hero, Equipment_SO.Type type)
    {
        string key = $"{HeroEquipKeyPrefix}{hero.heroID}_{type}";

        string name = PlayerPrefs.GetString($"{key}_Name", "");
        if (string.IsNullOrEmpty(name)) return null;

        int level = PlayerPrefs.GetInt($"{key}_Level", 1);
        int stars = PlayerPrefs.GetInt($"{key}_Stars", 0);
        int awakening = PlayerPrefs.GetInt($"{key}_Awakening", 0);

        Equipment_SO baseEq = Resources.Load<Equipment_SO>($"Equipment/{name}");
        if (baseEq == null) return null;

        EquipmentInstance inst = new EquipmentInstance(baseEq);
        inst.level = level;
        inst.stars = stars;
        inst.awakening = awakening;

        return inst;
    }

    public void LoadHeroEquipment(Hero hero)
    {
        hero.equippedInstances.Clear();

        foreach (Equipment_SO.Type type in System.Enum.GetValues(typeof(Equipment_SO.Type)))
        {
            EquipmentInstance inst = GetHeroEquipment(hero, type);
            if (inst != null)
                hero.Equip(inst);
        }
    }

    // ============================================================
    // GUARDAR EQUIPO DE TODOS LOS HÉROES ASIGNADOS
    // ============================================================

    public void SaveAllAssignedHeroes(HeroAssignmentManager ham)
    {
        List<Hero> heroes = ham.GetAllAssignedHeroes();

        foreach (var hero in heroes)
        {
            foreach (var inst in hero.equippedInstances)
            {
                EquipHero(hero, inst);
            }
        }

        PlayerPrefs.Save();
    }
}
