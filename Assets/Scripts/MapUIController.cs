using UnityEngine;
using UnityEngine.EventSystems;
using DG.Tweening;

public class MapUIController : MonoBehaviour,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IScrollHandler
{
    [Header("Referencias")]
    public RectTransform mapRect;
    private RectTransform parentRect;

    [Header("Zoom")]
    public float zoomSpeed = 0.2f;
    public float minZoom = 0.7f;
    public float maxZoom = 2.5f;

    [Header("Inercia")]
    public float inertiaPower = 0.015f;
    public float inertiaDuration = 1.2f;

    [Header("Elasticidad")]
    public float elasticStrength = 0.35f;
    public float bounceDuration = 0.6f;

    private Vector2 velocity;
    private Vector2 lastPointerPos;
    private bool dragging;

    private Tweener moveTween;

    void Start()
    {
        parentRect = mapRect.parent.GetComponent<RectTransform>();
    }

    #region Drag

    public void OnBeginDrag(PointerEventData eventData)
    {
        dragging = true;
        velocity = Vector2.zero;
        moveTween?.Kill();
        lastPointerPos = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        Vector2 delta = eventData.position - lastPointerPos;
        lastPointerPos = eventData.position;

        velocity = delta / Time.deltaTime;

        Vector2 newPos = mapRect.anchoredPosition + ApplyElastic(delta);
        mapRect.anchoredPosition = newPos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        dragging = false;
        ApplyInertia();
    }

    #endregion

    #region Zoom

    public void OnScroll(PointerEventData eventData)
    {
        Zoom(eventData.scrollDelta.y * zoomSpeed);
    }

    private void Zoom(float amount)
    {
        float newScale = Mathf.Clamp(
            mapRect.localScale.x + amount,
            minZoom,
            maxZoom
        );

        mapRect.DOScale(newScale, 0.25f).SetEase(Ease.OutQuad);
    }

    #endregion

    #region Movement Core

    private Vector2 ApplyElastic(Vector2 delta)
    {
        Vector2 pos = mapRect.anchoredPosition;
        Vector2 limits = GetLimits();

        float x = delta.x;
        float y = delta.y;

        if (pos.x > limits.x || pos.x < -limits.x)
            x *= elasticStrength;

        if (pos.y > limits.y || pos.y < -limits.y)
            y *= elasticStrength;

        return new Vector2(x, y);
    }

    private void ApplyInertia()
    {
        Vector2 target = mapRect.anchoredPosition + velocity * inertiaPower;

        moveTween = mapRect
            .DOAnchorPos(target, inertiaDuration)
            .SetEase(Ease.OutCubic)
            .OnUpdate(CheckBounds)
            .OnComplete(SpringBack);
    }

    private void CheckBounds()
    {
        Vector2 limits = GetLimits();
        Vector2 pos = mapRect.anchoredPosition;

        if (Mathf.Abs(pos.x) > limits.x * 1.4f ||
            Mathf.Abs(pos.y) > limits.y * 1.4f)
        {
            moveTween?.Kill();
            SpringBack();
        }
    }

    private void SpringBack()
    {
        Vector2 clamped = GetClampedPosition(mapRect.anchoredPosition);

        mapRect
            .DOAnchorPos(clamped, bounceDuration)
            .SetEase(Ease.OutBack);
    }

    #endregion

    #region Limits

    private Vector2 GetLimits()
    {
        float scaledWidth = mapRect.rect.width * mapRect.localScale.x;
        float scaledHeight = mapRect.rect.height * mapRect.localScale.y;

        float parentWidth = parentRect.rect.width;
        float parentHeight = parentRect.rect.height;

        float limitX = Mathf.Max(0, (scaledWidth - parentWidth) / 2f);
        float limitY = Mathf.Max(0, (scaledHeight - parentHeight) / 2f);

        return new Vector2(limitX, limitY);
    }

    private Vector2 GetClampedPosition(Vector2 currentPos)
    {
        Vector2 limits = GetLimits();

        float x = Mathf.Clamp(currentPos.x, -limits.x, limits.x);
        float y = Mathf.Clamp(currentPos.y, -limits.y, limits.y);

        return new Vector2(x, y);
    }

    #endregion
}