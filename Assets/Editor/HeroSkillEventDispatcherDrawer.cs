using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

[System.Serializable]


public class HeroSkillEventDispatcher : MonoBehaviour
{
    public Hero hero;

    [Tooltip("Eventos de Unity únicos para este objeto.")]
    public TieredSkillEvents[] onSkillExecutedEvents;

    private Dictionary<UnityEvent, UnityAction> trackedListeners = new Dictionary<UnityEvent, UnityAction>();

    private Hero lastAssignedHero = null;

    private void Awake()
    {
        ResolveHeroReference();
        EnsureEventsMatchHero();
    }

    private void Start()
    {
        BindEvents();
    }

    private void ResolveHeroReference()
    {
        if (hero == null) hero = GetComponent<Hero>();

        if (hero == null)
        {
            var assign = FindObjectOfType<HeroAssignmentManager>();
            if (assign != null) hero = assign.ActiveHero;
        }
    }

    private void EnsureEventsMatchHero()
    {
        if (hero == null || hero.skills == null) return;

        // Solo regenerar si el héroe ha cambiado
        if (lastAssignedHero != hero)
        {
            InitializeEvents(hero.skills.Count);
            lastAssignedHero = hero;
        }
    }

    public void BindEvents()
    {
        if (hero == null || hero.skills == null) return;

        UnbindEvents();

        for (int i = 0; i < hero.skills.Count; i++)
        {
            int index = i;
            UnityEvent skillEvent = hero.skills[i].OnSkillExecuted;

            UnityAction action = () => TriggerSkillEvent(index);

            trackedListeners[skillEvent] = action;

            skillEvent.AddListener(action);
        }
    }

    private void UnbindEvents()
    {
        foreach (var kvp in trackedListeners)
        {
            if (kvp.Key != null)
                kvp.Key.RemoveListener(kvp.Value);
        }
        trackedListeners.Clear();
    }

    private void OnDestroy()
    {
        UnbindEvents();
    }

    private void InitializeEvents(int count)
    {
        onSkillExecutedEvents = new TieredSkillEvents[count];

        for (int i = 0; i < count; i++)
        {
            onSkillExecutedEvents[i] = new TieredSkillEvents();
        }
    }

    public void TriggerSkillEvent(int index)
    {
        if (index < 0 || onSkillExecutedEvents == null || index >= onSkillExecutedEvents.Length) return;

        onSkillExecutedEvents[index]?.InvokeTier(1);
        Debug.Log($"[{gameObject.name}] Evento ejecutado para {hero.heroName} - Skill Index: {index}");
    }

    public void TriggerSkillEvent(int index, int tier)
    {
        if (index < 0 || onSkillExecutedEvents == null || index >= onSkillExecutedEvents.Length) return;

        onSkillExecutedEvents[index]?.InvokeTier(tier);
        Debug.Log($"[{gameObject.name}] Evento ejecutado para {hero.heroName} - Skill Index: {index} (Tier {tier})");
    }

    private void OnValidate()
    {
        ResolveHeroReference();
        EnsureEventsMatchHero();
    }
}
