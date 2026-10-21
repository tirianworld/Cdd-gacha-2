using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public int cardIndex;

    private Canvas canvas;
    private RectTransform rect;
    private CanvasGroup group;

    private Vector3 dragOffset;
    public bool droppedOnCard = false;
    public static bool IsDragging = false;
    public bool isDraggingThis = false;
    private HandManager handManager;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        group = GetComponent<CanvasGroup>();
        handManager = FindObjectOfType<HandManager>();

    }

    private void Start()
    {
        if (group != null)
        {
            group.interactable = true;
            group.blocksRaycasts = true;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        transform.SetParent(handManager.dragLayer, true);
        var anim = GetComponent<CardAnimatorControllerDOTween>();
        if (anim != null)
            anim.OnBeginDragFix();

        IsDragging = true;
        isDraggingThis = true;
        droppedOnCard = false;

        if (group != null)
            group.blocksRaycasts = false;

        // Convertir posiciÃ³n del ratÃ³n a World Space
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldPos
        );

        dragOffset = rect.position - worldPos;

        // Sacar la carta del Grid
        transform.SetParent(handManager.dragLayer, true);

    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector3 worldPos;
        RectTransformUtility.ScreenPointToWorldPointInRectangle(
            rect.parent as RectTransform,
            eventData.position,
            eventData.pressEventCamera,
            out worldPos
        );
        rect.position = worldPos + dragOffset;
    }

    public void OnEndDrag(PointerEventData eventData)

    {
        transform.SetParent(handManager.handPanel, false);
        var anim = GetComponent<CardAnimatorControllerDOTween>();
        if (anim != null)
            anim.OnEndDragFix();

        if (group != null)
        {
            group.alpha = 1f;
            group.blocksRaycasts = true;
        }

        // Si NO se soltÃ³ sobre otra carta â†’ volver al grid
        if (!droppedOnCard)
        {
            transform.SetParent(handManager.handPanel, false);
            rect.localPosition = Vector3.zero;
        }


        isDraggingThis = false;
        IsDragging = false;

        handManager.RefreshHandUI();
        StartCoroutine(RefreshAfterDrag());
    }

    private IEnumerator RefreshAfterDrag()
    {
        yield return null;
        handManager.RefreshHandUI();
    }

    private void OnDisable()
    {
        isDraggingThis = false;
        IsDragging = false;
        if (group != null)
        {
            group.blocksRaycasts = true;
        }
    }
}


