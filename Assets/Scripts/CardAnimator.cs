using UnityEngine;
using System.Collections;

public class CardAnimator : MonoBehaviour
{
    public float moveDuration = 0.25f;
    public float mergeScale = 1.3f;
    public float mergeDuration = 0.15f;

    public IEnumerator MoveTo(RectTransform card, Vector2 targetPos)
    {
        Vector2 start = card.anchoredPosition;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveDuration;
            card.anchoredPosition = Vector2.Lerp(start, targetPos, t);
            yield return null;
        }

        card.anchoredPosition = targetPos;
    }

    public IEnumerator MergeEffect(RectTransform card)
    {
        Vector3 original = card.localScale;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / mergeDuration;
            card.localScale = Vector3.Lerp(original, original * mergeScale, t);
            yield return null;
        }

        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / mergeDuration;
            card.localScale = Vector3.Lerp(original * mergeScale, original, t);
            yield return null;
        }

        card.localScale = original;
    }

    public IEnumerator SpawnEffect(RectTransform card)
    {
        card.localScale = Vector3.zero;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / mergeDuration;
            card.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, t);
            yield return null;
        }

        card.localScale = Vector3.one;
    }
}
