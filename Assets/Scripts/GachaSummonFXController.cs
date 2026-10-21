using UnityEngine;

public class GachaSummonFXController : MonoBehaviour
{
    public GachaManager gacha;
    public Animator summonAnimator;
    public GameObject vfxRoot; // partículas, luz, etc.

    private void OnEnable()
    {
        if (gacha == null) gacha = GachaManager.Instance;
        if (gacha != null)
        {
            gacha.OnSummonStarted += HandleSummonStarted;
            gacha.OnSummonFinished += HandleSummonFinished;
        }
    }

    private void OnDisable()
    {
        if (gacha != null)
        {
            gacha.OnSummonStarted -= HandleSummonStarted;
            gacha.OnSummonFinished -= HandleSummonFinished;
        }
    }

    private void HandleSummonStarted()
    {
        if (vfxRoot != null) vfxRoot.SetActive(true);
        if (summonAnimator != null) summonAnimator.SetTrigger("StartSummon");
    }

    private void HandleSummonFinished(GachaPullResult[] results)
    {
        // Aquí puedes:
        // - hacer fade del FX
        // - disparar transición a la pantalla de cartas
        if (summonAnimator != null) summonAnimator.SetTrigger("EndSummon");
    }

    // Llamado desde un AnimationEvent al final de la cinemática
    public void OnSummonCinematicFinished()
    {
        // Podrías desactivar VFX y mostrar directamente las cartas:
        if (vfxRoot != null) vfxRoot.SetActive(false);
    }
}
