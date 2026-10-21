using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using DG.Tweening;
using DG.Tweening.Core;
using DG.Tweening.Plugins.Options;



public class CardUI : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IEndDragHandler
{
    public int cardIndex;
    public HandManager handManager;

    public Image icon;
    public TextMeshProUGUI skillName;
    public TextMeshProUGUI tierText;
    public Image rarityFrame;
    public TextMeshProUGUI descriptionText;
    public Material cardMaterialInstance;
    private Tween glowTween;
    public bool isDragging = false;

    private void Awake()
    {
        if (icon != null && icon.material != null)
        {
            cardMaterialInstance = Instantiate(icon.material);
            icon.material = cardMaterialInstance;
        }
    }


    public void UpdateVisual(HandSkill skill)
    {
        // Si la carta es nula, desactivamos el objeto y salimos
        if (skill == null || skill.baseSkill == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Asegurar que la carta está activa
        if (!gameObject.activeSelf)
            gameObject.SetActive(true);

        // Ahora sí es seguro acceder a los datos
        icon.sprite = skill.baseSkill.icon;
        skillName.text = skill.baseSkill.skillName;
        if (tierText != null) tierText.text = "Tier " + skill.tier;
        if (descriptionText != null) descriptionText.text = skill.baseSkill.description;

        switch (skill.tier)
        {
            case 1:
                rarityFrame.sprite = skill.baseSkill.tier1Sprite;
                break;
            case 2:
                rarityFrame.sprite = skill.baseSkill.tier2Sprite;
                break;
            case 3:
                rarityFrame.sprite = skill.baseSkill.tier3Sprite;
                break;
            default:
                rarityFrame.sprite = skill.baseSkill.tier1Sprite;
                break;
        }
    }



    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        StartCoroutine(ResetDragFlag());
    }

    private System.Collections.IEnumerator ResetDragFlag()
    {
        yield return null;
        isDragging = false;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isDragging) return;
        if (handManager == null) return;
        if (handManager.actionsLeft <= 0) return;

        handManager.UseSkill(cardIndex);
        handManager.RefreshHandUI();
    }
    public void EnableRankupGlow()
    {
        if (cardMaterialInstance == null) return;

        // Color rosado
        Color pink = new Color(1f, 0.3f, 0.7f, 1f);

        // Asignar color inicial
        cardMaterialInstance.SetColor("_GlowColor", pink);
        cardMaterialInstance.SetFloat("_GlowPower", 0.4f);

        glowTween?.Kill();

        // DOTween Pro: animación blendable para evitar saltos
        glowTween = DOTween.To(
            () => cardMaterialInstance.GetFloat("_GlowPower"),
            x => cardMaterialInstance.SetFloat("_GlowPower", x),
            1.3f,
            0.6f
        )
        .SetEase(Ease.InOutQuad)
        .SetLoops(-1, LoopType.Yoyo)
        .SetUpdate(true); // sigue animando aunque haya pausas
    }
    public void DisableRankupGlow()
    {
        if (cardMaterialInstance == null) return;

        glowTween?.Kill();

        // DOTween Pro: animación suave al apagar
        cardMaterialInstance
            .DOFloat(0f, "_GlowPower", 0.25f)
            .SetEase(Ease.OutQuad);
    }

}

