using UnityEngine;

public class CombatPortraitManager : MonoBehaviour
{
    [System.Serializable]
    public class HeroPortraitEntry
    {
        public Hero hero;              // Héroe asociado
        public GameObject portraitObj; // GameObject del retrato
    }

    [Header("Referencias")]
    public CombatManager combatManager;

    [Header("Retratos por héroe")]
    public HeroPortraitEntry[] portraits;

    private void Start()
    {
        UpdatePortraits();
    }

    private void Update()
    {
        // Opcional: si quieres que cambie dinámicamente en runtime
        UpdatePortraits();
    }

    public void UpdatePortraits()
    {
        if (combatManager == null || combatManager.hero == null)
        {
            Debug.LogWarning("CombatPortraitManager: CombatManager o hero no asignado.");
            return;
        }

        Hero activeHero = combatManager.hero;

        foreach (var entry in portraits)
        {
            if (entry.portraitObj == null) continue;

            bool match = entry.hero == activeHero;
            entry.portraitObj.SetActive(match);
        }
    }
}
