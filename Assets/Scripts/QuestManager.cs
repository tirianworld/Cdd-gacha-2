using System;
using UnityEngine;
using UnityEngine.Events;


public class QuestManager : MonoBehaviour
{
    [Header("Quests")]
    public QuestEntry[] quests;

    public void CompleteQuest(string questName)
    {
        QuestEntry quest = FindQuest(questName);
        if (quest == null) return;

        if (quest.bloqueada)
        {
            Debug.LogWarning($"Quest bloqueada: {questName}");
            return;
        }

        if (quest.questCompletada) return;

        quest.questCompletada = true;
        quest.onQuestCompleted?.Invoke();
    }

    public void SetQuestLocked(string questName, bool locked)
    {
        QuestEntry quest = FindQuest(questName);
        if (quest == null) return;

        quest.bloqueada = locked;
    }

    public void LockQuest(string questName)
    {
        SetQuestLocked(questName, true);
    }

    public void UnlockQuest(string questName)
    {
        SetQuestLocked(questName, false);
    }

    public void ResetQuest(string questName)
    {
        QuestEntry quest = FindQuest(questName);
        if (quest == null) return;

        quest.questCompletada = false;
    }

    public bool IsQuestCompleted(string questName)
    {
        QuestEntry quest = FindQuest(questName);
        return quest != null && quest.questCompletada;
    }

    public bool IsQuestLocked(string questName)
    {
        QuestEntry quest = FindQuest(questName);
        return quest == null || quest.bloqueada;
    }

    private QuestEntry FindQuest(string questName)
    {
        if (quests == null || string.IsNullOrWhiteSpace(questName))
        {
            Debug.LogWarning("QuestManager: nombre de quest vacio o lista sin configurar.");
            return null;
        }

        foreach (QuestEntry quest in quests)
        {
            if (quest != null && quest.questName == questName)
                return quest;
        }

        Debug.LogWarning($"QuestManager: no existe una quest llamada '{questName}'.");
        return null;
    }
}

[Serializable]
public class QuestEntry
{
    [Header("Datos de la quest")]
    public string questName;
    public bool questCompletada;
    public bool bloqueada = true;

    [Header("Evento al completarse")]
    public UnityEvent onQuestCompleted;
}
