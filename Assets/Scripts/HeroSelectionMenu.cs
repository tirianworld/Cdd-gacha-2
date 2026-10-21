using UnityEngine;
using UnityEngine.UI;
using TMPro;


public class HeroSelectionMenu : MonoBehaviour
{
    public Transform heroGridParent;
    public GameObject heroButtonPrefab;
    private PatrolFormationUI currentFormation;

    public void OpenMenu(PatrolFormationUI formation)
    {
        currentFormation = formation;
        gameObject.SetActive(true);
        PopulateGrid();
    }

    void PopulateGrid()
    {
        foreach (Transform t in heroGridParent)
            if (t.gameObject.activeSelf) DestroyImmediate(t.gameObject);

        foreach (var hero in currentFormation.patrolManager.roster)
        {
            if (hero.GetComponent<DispatchedMarker>() != null) continue;

            var btnObj = Object.Instantiate(heroButtonPrefab, heroGridParent);
            btnObj.SetActive(true);
            btnObj.GetComponentInChildren<TextMeshProUGUI>().text = hero.heroName;

            var btn = btnObj.GetComponent<Button>();
            btn.onClick.AddListener(() => SelectHero(hero));
        }
    }

    void SelectHero(Hero hero)
    {
        currentFormation.AssignHero(hero);
        gameObject.SetActive(false);
    }
}
