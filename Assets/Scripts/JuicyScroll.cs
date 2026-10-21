using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class JuicyScroll : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float smoothScrollDuration = 0.3f; // Duración del scroll suave
    public float bounceStrength = 0.2f;      // Fuerza del rebote
    public Ease scrollEase = Ease.OutCubic;  // Tipo de suavizado

    private float targetPosition = 0f;
    private bool isDragging = false;

    private void Start()
    {
        if (scrollRect == null)
        {
            scrollRect = GetComponent<ScrollRect>();
        }

        scrollRect.onValueChanged.AddListener(OnScrollValueChanged);
    }

    private void Update()
    {
        // Solo permite interacción mientras no estás arrastrando
        if (!isDragging && Input.GetMouseButtonUp(0))
        {
            ApplyBounceEffect();
        }
    }

    private void OnScrollValueChanged(Vector2 value)
    {
        // Cuando se detecta arrastre, cancela cualquier animación previa
        if (Input.GetMouseButton(0))
        {
            DOTween.Kill(scrollRect);
            isDragging = true;
        }
        else
        {
            isDragging = false;
        }
    }

    public void SmoothScrollTo(float normalizedPosition)
    {
        DOTween.To(() => scrollRect.verticalNormalizedPosition,
                   x => scrollRect.verticalNormalizedPosition = x,
                   normalizedPosition,
                   smoothScrollDuration)
               .SetEase(scrollEase);
    }

    private void ApplyBounceEffect()
    {
        // Rebota hacia el límite superior o inferior si se supera
        if (scrollRect.verticalNormalizedPosition > 1f)
        {
            DOTween.To(() => scrollRect.verticalNormalizedPosition,
                       x => scrollRect.verticalNormalizedPosition = x,
                       1f,
                       smoothScrollDuration)
                   .SetEase(scrollEase)
                   .OnComplete(() => Bounce(-bounceStrength));
        }
        else if (scrollRect.verticalNormalizedPosition < 0f)
        {
            DOTween.To(() => scrollRect.verticalNormalizedPosition,
                       x => scrollRect.verticalNormalizedPosition = x,
                       0f,
                       smoothScrollDuration)
                   .SetEase(scrollEase)
                   .OnComplete(() => Bounce(bounceStrength));
        }
    }

    private void Bounce(float strength)
    {
        scrollRect.verticalNormalizedPosition += strength;
        DOTween.To(() => scrollRect.verticalNormalizedPosition,
                   x => scrollRect.verticalNormalizedPosition = x,
                   Mathf.Clamp(scrollRect.verticalNormalizedPosition, 0f, 1f),
                   smoothScrollDuration * 0.5f)
               .SetEase(Ease.OutBounce);
    }
}
