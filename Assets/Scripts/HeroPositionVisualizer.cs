using UnityEngine;

public class HeroPositionVisualizer : MonoBehaviour
{
    public Hero assignedHero;
    [Header("Índice de esta ranura (0-3)")]
    public int slotIndex;

    [Header("Lista de héroes posibles")]
    public Hero[] heroes;


    [Header("Visuales correspondientes (mismo orden que heroes[])")]
    public GameObject[] visuals;

    private HeroAssignmentManager manager;
    public GameObject unlockedObject;
    public GameObject lockedObject;

    private void Start()
    {
        manager = FindAnyObjectByType<HeroAssignmentManager>();
        RefreshVisual();
    }
    public void Update()
    {
        RefreshVisual();
    }
    public void RefreshVisual()
    {
         assignedHero = manager.GetHeroByIndex(slotIndex);

        // Si no hay héroe asignado → apagar todo
        if (assignedHero == null)
        {
            DisableAll();
            return;
        }

        // Buscar qué índice tiene este héroe en el array
        int heroIndex = GetHeroIndex(assignedHero);

        // Si no está en la lista → apagar todo
        if (heroIndex == -1)
        {
            DisableAll();
            return;
        }

        // Activar solo el visual correcto
        for (int i = 0; i < visuals.Length; i++)
        {
            visuals[i].SetActive(i == heroIndex);
        }
    }

    private int GetHeroIndex(Hero hero)
    {
        for (int i = 0; i < heroes.Length; i++)
        {
            if (heroes[i] == hero)
                return i;
        }
        return -1;
    }

    private void DisableAll()
    {
        foreach (var v in visuals)
            v.SetActive(false);
    }
}
