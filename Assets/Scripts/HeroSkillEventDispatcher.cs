using UnityEngine;
using UnityEngine.Events;

[System.Serializable]
public class TieredSkillEvents
{
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
            default: tier1?.Invoke(); break;
        }
    }
}

public class HeroSkillEventDispatcher : MonoBehaviour
{
    public Hero hero;                     // Héroe dueño de este dispatcher
    public TieredSkillEvents[] skillEvents;

    public CombatManager combatManager;
    public Hero activeCombatHero;

    private void Awake()
    {
        ResolveHeroReference();
        combatManager = FindObjectOfType<CombatManager>();
    }

    private void Update()
    {
        if (combatManager != null && combatManager.hero != null)
        {
            activeCombatHero = combatManager.hero;

            // Activar solo el dispatcher del héroe activo
            this.enabled = (hero == activeCombatHero);
        }
    }


    public void ResolveHeroReference()
    {
        if (hero == null)
            hero = GetComponent<Hero>();

        // 🔧 FIX: antes solo miraba en el propio GameObject. Si el
        // Hero está en un padre o hijo (jerarquía típica de un
        // prefab de héroe con HeroVisual + dispatcher separados),
        // esto se quedaba en null para siempre.
        if (hero == null)
            hero = GetComponentInParent<Hero>();

        if (hero == null)
            hero = GetComponentInChildren<Hero>(true);

        // ⚠️ FIX: este fallback es peligroso con varios héroes en
        // escena porque ActiveHero puede no coincidir con el héroe
        // real de ESTE dispatcher. Se deja como último recurso pero
        // ahora se loguea para que sea visible en consola.
        if (hero == null)
        {
            var assign = FindObjectOfType<HeroAssignmentManager>();
            if (assign != null)
            {
                hero = assign.ActiveHero;
                Debug.LogWarning($"[HeroSkillEventDispatcher] '{name}' no tenía Hero asignado; " +
                                  $"se usó el fallback ActiveHero ({hero?.heroName ?? "null"}). " +
                                  $"Verifica que este dispatcher tenga su propio Hero correctamente enlazado.");
            }
        }

        if (hero == null)
        {
            Debug.LogError($"[HeroSkillEventDispatcher] '{name}' no pudo resolver ninguna referencia a Hero. " +
                            $"Las skills de este héroe NO se ejecutarán.");
        }
    }

    public void RegenerateEvents()
    {
        if (hero == null || hero.skills == null) return;

        int count = hero.skills.Count;
        skillEvents = new TieredSkillEvents[count];

        for (int i = 0; i < count; i++)
            skillEvents[i] = new TieredSkillEvents();
    }

    // ============================================================
    // 🔥 EJECUCIÓN DE SKILLS
    // ============================================================
    public void TriggerSkillEvent(int index)
    {
        TriggerSkillEvent(index, 1);
    }

    public void TriggerSkillEvent(int index, int tier)
    {
        // 🔧 FIX: antes este return era completamente silencioso.
        // Si RegenerateEvents() nunca se llamó (o hero.skills.Count
        // cambió después), el evento se perdía sin ningún rastro.
        if (index < 0 || skillEvents == null || index >= skillEvents.Length)
        {
            Debug.LogError($"[HeroSkillEventDispatcher] '{name}' TriggerSkillEvent({index}) fuera de rango. " +
                            $"skillEvents.Length={(skillEvents == null ? "null" : skillEvents.Length.ToString())}. " +
                            $"¿Falta llamar a RegenerateEvents() para este héroe?");
            return;
        }

        // ❗ Validación principal: SOLO el héroe activo puede ejecutar skills
        if (combatManager != null && hero != combatManager.hero)
        {
            Debug.LogWarning($"[HeroSkillEventDispatcher] '{name}' evento ignorado: " +
                              $"hero de este dispatcher ({hero?.heroName ?? "null"}) != " +
                              $"combatManager.hero ({combatManager.hero?.heroName ?? "null"}).");
            return;
        }

        skillEvents[index].InvokeTier(tier);
    }

    private void OnValidate()
    {
        ResolveHeroReference();
    }
}