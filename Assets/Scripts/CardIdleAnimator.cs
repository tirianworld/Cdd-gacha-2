using UnityEngine;
using DG.Tweening;
using System.Collections;
using UnityEngine.EventSystems;


public class CardAnimatorControllerDOTween : MonoBehaviour
{
    public ShineDOTween shine;
    public CardDragHandler drag;
    [Header("Hover")]
    public float hoverScale = 1.15f;
    public float hoverMoveY = 20f;
    public float hoverDuration = 0.15f;
    public Ease hoverEase = Ease.OutQuad;

    private Tween hoverTween;
    private bool isHovering = false;

    [Header("Idle")]
    public float idleMoveY = 10f;
    public float idleDuration = 1.5f;
    public Ease idleEase = Ease.InOutSine;

    [Header("Sway")]
    public float swayAngle = 8f;
    public float swayDuration = 0.25f;
    public Ease swayEase = Ease.InOutSine;
    [Header("Skill Use")]
    public float skillPunchScale = 1.35f;
    public float skillPunchDuration = 0.25f;
    public float skillShakeStrength = 15f;
    public float skillShakeDuration = 0.20f;
    public Color skillFlashColor = Color.white;
    public float skillFlashDuration = 0.15f;

    private Tween skillTween;
    private UnityEngine.UI.Image img;
    private Color originalColor;

    private RectTransform rect;
    private Tween idleTween;
    private Tween swayTween;

    private Vector2 basePos;
    private bool initialized = false;
    private bool dragging = false;
    private Vector3 baseScale;

    private void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (drag == null) drag = GetComponent<CardDragHandler>();

        img = GetComponent<UnityEngine.UI.Image>();
        if (img != null)
            originalColor = img.color;
    }


    private void Start()
    {
        // Guardamos la posición base una vez que el layout ya colocó la carta
        StartCoroutine(InitAfterLayout());
    }

    private IEnumerator InitAfterLayout()
    {
        // Esperar a que el layout deje de moverse
        Vector2 lastPos = rect.anchoredPosition;
        yield return null;

        int safety = 0;
        while (Vector2.Distance(lastPos, rect.anchoredPosition) > 0.1f && safety < 10)
        {
            lastPos = rect.anchoredPosition;
            yield return null;
            safety++;
        }

        // Ahora sí: layout estable
        basePos = rect.anchoredPosition;
        baseScale = rect.localScale;

        initialized = true;
    }


    private void Update()
    {
        if (!initialized) return;

        if (shine != null && shine.masterEnable)
        {
            StopTweens();
            return;
        }

        if (isHovering && !drag.isDraggingThis)
            return; // hover manda, no idle/sway

        if (dragging)
            StartSway();
        else
            StartIdle();
    }


    private void StartIdle()
    {
        if (idleTween != null && idleTween.IsActive()) return;

        StopTweens();

        rect.anchoredPosition = basePos;

        idleTween = rect.DOAnchorPosY(basePos.y + idleMoveY, idleDuration)
            .SetEase(idleEase)
            .SetLoops(-1, LoopType.Yoyo);
    }

    private void StartSway()
    {
        if (swayTween != null && swayTween.IsActive()) return;

        StopTweens();

        rect.anchoredPosition = basePos;
        rect.localRotation = Quaternion.identity;

        swayTween = rect.DOLocalRotate(new Vector3(0, 0, swayAngle), swayDuration)
            .SetEase(swayEase)
            .SetLoops(-1, LoopType.Yoyo);
    }

    public void StopTweens()
    {
        idleTween?.Kill();
        swayTween?.Kill();
        idleTween = null;
        swayTween = null;

        rect.localRotation = Quaternion.identity;
    }
    public void PlayMergeAnimation()
    {
        StopTweens(); // detener idle/sway

        rect.localRotation = Quaternion.identity;

        Vector3 originalScale = rect.localScale;
        Vector3 punchScale = originalScale * 1.25f;

        Sequence seq = DOTween.Sequence();

        seq.Append(rect.DOScale(punchScale, 0.15f).SetEase(Ease.OutBack))
           .Append(rect.DOScale(originalScale, 0.20f).SetEase(Ease.InOutQuad))
           .Join(rect.DOPunchRotation(new Vector3(0, 0, 15f), 0.3f, 10, 0.5f))
           .OnComplete(() =>
           {
               if (gameObject.activeInHierarchy) 
                   StartCoroutine(ReenableIdleAfterLayoutFullySettled());
           });
    }

    private IEnumerator ReenableIdleAfterLayoutFullySettled()
    {
        // Esperar a que HandManager termine su RefreshHandUI
        yield return null;
        yield return null;
        yield return null; // ← este tercer frame elimina el flash

        // Recalcular posición real después del layout
        basePos = rect.anchoredPosition;

        // Si no se está arrastrando, volver a Idle
        if (!drag.isDraggingThis)
            StartIdle();
    }


    public void OnBeginDragFix()
    {
        StopTweens();
        dragging = true;
    }

    public void OnEndDragFix()
    {
        StartCoroutine(RecalculateBaseAfterLayout());
    }
    private IEnumerator RecalculateBaseAfterLayout()
    {
        dragging = false;

        // Esperar a que el layout coloque la carta en su sitio final
        yield return null;
        yield return null;
        yield return null;

        basePos = rect.anchoredPosition;

        // Si Shine no está activo, volver a Idle
        if (!shine.masterEnable)
            StartIdle();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (drag.isDraggingThis) return;   // no hover mientras arrastras
        if (shine != null && shine.masterEnable) return;

        isHovering = true;
        PlayHoverAnimation();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        isHovering = false;
        StopHoverAnimation();
    }
    private void PlayHoverAnimation()
    {
        StopTweens(); // detener idle/sway

        hoverTween?.Kill();

        hoverTween = DOTween.Sequence()
            .Join(rect.DOScale(hoverScale, hoverDuration).SetEase(hoverEase))
            .Join(rect.DOAnchorPosY(basePos.y + hoverMoveY, hoverDuration).SetEase(hoverEase));
    }
    private void StopHoverAnimation()
    {
        hoverTween?.Kill();

        rect.DOScale(1f, hoverDuration).SetEase(hoverEase);
        rect.DOAnchorPos(basePos, hoverDuration).SetEase(hoverEase);

        if (!drag.isDraggingThis)
            StartIdle();
    }
    public void ForceRecalculateBase()
    {
        if (gameObject.activeInHierarchy) 
        StartCoroutine(RecalculateAfterLayout());
    }

    private IEnumerator RecalculateAfterLayout()
    {
        yield return null;
        yield return null;
        yield return null;

        basePos = rect.anchoredPosition;
        baseScale = rect.localScale;
    }




}
