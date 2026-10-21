using UnityEngine;
using UnityEngine.Events;
using System.Linq;

[System.Serializable]
public class EquipmentSkillEvent
{
    public Equipment_SO equipment;   // Equipo que otorga la skill
    public UnityEvent tier1 = new UnityEvent();
    public UnityEvent tier2 = new UnityEvent();
    public UnityEvent tier3 = new UnityEvent();

    public void InvokeTier(int tier)
    {
        switch (tier)
        {
            case 1: tier1?.Invoke(); break;
            case 2: tier2?.Invoke(); break;
            case 3: tier3?.Invoke(); break;
        }
    }
}

public class EquipmentSkillEventDispatcher : MonoBehaviour
{
    public Hero hero;

    [Tooltip("Lista manual de skills de equipo con sus eventos.")]
    public EquipmentSkillEvent[] equipmentSkillEvents;

    public CombatManager combatManager;
    public Hero activeCombatHero;

    private void Awake()
    {
        if (hero == null)
            hero = GetComponent<Hero>();

        combatManager = FindObjectOfType<CombatManager>();
    }

    private void Update()
    {
        if (combatManager != null && combatManager.hero != null)
            activeCombatHero = combatManager.hero;
    }

    // ============================================================
    // 🔥 MÉTODO QUE DEBE LLAMARSE CUANDO SE USA UNA SKILL
    // ============================================================
    public void OnHeroUsedSkill(HeroSkill usedSkill, int tier)
    {
        if (hero != activeCombatHero)
            return;

        foreach (var evt in equipmentSkillEvents)
        {
            if (evt.equipment == null) continue;
            if (evt.equipment.grantedHeroSkill == null) continue;

            // Coincide la skill usada con la skill del equipo
            if (evt.equipment.grantedHeroSkill.skillName == usedSkill.skillName)
            {
                // ✔ AHORA comprobamos si el héroe tiene equipada una instancia de ese equipo
                bool isEquipped = hero.equippedInstances
                    .Any(inst => inst.baseData == evt.equipment);

                if (isEquipped)
                {
                    evt.InvokeTier(tier);
                    Debug.Log($"[EquipmentSkillEventDispatcher] Ejecutado evento de {evt.equipment.name}, tier {tier}");
                }
            }
        }
    }
}
