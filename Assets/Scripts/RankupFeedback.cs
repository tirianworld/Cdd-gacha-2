using UnityEngine;
using DG.Tweening;

public class RankupFeedback : MonoBehaviour
{
    [Header("Shader Properties")]
    public string glowPowerProp = "_GlowPower";
    public string glowColorProp = "_GlowColor";
    public string shineProp = "_ShineLocation";

    [Header("Feedback Settings")]
    public Color glowColor = new Color(1f, 0.3f, 0.7f, 1f); // Rosado
    public float glowMax = 1.2f;
    public float glowMin = 0.3f;
    public float shineDuration = 1.2f;
    public float delayBetweenShines = 0.5f;

    [Header("Particles")]
    public GameObject pinkDustPrefab;
    private GameObject dustInstance;

    private Material mat;
    private Tween glowTween;
    private Tween shineTween;

    void Awake()
    {
        var img = GetComponent<UnityEngine.UI.Image>();
        if (img != null)
        {
            mat = Instantiate(img.material);
            img.material = mat;
        }
    }

    public void PlayFeedback()
    {
        if (mat == null) return;

        // Color inicial
        mat.SetColor(glowColorProp, glowColor);
        mat.SetFloat(glowPowerProp, glowMin);

        glowTween?.Kill();
        shineTween?.Kill();

        // 🔥 Glow pulsante
        glowTween = DOTween.To(
            () => mat.GetFloat(glowPowerProp),
            x => mat.SetFloat(glowPowerProp, x),
            glowMax,
            0.6f
        )
        .SetLoops(-1, LoopType.Yoyo)
        .SetEase(Ease.InOutQuad);

        // ✨ Shine deslizante
        shineTween = DOTween.Sequence()
            .Append(mat.DOFloat(1f, shineProp, shineDuration).SetEase(Ease.InOutQuad))
            .AppendInterval(delayBetweenShines)
            .SetLoops(-1, LoopType.Restart);

        // 🌸 Partículas rosadas
        if (pinkDustPrefab != null && dustInstance == null)
        {
            dustInstance = Instantiate(pinkDustPrefab, transform);
            dustInstance.transform.localPosition = Vector3.zero;
        }
    }

    public void StopFeedback()
    {
        glowTween?.Kill();
        shineTween?.Kill();

        if (mat != null)
            mat.DOFloat(0f, glowPowerProp, 0.25f);

        if (dustInstance != null)
            Destroy(dustInstance);
    }
}
