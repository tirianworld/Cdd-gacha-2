using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;
using TMPro;

public class HeroGridSorter : MonoBehaviour
{
    [Header("Grid Parent")]
    public Transform gridParent;

    [Header("Name Search Input")]
    public TMP_InputField nameInputField;

    public enum SortMode
    {
        BondLevelAsc,
        BondLevelDesc,
        CombatClassAsc,
        CombatClassDesc,
        AttackAsc,
        AttackDesc,
        HPAsc,
        HPDesc,
        DefenseAsc,
        DefenseDesc
    }

    [Header("Current Sort Mode")]
    public SortMode currentMode = SortMode.BondLevelAsc;
    [Header("Dropdown de Razas")]
    public TMP_Dropdown razaDropdown;

    // -------------------------------
    // UNITY LIFECYCLE
    // -------------------------------

    void Start()
    {
        if (nameInputField != null)
            nameInputField.onValueChanged.AddListener(delegate { FilterByName(); });

        InicializarDropdownRazas();
    }


    void Update()
    {
        SortGrid(currentMode);
    }
    void InicializarDropdownRazas()
    {
        if (razaDropdown == null) return;

        razaDropdown.ClearOptions();

        List<string> opciones = System.Enum.GetNames(typeof(HeroRace)).ToList();
        razaDropdown.AddOptions(opciones);

        razaDropdown.onValueChanged.AddListener(delegate {
            HeroRace razaSeleccionada = (HeroRace)razaDropdown.value;
            FiltrarPorRaza(razaSeleccionada);
        });
    }

    // -------------------------------
    // NAME SEARCH FILTER (INPUTFIELD)
    // -------------------------------

    void FilterByName()
    {
        if (nameInputField == null) return;

        string searchText = nameInputField.text.Trim().ToLower();

        foreach (Transform child in gridParent)
        {
            var slotUI = child.GetComponent<HeroSlotUI>();

            if (slotUI == null || slotUI.hero == null)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            // Si el input está vacío → mostrar todo
            if (string.IsNullOrEmpty(searchText))
            {
                child.gameObject.SetActive(true);
                continue;
            }

            string heroName = slotUI.hero.heroName.ToLower();

            // 🔥 NUEVO: búsqueda en tags
            bool tagMatch = slotUI.hero.tags
                .Any(t => t != null && t.ToLower().Contains(searchText));

            bool nameMatch = heroName.Contains(searchText);

            // Mostrar si coincide nombre o tags
            child.gameObject.SetActive(nameMatch || tagMatch);
        }
    }


    // -------------------------------
    // FILTRADO POR RELIGION
    // -------------------------------

    public void FiltrarPorReligion(HeroReligion religion)
    {
        foreach (Transform child in gridParent)
        {
            var slotUI = child.GetComponent<HeroSlotUI>();

            if (slotUI != null && slotUI.hero != null)
            {
                bool coincide = slotUI.hero.heroReligion == religion;
                child.gameObject.SetActive(coincide);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    public void FiltrarPorAtributo(HeroAttribute atributo)
    {
        foreach (Transform child in gridParent)
        {
            var slotUI = child.GetComponent<HeroSlotUI>();

            if (slotUI != null && slotUI.hero != null)
            {
                bool coincide = slotUI.hero.attribute == atributo;
                child.gameObject.SetActive(coincide);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    public void FiltrarPorOrganizacion(Alianza organizacion)
    {
        foreach (Transform child in gridParent)
        {
            var slotUI = child.GetComponent<HeroSlotUI>();

            if (slotUI != null && slotUI.hero != null)
            {
                bool coincide = slotUI.hero.alianza == organizacion;
                child.gameObject.SetActive(coincide);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }
    public void FiltrarPorRaza(HeroRace raza)
    {
        foreach (Transform child in gridParent)
        {
            var slotUI = child.GetComponent<HeroSlotUI>();

            if (slotUI != null && slotUI.hero != null)
            {
                bool coincide = slotUI.hero.races.Contains(raza);
                child.gameObject.SetActive(coincide);
            }
            else
            {
                child.gameObject.SetActive(false);
            }
        }
    }

    public void NATURAL()
    {
        FiltrarPorAtributo(HeroAttribute.Natural);
    }
    public void ARCANO()
    {
        FiltrarPorAtributo(HeroAttribute.Arcano);
    }
    public void PROFANO()
    {
        FiltrarPorAtributo(HeroAttribute.Profano);
    }
    public void DIVINO()
    {
        FiltrarPorAtributo(HeroAttribute.Divino);
    }
    public void MostrarTodos()
    {
        foreach (Transform child in gridParent)
        {
            child.gameObject.SetActive(true);
        }
    }
   

    // -------------------------------
    // SORT SHORTCUTS
    // -------------------------------

    public void CC() => SetSortMode(SortMode.CombatClassDesc);
    public void VIDA() => SetSortMode(SortMode.HPDesc);
    public void DAÑO() => SetSortMode(SortMode.AttackDesc);
    public void DEFENSA() => SetSortMode(SortMode.DefenseDesc);
    public void BOND() => SetSortMode(SortMode.BondLevelDesc);
    public void ALIANZA_CALDODRAGON() => FiltrarPorOrganizacion(Alianza.CaldoDeDragón);
    public void ALIANZA_CONSEJOOMEGA() => FiltrarPorOrganizacion(Alianza.ConsejoOmega);
    public void ALIANZA_MAGOR() => FiltrarPorOrganizacion(Alianza.Magor);
    public void ALIANZA_ELTRIBUNAL() => FiltrarPorOrganizacion(Alianza.ElTribunal);
    public void ALIANZA_PORTADORES() => FiltrarPorOrganizacion(Alianza.PortadoresDeMarca);
    public void ALIANZA_MOONHAVEN() => FiltrarPorOrganizacion(Alianza.Moonhaven);
    public void ALIANZA_ANTIGUOS() => FiltrarPorOrganizacion(Alianza.Antiguos);
    public void ALIANZA_MERCADONEGRO() => FiltrarPorOrganizacion(Alianza.MercadoNegro);
    public void ALIANZA_COMPANIA() => FiltrarPorOrganizacion(Alianza.Compañia);
    public void ALIANZA_VAPORLIBRE() => FiltrarPorOrganizacion(Alianza.VaPorLibre);

    public void SetSortMode(SortMode mode)
    {
        currentMode = mode;
        SortGrid(currentMode);
    }

    // -------------------------------
    // SORT GRID
    // -------------------------------

    void SortGrid(SortMode mode)
    {
        List<HeroSlotUI> slots = new List<HeroSlotUI>();

        foreach (Transform child in gridParent)
        {
            var slotUI = child.GetComponent<HeroSlotUI>();
            if (slotUI != null)
                slots.Add(slotUI);
        }

        IOrderedEnumerable<HeroSlotUI> ordered =
            slots.OrderByDescending(s => s.hero != null && s.hero.isUnlocked);

        switch (mode)
        {
            case SortMode.BondLevelAsc:
                ordered = ordered.ThenBy(s => s.hero != null ? s.hero.BondLevel : int.MinValue);
                break;

            case SortMode.BondLevelDesc:
                ordered = ordered.ThenByDescending(s => s.hero != null ? s.hero.BondLevel : int.MinValue);
                break;

            case SortMode.CombatClassAsc:
                ordered = ordered.ThenBy(s => s.hero != null ? (int)s.hero.CC : int.MinValue);
                break;

            case SortMode.CombatClassDesc:
                ordered = ordered.ThenByDescending(s => s.hero != null ? (int)s.hero.CC : int.MinValue);
                break;

            case SortMode.AttackAsc:
                ordered = ordered.ThenBy(s => s.hero != null ? s.hero.stats.attack : float.MinValue);
                break;

            case SortMode.AttackDesc:
                ordered = ordered.ThenByDescending(s => s.hero != null ? s.hero.stats.attack : float.MinValue);
                break;

            case SortMode.HPAsc:
                ordered = ordered.ThenBy(s => s.hero != null ? s.hero.stats.maxHealth : float.MinValue);
                break;

            case SortMode.HPDesc:
                ordered = ordered.ThenByDescending(s => s.hero != null ? s.hero.stats.maxHealth : float.MinValue);
                break;

            case SortMode.DefenseAsc:
                ordered = ordered.ThenBy(s => s.hero != null ? s.hero.stats.defense : float.MinValue);
                break;

            case SortMode.DefenseDesc:
                ordered = ordered.ThenByDescending(s => s.hero != null ? s.hero.stats.defense : float.MinValue);
                break;
        }

        List<HeroSlotUI> orderedList = ordered.ToList();

        for (int i = 0; i < orderedList.Count; i++)
        {
            orderedList[i].transform.SetSiblingIndex(i);
        }
    }
}