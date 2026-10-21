using UnityEngine;
using DG.Tweening;
using AllIn1SpriteShader;

public class ShineDOTween : MonoBehaviour
{
    [Header("Activación general")]
    public bool masterEnable = false;

    [Header("Configuración Shine")]
    public float duration = 1.5f;
    public float delayBetween = 1f;
    public Ease ease = Ease.Linear;
    public bool shinePingPong = false;

    [Header("Vibración (DOTween Pro)")]
    public bool vibrate = false;
    public float vibrateDuration = 0.3f;
    public float vibrateStrength = 0.05f;
    public int vibrateVibrato = 20;

    [Header("Propiedades del Shader")]
    public string shineProperty = "_ShineLocation";
    public string glowProperty = "_Glow";

    private Material mat;
    private Transform cachedTransform;

    private Tween shineTween;
    private Tween vibrateTween;

    private AllIn1Shader shaderComponent;

    private Vector3 originalScale;

    // Estados anteriores para detectar cambios en tiempo real
    private bool prevMaster;
    private bool prevVibrate;
    private bool prevPingPong;

    void Start()
    {
        cachedTransform = transform;
        originalScale = cachedTransform.localScale;

        Renderer r = GetComponent<Renderer>();
        if (r != null) mat = r.material;
        else
        {
            var img = GetComponent<UnityEngine.UI.Graphic>();
            if (img != null) mat = img.material;
        }

        shaderComponent = GetComponent<AllIn1Shader>();

        prevMaster = masterEnable;
        prevVibrate = vibrate;
        prevPingPong = shinePingPong;

        if (masterEnable) ActivarEfectos();
        else DesactivarEfectos();
    }

    void Update()
    {
        // Detecta cambios en tiempo real
        if (masterEnable != prevMaster)
        {
            if (masterEnable) ActivarEfectos();
            else DesactivarEfectos();
        }

        if (masterEnable)
        {
            if (vibrate != prevVibrate)
            {
                if (vibrate) IniciarVibracion();
                else PararVibracion();
            }

            if (shinePingPong != prevPingPong)
            {
                IniciarShine();
            }
        }

        prevMaster = masterEnable;
        prevVibrate = vibrate;
        prevPingPong = shinePingPong;
    }

    // ---------------------------------------------------------
    // EFECTOS
    // ---------------------------------------------------------

    public void ActivarEfectos()
    {
        if (shaderComponent != null)
        {
            shaderComponent.SetKeyword("SHINE_ON", true);
            shaderComponent.SetKeyword("GLOW_ON", true);
        }

        IniciarShine();

        if (vibrate)
            IniciarVibracion();
    }

    public void DesactivarEfectos()
    {
        shineTween?.Kill();
        vibrateTween?.Kill();

        if (mat != null)
        {
            mat.SetFloat(shineProperty, 0f);
            mat.SetFloat(glowProperty, 0f);
        }

        if (shaderComponent != null)
            shaderComponent.ClearAllKeywords();

        cachedTransform.localScale = originalScale;
        cachedTransform.localRotation = Quaternion.identity;
    }

    void IniciarShine()
    {
        shineTween?.Kill();

        mat.SetFloat(shineProperty, 0f);

        Sequence seq = DOTween.Sequence();

        if (shinePingPong)
        {
            seq.Append(DOTween.To(() => mat.GetFloat(shineProperty), x => mat.SetFloat(shineProperty, x), 1f, duration).SetEase(ease))
               .Append(DOTween.To(() => mat.GetFloat(shineProperty), x => mat.SetFloat(shineProperty, x), 0f, duration).SetEase(ease));
        }
        else
        {
            seq.Append(DOTween.To(() => mat.GetFloat(shineProperty), x => mat.SetFloat(shineProperty, x), 1f, duration).SetEase(ease));
        }

        seq.AppendInterval(delayBetween)
           .SetLoops(-1, LoopType.Restart);

        shineTween = seq;
    }

    void IniciarVibracion()
    {
        vibrateTween?.Kill();

        vibrateTween = cachedTransform.DOShakeScale(
            vibrateDuration,
            vibrateStrength * 0.01f,
            vibrateVibrato,
            90,
            false
        ).SetLoops(-1, LoopType.Restart);
    }

    void PararVibracion()
    {
        vibrateTween?.Kill();
        cachedTransform.localScale = originalScale;
    }
}
