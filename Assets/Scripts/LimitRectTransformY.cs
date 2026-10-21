using UnityEngine;
using UnityEngine.UI;

public class ScrollClampY : MonoBehaviour
{
    public ScrollRect scrollRect;
    public float minNormalized = 0.2f; // mínimo scroll en Y
    public float maxNormalized = 0.8f; // máximo scroll en Y

    void Start()
    {
        scrollRect.onValueChanged.AddListener(OnScroll);
        // Desactiva el scroll horizontal por seguridad
        scrollRect.horizontal = false;
    }

    void OnScroll(Vector2 pos)
    {
        // Bloquear X en 0 y limitar solo Y
        float clampedY = Mathf.Clamp(pos.y, minNormalized, maxNormalized);
        scrollRect.normalizedPosition = new Vector2(0f, clampedY);
    }
}
