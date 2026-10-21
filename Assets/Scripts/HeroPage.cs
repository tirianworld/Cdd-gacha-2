using UnityEngine;
using TMPro;

public class HeroPage : MonoBehaviour
{
    public Hero hero;

    [Header("Caracteristicas")]
    public TextMeshProUGUI RazaText;
    public TextMeshProUGUI ReligionText;
    public TextMeshProUGUI ClaseText;

    [Header("Stats")]
    public TextMeshProUGUI CCText;
    public TextMeshProUGUI NivelText;
    public TextMeshProUGUI VidaText;
    public TextMeshProUGUI DañoText;
    public TextMeshProUGUI DefensaText;

    [Header("Estrellas")]
    public GameObject Estrella1;
    public GameObject Estrella2;
    public GameObject Estrella3;
    public GameObject Estrella4;
    public GameObject Estrella5;
    public GameObject Busto;

    [Header("Corazones (BondLevel)")]
    public GameObject Corazon1;
    public GameObject Corazon2;
    public GameObject Corazon3;
    public GameObject Corazon4;
    public GameObject Corazon5;

    void Start()
    {
        RefreshUI();
    }

    void Update()
    {
        RefreshUI();
    }

    void RefreshUI()
    {
        if (hero == null) return;

        // Características
        RazaText.text = hero.races.ToString();
        ReligionText.text = hero.heroReligion.ToString();
        ClaseText.text = string.Join(", ", hero.heroClasses);

        // Stats
        NivelText.text = hero.stats.level.ToString();
        VidaText.text = hero.stats.maxHealth.ToString();
        CCText.text = hero.CC.ToString();
        DañoText.text = hero.stats.attack.ToString();
        DefensaText.text = hero.stats.defense.ToString();

        // Estrellas

        // Corazones (BondLevel)
        // SetCorazones(hero.BondLevel);
    }



    // void SetCorazones(int bond)
    // {
    // Corazon1.SetActive(bond >= 1);
    //   Corazon2.SetActive(bond >= 2);
    // Corazon3.SetActive(bond >= 3);
    //   Corazon4.SetActive(bond >= 4);
    // Corazon5.SetActive(bond >= 5);
    //}
}
