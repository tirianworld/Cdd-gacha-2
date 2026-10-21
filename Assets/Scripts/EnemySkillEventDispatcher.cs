using UnityEngine;
using UnityEngine.Events;
using System.Collections.Generic;

public class EnemySkillEventDispatcher : MonoBehaviour
{
    public Enemy enemy;

    [Tooltip("Estos eventos se sincronizan automáticamente con las HeroSkills del enemigo.")]
    public List<UnityEvent> onEnemySkillExecutedEvents = new List<UnityEvent>();


    private void OnValidate()
    {
        SyncEventsWithSkills();
    }


    private void Start()
    {
        if (enemy == null) return;

        // Registrar listeners por skill
        for (int i = 0; i < enemy.heroSkills.Count; i++)
        {
            int index = i;
            enemy.heroSkills[i].OnSkillExecuted.AddListener(() => TriggerSkillEvent(index));
        }
    }


    private void TriggerSkillEvent(int index)
    {
        if (index < 0 || index >= onEnemySkillExecutedEvents.Count)
            return;

        onEnemySkillExecutedEvents[index]?.Invoke();
    }


    public void SyncEventsWithSkills()
    {
        if (enemy == null || enemy.heroSkills == null)
            return;

        int skillCount = enemy.heroSkills.Count;

        // Añadir eventos faltantes
        while (onEnemySkillExecutedEvents.Count < skillCount)
            onEnemySkillExecutedEvents.Add(new UnityEvent());

        // Eliminar eventos sobrantes
        while (onEnemySkillExecutedEvents.Count > skillCount)
            onEnemySkillExecutedEvents.RemoveAt(onEnemySkillExecutedEvents.Count - 1);
    }
}
