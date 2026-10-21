using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public class HeroSlotUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Referencias")]
    public Image portraitImage;
    public TMP_Text nameText;
    public GameObject lockedOverlay;

    [Header("Datos del héroe")]
    public Hero hero;
    public Sprite lockedSprite;
    public Sprite unlockedSprite;
    public bool Herohold;

    [Header("Dificultad (un objeto por nivel)")]
    public GameObject diffMuyFacil;
    public GameObject diffPrincipiante;
    public GameObject diffIntermedioFacil;
    public GameObject diffIntermedioDificil;
    public GameObject diffDificil;
    public GameObject diffMuyDificil;
    public GameObject diffInsano;
    public GameObject diffTeLaMamas;

    [Header("Nivel")]
    public GameObject levelObject;
    public TMP_Text levelText;

    [Header("Estrellas (1 a 5)")]
    public GameObject star1;
    public GameObject star2;
    public GameObject star3;
    public GameObject star4;
    public GameObject star5;

    [Header("Grado inicial")]
    public GameObject initialRankObject;

    [Header("Estado de desbloqueo")]
    public GameObject unlockedObject;
    public GameObject lockedObject;

    // -------------------------------
    // NUEVO SISTEMA DE HOLD
    // -------------------------------
    [Header("Mantener pulsado")]
    public GameObject holdFillObject;          // Debe tener una Image con fillAmount
    public float holdDuration = 1.2f;          // Tiempo para completar
    public float holdDelay = 0.6f;
    public UnityEvent onHoldComplete;          // Evento a ejecutar
    private Image holdFillImage;
    private Tween holdTween;
    private bool isHolding = false;
    private Tween delayTween;

    private void Start()
    {
        if (holdFillObject != null)
        {
            holdFillImage = holdFillObject.GetComponent<Image>();
            holdFillObject.SetActive(false);
        }
    }

    public void Setup(Hero heroData, Sprite locked, Sprite unlocked)
    {
        hero = heroData;
        lockedSprite = locked;
        unlockedSprite = unlocked;
        Refresh();
    }

    private void Update()
    {
        Refresh();
    }

    public void Refresh()
    {
        bool unlocked = hero != null && hero.isUnlocked;

        if (unlockedObject != null)
            unlockedObject.SetActive(unlocked);

        if (lockedObject != null)
            lockedObject.SetActive(!unlocked);

        GetComponent<Button>().interactable = unlocked;

        if (levelText != null)
            levelText.text = unlocked ? $"Lv. {hero.stats.level}" : "Lv. ?";
    }

    // ---------------------------------------------------------
    // SISTEMA DE HOLD: Mantener pulsado para ejecutar un evento
    // ---------------------------------------------------------
    public void OnPointerDown(PointerEventData eventData)
    {
        if (!GetComponent<Button>().interactable)
            return;

        isHolding = true;
        Herohold = true; // ← ACTIVAR AQUÍ

        if (holdFillObject != null)
        {
            holdFillObject.SetActive(true);
            holdFillImage.fillAmount = 0f;
        }

        delayTween = DOVirtual.DelayedCall(holdDelay, () =>
        {
            if (!isHolding) return;

            holdTween = holdFillImage
                .DOFillAmount(1f, holdDuration)
                .SetEase(Ease.Linear)
                .OnComplete(() =>
                {
                    if (isHolding)
                        onHoldComplete?.Invoke();
                });
        });
    }



    public void OnPointerUp(PointerEventData eventData)
    {
        isHolding = false;
        Herohold = false; // ← DESACTIVAR AQUÍ

        if (delayTween != null && delayTween.IsActive())
            delayTween.Kill();

        if (holdTween != null && holdTween.IsActive())
            holdTween.Kill();

        if (holdFillObject != null)
            holdFillObject.SetActive(false);
    }



    // ---------------------------------------------------------
    // RESTO DE MÉTODOS ORIGINALES (sin cambios)
    // ---------------------------------------------------------

    public void StarsShow()
    {
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
        star4.SetActive(false);
        star5.SetActive(false);

        switch (hero.Stars)
        {
            case 1: star1.SetActive(true); break;
            case 2: star2.SetActive(true); break;
            case 3: star3.SetActive(true); break;
            case 4: star4.SetActive(true); break;
            case 5: star5.SetActive(true); break;
        }

        diffMuyFacil.SetActive(false);
        diffPrincipiante.SetActive(false);
        diffIntermedioFacil.SetActive(false);
        diffIntermedioDificil.SetActive(false);
        diffDificil.SetActive(false);
        diffMuyDificil.SetActive(false);
        diffInsano.SetActive(false);
        diffTeLaMamas.SetActive(false);

        initialRankObject.SetActive(false);
        levelObject.SetActive(false);
    }

    public void DificultyShow()
    {
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
        star4.SetActive(false);
        star5.SetActive(false);

        initialRankObject.SetActive(false);
        levelObject.SetActive(false);

        diffMuyFacil.SetActive(false);
        diffPrincipiante.SetActive(false);
        diffIntermedioFacil.SetActive(false);
        diffIntermedioDificil.SetActive(false);
        diffDificil.SetActive(false);
        diffMuyDificil.SetActive(false);
        diffInsano.SetActive(false);
        diffTeLaMamas.SetActive(false);

        switch (hero.CC)
        {
            case CombatClassLevel.MuyFacil: diffMuyFacil.SetActive(true); break;
            case CombatClassLevel.Principiante: diffPrincipiante.SetActive(true); break;
            case CombatClassLevel.IntermedioFacil: diffIntermedioFacil.SetActive(true); break;
            case CombatClassLevel.IntermedioDificil: diffIntermedioDificil.SetActive(true); break;
            case CombatClassLevel.Dificil: diffDificil.SetActive(true); break;
            case CombatClassLevel.MuyDificil: diffMuyDificil.SetActive(true); break;
            case CombatClassLevel.Insano: diffInsano.SetActive(true); break;
            case CombatClassLevel.TeLaMamas: diffTeLaMamas.SetActive(true); break;
        }
    }

    public void RankShow()
    {
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
        star4.SetActive(false);
        star5.SetActive(false);

        diffMuyFacil.SetActive(false);
        diffPrincipiante.SetActive(false);
        diffIntermedioFacil.SetActive(false);
        diffIntermedioDificil.SetActive(false);
        diffDificil.SetActive(false);
        diffMuyDificil.SetActive(false);
        diffInsano.SetActive(false);
        diffTeLaMamas.SetActive(false);

        levelObject.SetActive(false);

        initialRankObject.SetActive(true);
    }

    public void LevelShow()
    {
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
        star4.SetActive(false);
        star5.SetActive(false);

        diffMuyFacil.SetActive(false);
        diffPrincipiante.SetActive(false);
        diffIntermedioFacil.SetActive(false);
        diffIntermedioDificil.SetActive(false);
        diffDificil.SetActive(false);
        diffMuyDificil.SetActive(false);
        diffInsano.SetActive(false);
        diffTeLaMamas.SetActive(false);

        initialRankObject.SetActive(false);

        levelObject.SetActive(true);
    }

    public void OnClick()
    {
        if (hero != null && hero.isUnlocked)
            Debug.Log($"Seleccionaste a {hero.heroName}");
        else
            Debug.Log("Este héroe aún está bloqueado.");
    }

    public void HideAll()
    {
        star1.SetActive(false);
        star2.SetActive(false);
        star3.SetActive(false);
        star4.SetActive(false);
        star5.SetActive(false);

        diffMuyFacil.SetActive(false);
        diffPrincipiante.SetActive(false);
        diffIntermedioFacil.SetActive(false);
        diffIntermedioDificil.SetActive(false);
        diffDificil.SetActive(false);
        diffMuyDificil.SetActive(false);
        diffInsano.SetActive(false);
        diffTeLaMamas.SetActive(false);

        if (levelObject != null)
            levelObject.SetActive(false);

        if (initialRankObject != null)
            initialRankObject.SetActive(false);
    }
}
