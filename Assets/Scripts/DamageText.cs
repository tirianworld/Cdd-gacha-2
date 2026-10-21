using UnityEngine;
using TMPro;
using DG.Tweening;

public class DamageText2D : MonoBehaviour
{
    public TextMeshProUGUI text;
    public float duration = 1f;

    public void Play(float amount, Color color)
    {
        gameObject.SetActive(true);

        if (text == null)
            text = GetComponent<TextMeshProUGUI>();

        text.text = Mathf.RoundToInt(amount).ToString();
        text.color = color;
        text.alpha = 1f;

        text.DOKill();

        // Solo fade, sin mover transform
        text.DOFade(0f, duration);

        DOVirtual.DelayedCall(duration, () => gameObject.SetActive(false));
    }
}
