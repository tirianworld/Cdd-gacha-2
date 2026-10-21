using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SkillButtonDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler, IDropHandler
{
    public HeroSkill skill;
    public int rank = 1;
    private CanvasGroup canvasGroup;
    public Transform originalParent;
    public bool isCombined = false;
    [Header("Overlay de rango")]
    public Image rankOverlay; // asigna en el inspector
    public Sprite rank1Sprite;
    public Sprite rank2Sprite;
    public Sprite rank3Sprite;

   

    void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }

    // ✅ Implementación obligatoria
    public void OnBeginDrag(PointerEventData eventData)
    {
        originalParent = transform.parent;
        canvasGroup.blocksRaycasts = false;
        transform.SetParent(transform.root); // mover al canvas root
    }

    // ✅ Implementación obligatoria
    public void OnDrag(PointerEventData eventData)
    {
        transform.position = eventData.position;
    }

    // ✅ Implementación obligatoria
    public void OnEndDrag(PointerEventData eventData)
    {
        transform.SetParent(originalParent);
        canvasGroup.blocksRaycasts = true;
    }

    // IDropHandler también necesita su método
    public void OnDrop(PointerEventData eventData)
    {
        var dragged = eventData.pointerDrag?.GetComponent<SkillButtonDrag>();
        if (dragged == null || dragged == this) return;

        // ✅ Si son la misma skill y rango → combinar
        if (skill != null && dragged.skill != null &&
            skill.skillName == dragged.skill.skillName && rank == dragged.rank)
        {
            rank = Mathf.Min(3, rank + 1);
            Debug.Log($"Habilidad {skill.skillName} combinada → rango {rank}");
            UpdateVisual();
            dragged.gameObject.SetActive(false);
        }
        else
        {
            // ✅ Si no son combinables → reordenar en el layout
            int targetIndex = transform.GetSiblingIndex();
            dragged.transform.SetParent(transform.parent, false);
            dragged.transform.SetSiblingIndex(targetIndex);
            Debug.Log($"Carta {dragged.skill.skillName} movida a posición {targetIndex}");
        }
    }


    public void UpdateVisual()
    {
        var img = GetComponentInChildren<Image>();
        var txt = GetComponentInChildren<TMPro.TextMeshProUGUI>();
        if (img != null) img.sprite = skill.icon;
        if (txt != null) txt.text = $"{skill.skillName} (R{rank})";
   
        // ✅ Cambiar overlay según rango
        if (rankOverlay != null)
        {
            switch (rank)
            {
                case 1: rankOverlay.sprite = rank1Sprite; break;
                case 2: rankOverlay.sprite = rank2Sprite; break;
                case 3: rankOverlay.sprite = rank3Sprite; break;
            }
            rankOverlay.gameObject.SetActive(true);
        }
    }
    public void ResetSkillButton()
    {
        rank = 1;
        isCombined = false;

        // ✅ Reactivar y volver a ser hijo del layout
        if (originalParent != null)
            transform.SetParent(originalParent, false);

        gameObject.SetActive(true);

        var layoutElement = GetComponent<UnityEngine.UI.LayoutElement>();
        if (layoutElement != null)
            layoutElement.ignoreLayout = false;

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }

        UpdateVisual();
    }


}
