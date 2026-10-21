using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class HeroAssignmentManager : MonoBehaviour
{
    [Header("Héroes asignados (máx 4)")]
    public Hero Hero1;
    public Hero Hero2;
    public Hero Hero3;
    public Hero Hero4;

    [Header("GameObjects visuales de héroes")]
    public HeroVisual[] heroVisuals;

    [Header("Referencia al CombatManager")]
    public CombatManager combat;

    private const string HeroKeyPrefix = "AssignedHero_";
    private int currentHeroIndex = 0;

    private IEnumerator Start()
    {
        // Espera a que el GameManager cargue el roster
        yield return new WaitUntil(() => GameManager.Instance != null && GameManager.Instance.roster.Count > 0);

        LoadAssignments();
        ActivateHeroByIndex(currentHeroIndex);
    }

    // ============================================================
    // ASIGNACIÓN DE HÉROES
    // ============================================================
    public void AssignOrSwapHero(int targetSlot, Hero hero)
    {
        Hero heroInTarget = GetHeroByIndex(targetSlot);

        // ============================
        // CASO ESPECIAL: DESASIGNAR
        // ============================
        if (hero == null)
        {
            SetHeroReference(targetSlot, null);
            SaveSlot(targetSlot, null);
            Debug.Log($"Slot {targetSlot + 1} vaciado.");
            return;
        }



        // 2) Ver si el héroe ya está asignado en otro slot
        int currentSlot = GetSlotOfHero(hero);

        // ============================
        // CASO A: El slot destino está vacío
        // ============================
        if (heroInTarget == null)
        {
            if (currentSlot != -1)
                SetHeroReference(currentSlot, null);

            SetHeroReference(targetSlot, hero);
            SaveSlot(targetSlot, hero);

            Debug.Log($"Movido {hero.heroName} al slot {targetSlot + 1}");
            return;
        }

        // ============================
        // CASO B: INTERCAMBIO
        // ============================
        Hero otherHero = heroInTarget;

        if (currentSlot != -1)
            SetHeroReference(currentSlot, otherHero);

        SetHeroReference(targetSlot, hero);

        SaveSlot(targetSlot, hero);
        if (currentSlot != -1)
            SaveSlot(currentSlot, otherHero);

        Debug.Log($"Intercambiados {hero.heroName} ↔ {otherHero.heroName}");
    }


    private void SaveSlot(int slot, Hero hero)
    {
        if (hero == null)
            PlayerPrefs.DeleteKey(HeroKeyPrefix + slot);
        else
            PlayerPrefs.SetInt(HeroKeyPrefix + slot, hero.heroID);

        PlayerPrefs.Save();
    }

    public int GetSlotOfHero(Hero hero)
    {
        if (Hero1 == hero) return 0;
        if (Hero2 == hero) return 1;
        if (Hero3 == hero) return 2;
        if (Hero4 == hero) return 3;
        return -1;
    }



    public void AssignHero(int slotIndex, Hero hero)
    {
        if (slotIndex < 0 || slotIndex >= 4)
        {
            Debug.LogError("Slot inválido. Debe estar entre 0 y 3.");
            return;
        }

        if (hero == null)
        {
            Debug.LogError("Héroe nulo, no se puede asignar.");
            return;
        }

        if (hero.GetComponent<DispatchedMarker>() != null)
        {
            Debug.LogWarning($"{hero.heroName} está en misión y no puede asignarse.");
            return;
        }

        if (Hero1 == hero || Hero2 == hero || Hero3 == hero || Hero4 == hero)
        {
            Debug.LogWarning($"{hero.heroName} ya está asignado.");
            return;
        }

        PlayerPrefs.SetInt(HeroKeyPrefix + slotIndex, hero.heroID);
        PlayerPrefs.Save();

        SetHeroReference(slotIndex, hero);

        EquipmentManager.Instance.LoadHeroEquipment(hero);

        SyncHeroPositions();
        Debug.Log($"Héroe {hero.heroName} asignado al slot {slotIndex + 1}.");
    }

    public void AssignHeroToSlot(Hero hero, int slotNumber)
    {
        AssignHero(slotNumber - 1, hero);
    }

    public Hero GetAssignedHero(int slotIndex)
    {
        int heroID = PlayerPrefs.GetInt(HeroKeyPrefix + slotIndex, -1);
        if (heroID == -1) return null;

        return GameManager.Instance.roster.FirstOrDefault(h => h.heroID == heroID);
    }

    public List<Hero> GetAllAssignedHeroes()
    {
        List<Hero> assigned = new List<Hero>();
        for (int i = 0; i < 4; i++)
        {
            Hero h = GetAssignedHero(i);
            if (h != null)
                assigned.Add(h);
        }
        return assigned;
    }

    public void ClearAssignments()
    {
        for (int i = 0; i < 4; i++)
            PlayerPrefs.DeleteKey(HeroKeyPrefix + i);

        PlayerPrefs.Save();

        Hero1 = null;
        Hero2 = null;
        Hero3 = null;
        Hero4 = null;

        Debug.Log("Asignaciones limpiadas.");
    }

    private void LoadAssignments()
    {
        for (int i = 0; i < 4; i++)
        {
            Hero hero = GetAssignedHero(i);
            SetHeroReference(i, hero);

            if (hero != null)
                EquipmentManager.Instance.LoadHeroEquipment(hero);
        }
    }

    private void SetHeroReference(int slotIndex, Hero hero)
    {
        switch (slotIndex)
        {
            case 0: Hero1 = hero; break;
            case 1: Hero2 = hero; break;
            case 2: Hero3 = hero; break;
            case 3: Hero4 = hero; break;
        }
    }

    public void SyncHeroPositions()
    {
        if (Hero1 != null) Hero1.position = 0;
        if (Hero2 != null) Hero2.position = 1;
        if (Hero3 != null) Hero3.position = 2;
        if (Hero4 != null) Hero4.position = 3;
    }

    // ============================================================
    // ACTIVAR VISUAL DEL HÉROE
    // ============================================================

    private void ActivateHeroVisual(Hero hero)
    {
        foreach (var hv in heroVisuals)
        {
            if (hv == null) continue;

            bool isActive = hv.hero == hero;
            hv.gameObject.SetActive(isActive);
        }
    }

    // ============================================================
    // CAMBIO DE HÉROE EN COMBATE
    // ============================================================

    public void ActivateHeroByIndex(int index)
    {
        Hero activeHero = GetHeroByIndex(index);
        if (activeHero == null)
        {
            Debug.LogWarning("No hay héroe en este slot.");
            return;
        }

        if (activeHero.stats.currentHealth <= 0)
        {
            Debug.LogWarning($"No se puede cambiar a {activeHero.heroName} porque está muerto.");
            return;
        }

        currentHeroIndex = index;

        ActivateHeroVisual(activeHero);

        combat.hero = activeHero;
        combat.handManager.hero = activeHero;
        combat.RefreshHeroStatusUI();

        combat.heroCombatSprite.sprite = activeHero.combatSprite != null
            ? activeHero.combatSprite
            : activeHero.portrait;

        combat.handManager.actionsLeft = 1;
        combat.handManager.StartTurn();
        combat.handManager.RefreshHandUI();

        Debug.Log($"Héroe activo ahora: {activeHero.heroName}");
    }

    public Hero GetHeroByIndex(int index)
    {
        switch (index)
        {
            case 0: return Hero1;
            case 1: return Hero2;
            case 2: return Hero3;
            case 3: return Hero4;
        }
        return null;
    }

    // ============================================================
    // CAMBIO AUTOMÁTICO AL MORIR
    // ============================================================

    public void OnHeroDeath()
    {
        int nextIndex = -1;
        for (int i = 0; i < 4; i++)
        {
            Hero h = GetHeroByIndex(i);
            if (h != null && h.stats.currentHealth > 0)
            {
                nextIndex = i;
                break;
            }
        }

        if (nextIndex == -1)
        {
            Debug.Log("No quedan héroes. GAME OVER.");
            return;
        }

        ActivateHeroByIndex(nextIndex);
    }

    private Hero GetNextAvailableHero()
    {
        for (int i = 0; i < 4; i++)
        {
            Hero h = GetHeroByIndex(i);
            if (h != null && h.stats.currentHealth > 0 && h != ActiveHero)
                return h;
        }
        return null;
    }

    // ============================================================
    // CAMBIO MANUAL
    // ============================================================

    public void SwitchToNextHero()
    {
        int nextIndex = -1;
        // Buscar el siguiente héroe vivo con índice mayor que el actual
        for (int i = currentHeroIndex + 1; i < 4; i++)
        {
            Hero h = GetHeroByIndex(i);
            if (h != null && h.stats.currentHealth > 0)
            {
                nextIndex = i;
                break;
            }
        }
        // Si no hay ninguno después, buscar desde el principio
        if (nextIndex == -1)
        {
            for (int i = 0; i < currentHeroIndex; i++)
            {
                Hero h = GetHeroByIndex(i);
                if (h != null && h.stats.currentHealth > 0)
                {
                    nextIndex = i;
                    break;
                }
            }
        }

        if (nextIndex == -1)
        {
            Debug.Log("No hay más héroes disponibles.");
            return;
        }

        ActivateHeroByIndex(nextIndex);
    }

    public Hero ActiveHero => GetHeroByIndex(currentHeroIndex);

    // ============================================================
    // CAMBIO DE HÉROE DESDE HeroSwitcher (fuera del flujo normal de slots)
    // ============================================================

    /// <summary>
    /// Reemplaza en el roster (Hero1..Hero4) al héroe que estaba peleando
    /// por uno nuevo, manteniendo el mismo slot. Esto es lo que faltaba para
    /// que HeroSwitcher no dejara el roster desincronizado: sin esto,
    /// ResetAllHeroesOnCombatEnd() del CombatManager (que recorre
    /// GetHeroByIndex(i)) seguía viendo al héroe viejo en ese slot y el héroe
    /// nuevo podía quedar con su vida sin resetear para el próximo combate.
    /// </summary>
    public void ReplaceHeroInCombat(Hero oldHero, Hero newHero)
    {
        if (newHero == null)
        {
            Debug.LogWarning("HeroAssignmentManager: ReplaceHeroInCombat llamado con newHero nulo.");
            return;
        }

        int slot = oldHero != null ? GetSlotOfHero(oldHero) : -1;

        // Si el héroe anterior no estaba en ningún slot conocido (p.ej. era null
        // o ya se había perdido la referencia), usamos el slot activo actual.
        if (slot == -1)
            slot = currentHeroIndex;

        SetHeroReference(slot, newHero);
        SaveSlot(slot, newHero);

        currentHeroIndex = slot;

        Debug.Log($"[HeroAssignmentManager] Slot {slot + 1} actualizado a {newHero.heroName} tras cambio en combate.");
    }

    // ============================================================
    // GUARDAR EQUIPO
    // ============================================================

    public void SaveAllAssignedHeroes()
    {
        List<Hero> heroes = GetAllAssignedHeroes();

        foreach (var hero in heroes)
        {
            if (hero != null)
            {
                EquipmentManager.Instance.SaveAccountEquipment();
                EquipmentManager.Instance.LoadHeroEquipment(hero);
            }
        }

        PlayerPrefs.Save();
        Debug.Log("Se han guardado todos los héroes asignados con su equipo.");
    }
}