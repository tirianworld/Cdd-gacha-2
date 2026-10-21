using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

// ============================================================
//   SLOT DE EQUIPO ACTUAL (uno por categoría: anillo, sello,
//   casco, arma, armadura). No se instancia dinámicamente:
//   se coloca UNA VEZ en la escena/prefab del panel de héroe,
//   uno por cada Equipment_SO.Type.
// ============================================================
public class EquippedEquipmentSlot : MonoBehaviour
{
    [Header("Referencias")]
    public Hero targetHero;
    public GameManager gamemanager;

    [Header("Categoría que representa este slot")]
    public Equipment_SO.Type slotType;

    [Header("Referencia opcional al Equipment_SO (solo informativa/placeholder)")]
    public Equipment_SO baseEquipment;

    [Header("Grid de inventario (para que el item reaparezca al desequipar)")]
    public EquipmentGridSorter gridSorter;

    [Header("UI")]
    public Button unequipButton;
    public Image iconImage;
    public TextMeshProUGUI levelText;

    [Header("Estrellas (rango 0-5): un GameObject por estrella")]
    public GameObject[] starObjects = new GameObject[5];

    [Header("Despertar (Awaken): un GameObject por rango")]
    public GameObject awakeningR;
    public GameObject awakeningSR;
    public GameObject awakeningSSR;

    [Header("Estado vacío (opcional)")]
    public GameObject emptyStateObject; // se activa cuando no hay nada equipado en esta categoría

    [Header("Icono de Atributo Elemental")]
    // ⚠️ El orden de este array debe coincidir EXACTAMENTE con el orden de declaración
    // del enum HeroAttribute (ej: [0]=Arcano, [1]=Profano, [2]=Natural, [3]=Divino).
    public Sprite[] attributeSprites;
    // Image donde se pinta el sprite del atributo elemental del arma/equipo equipado.
    public Image attributeImage;

    // Instancia actualmente equipada en esta categoría (null si no hay nada)
    public EquipmentInstance CurrentInstance { get; private set; }

    private void Start()
    {
        if (gamemanager == null)
            gamemanager = GameManager.Instance;

        if (unequipButton != null)
            unequipButton.onClick.AddListener(Unequip);

        RefreshSlot();
    }

    private void OnEnable()
    {
        RefreshSlot();
    }

    // ============================================================
    //   REFRESCAR: busca en el héroe qué tiene equipado de esta categoría
    // ============================================================
    public void RefreshSlot()
    {
        if (targetHero == null)
        {
            SetEmpty();
            return;
        }

        CurrentInstance = targetHero.equippedInstances
            .FirstOrDefault(i => i != null && i.baseData != null && i.baseData.type == slotType);

        if (CurrentInstance == null)
        {
            SetEmpty();
            return;
        }

        UpdateUI();
    }

    private void SetEmpty()
    {
        CurrentInstance = null;

        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (levelText != null)
            levelText.text = "";

        UpdateStars(0);
        UpdateAwakening(null);
        UpdateAttributeIcon(null);

        if (unequipButton != null)
            unequipButton.interactable = false;

        if (emptyStateObject != null)
            emptyStateObject.SetActive(true);
    }

    // ============================================================
    //   ACTUALIZAR UI CON LA PIEZA EQUIPADA
    // ============================================================
    private void UpdateUI()
    {
        if (CurrentInstance == null || CurrentInstance.baseData == null) return;

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = CurrentInstance.baseData.icon;
        }

        if (levelText != null)
            levelText.text = "Nv " + CurrentInstance.level;

        UpdateStars(CurrentInstance.stars);
        UpdateAwakening(CurrentInstance.baseData.rango);
        UpdateAttributeIcon(CurrentInstance.baseData.elementalAffinity);

        if (unequipButton != null)
            unequipButton.interactable = true;

        if (emptyStateObject != null)
            emptyStateObject.SetActive(false);
    }

    private void UpdateStars(int stars)
    {
        int activeStars = Mathf.Clamp(stars, 0, starObjects.Length);

        for (int i = 0; i < starObjects.Length; i++)
        {
            if (starObjects[i] != null)
                starObjects[i].SetActive(i < activeStars);
        }
    }

    private void UpdateAwakening(Equipment_SO.Rango? rango)
    {
        bool isR = rango == Equipment_SO.Rango.R;
        bool isSR = rango == Equipment_SO.Rango.SR;
        bool isSSR = rango == Equipment_SO.Rango.SSR;

        if (awakeningR != null) awakeningR.SetActive(isR);
        if (awakeningSR != null) awakeningSR.SetActive(isSR);
        if (awakeningSSR != null) awakeningSSR.SetActive(isSSR);
    }

    // ============================================================
    //   ICONO DE ATRIBUTO ELEMENTAL
    //   Cambia la imagen según la afinidad elemental (HeroAttribute)
    //   del equipo actualmente puesto. null = sin equipo -> se oculta.
    // ============================================================
    private void UpdateAttributeIcon(HeroAttribute? attribute)
    {
        if (attributeImage == null)
            return;

        if (attribute == null || attributeSprites == null || attributeSprites.Length == 0)
        {
            attributeImage.sprite = null;
            attributeImage.enabled = false;
            return;
        }

        int index = (int)attribute.Value;

        if (index < 0 || index >= attributeSprites.Length || attributeSprites[index] == null)
        {
            attributeImage.sprite = null;
            attributeImage.enabled = false;
            return;
        }

        attributeImage.sprite = attributeSprites[index];
        attributeImage.enabled = true;
    }

    // ============================================================
    //   DESEQUIPAR
    // ============================================================
    public void Unequip()
    {
        if (targetHero == null || CurrentInstance == null) return;

        var inst = CurrentInstance;

        // Revierte stats base + de instancia y quita la skill otorgada (todo centralizado en Hero.Unequip)
        targetHero.Unequip(inst);
        targetHero.SaveHeroData_Prefs();

        if (gamemanager != null)
        {
            gamemanager.SaveEquipmentInventory();
            gamemanager.SaveEquipment();
        }

        // Refrescar este slot (queda vacío)
        RefreshSlot();

        // El grid de inventario se actualiza solo en su propio Update(),
        // pero forzamos un refresco inmediato para que no haya delay visual.
        if (gridSorter != null)
            gridSorter.SpawnAllEquipment();
    }
}