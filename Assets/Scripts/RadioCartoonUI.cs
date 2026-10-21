using UnityEngine;
using DG.Tweening;

public class SpriteBounceAAA : MonoBehaviour
{
    public RectTransform target;

    void Start()
    {
        if (target == null)
            target = GetComponent<RectTransform>();

        // Secuencia AAA
        Sequence seq = DOTween.Sequence();

        // 1. Anticipación (pequeño "pre-squash")
        seq.Append(
            target.DOScale(new Vector3(0.95f, 1.05f, 1f), 0.12f)
                  .SetEase(Ease.OutQuad)
        );

        // 2. Rebote principal + giro musical
        seq.Append(
            target.DOScale(new Vector3(1.12f, 0.88f, 1f), 0.22f)
                  .SetEase(Ease.OutBack)
        );

        seq.Join(
            target.DORotate(new Vector3(0, 0, 12f), 0.22f)
                  .SetEase(Ease.InOutSine)
        );

        // 3. Vuelta suave al estado base
        seq.Append(
            target.DOScale(Vector3.one, 0.25f)
                  .SetEase(Ease.OutElastic, 0.6f)
        );

        seq.Join(
            target.DORotate(Vector3.zero, 0.25f)
                  .SetEase(Ease.OutQuad)
        );

        // 4. Movimiento vertical ultra fluido
        seq.Join(
            target.DOAnchorPosY(target.anchoredPosition.y + 22f, 0.45f)
                  .SetEase(Ease.InOutSine)
        );

        // Loop infinito estilo musical
        seq.SetLoops(-1, LoopType.Restart);
    }
}
