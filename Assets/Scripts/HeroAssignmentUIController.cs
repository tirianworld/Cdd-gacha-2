using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class HeroAssignmentUIController : MonoBehaviour
{
    [Header("Referencia al Manager")]
    public HeroAssignmentManager manager;

    [Header("Botones de slots (0–3)")]
    public Image[] slotButtons = new Image[4];

    [Header("Sprites cuando el slot está vacío")]
    public Sprite[] emptySlotSprites = new Sprite[4];
    public Sprite[] fullSlotSprites = new Sprite[4];



    [Header("Botón del menú")]
    public Image menuButton;
    public Sprite menuEmptySprite;
    public Sprite menuAssignedSprite;
    [Header("Héroe que activa el sprite de ocupado")]
    public Hero heroPermitido;

    private void OnEnable()
    {
        RefreshAll();
    }

    private void Start()
    {
        RefreshAll();
    }
    private void Update()
    {
        RefreshAll();
    }
    // ============================================================
    // REFRESCAR TODA LA UI
    // ============================================================
    public void RefreshAll()
    {
        RefreshSlots();
        RefreshMenuButton();
    }

    // ============================================================
    // REFRESCAR LOS 4 SLOTS
    // ============================================================
    private void RefreshSlots()
    {
        for (int i = 0; i < 4; i++)
        {
            Hero heroEnSlot = manager.GetHeroByIndex(i);

            // Si el héroe del slot NO es el héroe de este UI → vacío
            if (heroEnSlot != heroPermitido)
            {
                slotButtons[i].sprite = emptySlotSprites[i];
            }
            else
            {
                // Si este UI controla a ese héroe y está en este slot → lleno
                slotButtons[i].sprite = fullSlotSprites[i];
            }
        }
    }






    // ============================================================
    // REFRESCAR BOTÓN DEL MENÚ
    // ============================================================
    private void RefreshMenuButton()
    {
        bool hasAnyHero =
            manager.Hero1 != null ||
            manager.Hero2 != null ||
            manager.Hero3 != null ||
            manager.Hero4 != null;

        menuButton.sprite = hasAnyHero ? menuAssignedSprite : menuEmptySprite;
    }

    // ============================================================
    // OBTENER SPRITE SEGÚN EL HÉROE
    // ============================================================
  
}

[System.Serializable]
public class HeroSpritePair
{
    public Hero hero;
    public Sprite sprite;
}
