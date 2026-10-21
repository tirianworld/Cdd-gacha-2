using UnityEngine;
using DG.Tweening;

public class CrackReveal_AIOS : MonoBehaviour
{
    public SpriteRenderer crackSprite;

    [Header("Duración de la animación")]
    public float duration = 1.5f;

    [Header("Direcciones")]
    public bool centerToEdges;
    public bool leftToRight;
    public bool rightToLeft;
    public bool topToBottom;
    public bool bottomToTop;

    private MaterialPropertyBlock mpb;

    // Propiedades del AllInOneSpriteShader
    private int fillID;
    private int directionID;
    private int centerSplitID;

    void Awake()
    {
        mpb = new MaterialPropertyBlock();

        fillID = Shader.PropertyToID("_Fill");
        directionID = Shader.PropertyToID("_Direction");
        centerSplitID = Shader.PropertyToID("_CenterSplit");

        crackSprite.GetPropertyBlock(mpb);

        // Inicializamos valores
        mpb.SetFloat(fillID, 0f);
        mpb.SetFloat(centerSplitID, 0f);
        crackSprite.SetPropertyBlock(mpb);
    }

    void Start()
    {
        PlayCrackAnimation();
    }

    public void PlayCrackAnimation()
    {
        crackSprite.GetPropertyBlock(mpb);

        // Dirección según AllInOneSpriteShader
        if (leftToRight) mpb.SetFloat(directionID, 0f);
        if (rightToLeft) mpb.SetFloat(directionID, 1f);
        if (bottomToTop) mpb.SetFloat(directionID, 2f);
        if (topToBottom) mpb.SetFloat(directionID, 3f);

        crackSprite.SetPropertyBlock(mpb);

        if (centerToEdges)
        {
            // Activamos modo centro
            mpb.SetFloat(centerSplitID, 1f);
            crackSprite.SetPropertyBlock(mpb);

            DOTween.To(() => 0f, v => UpdateFill(v), 1f, duration)
                .SetEase(Ease.OutQuad);

            return;
        }

        // Reveal normal por dirección
        mpb.SetFloat(centerSplitID, 0f);
        crackSprite.SetPropertyBlock(mpb);

        float start = 0f;
        float end = 1f;

        // Invertimos si es derecha→izquierda o arriba→abajo
        if (rightToLeft || topToBottom)
        {
            start = 1f;
            end = 0f;
        }

        DOTween.To(() => start, v => UpdateFill(v), end, duration)
            .SetEase(Ease.OutQuad);
    }

    private void UpdateFill(float value)
    {
        crackSprite.GetPropertyBlock(mpb);
        mpb.SetFloat(fillID, value);
        crackSprite.SetPropertyBlock(mpb);
    }
}
