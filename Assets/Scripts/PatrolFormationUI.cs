using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class PatrolFormationUI : MonoBehaviour
{
    public Button startButton;
    public Button[] heroSlots;
    public PatrolManager_v2 patrolManager;
    public PatrolMission_SO currentMission;
    public HeroSelectionMenu heroMenu;

    private Hero[] assignedHeroes = new Hero[4];
    private int currentSlotIndex = -1;

    void Start()
    {
        for (int i = 0; i < heroSlots.Length; i++)
        {
            int index = i;
            heroSlots[i].onClick.AddListener(() => OnSlotClicked(index));
        }
        if (startButton != null)
            startButton.onClick.AddListener(StartPatrol);
    }

    void OnSlotClicked(int slotIndex)
    {
        currentSlotIndex = slotIndex;
        if (heroMenu != null)
            heroMenu.OpenMenu(this);
    }

    public void AssignHero(Hero hero)
    {
        if (currentSlotIndex < 0) return;
        assignedHeroes[currentSlotIndex] = hero;
        heroSlots[currentSlotIndex].GetComponentInChildren<TextMeshProUGUI>().text = hero.heroName;
    }

    void StartPatrol()
    {
        var selected = new List<Hero>();
        foreach (var h in assignedHeroes)
            if (h != null) selected.Add(h);

        if (selected.Count == 0) return;

        patrolManager.DispatchToMission(currentMission, selected, false);
        Debug.Log($"Patrulla iniciada con {selected.Count} héroes.");
    }
}
