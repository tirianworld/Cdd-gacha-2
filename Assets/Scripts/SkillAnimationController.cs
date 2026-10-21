using UnityEngine;
using UnityEngine.Events;

public class SkillAnimationController : MonoBehaviour
{
    public Animator animator;

    // Evento que se dispara al terminar la animación
    public UnityEvent OnAnimationEnd;

    // Permite encadenar otra animación
    private string nextAnimationTrigger;

    public void PlaySkillAnimation(string trigger, UnityAction onEnd, string chainedTrigger = null)
    {
        if (animator == null) return;

        nextAnimationTrigger = chainedTrigger;

        // Suscribir callback
        OnAnimationEnd.RemoveAllListeners();
        if (onEnd != null) OnAnimationEnd.AddListener(onEnd);

        animator.SetTrigger(trigger);
    }

    // Este método lo llamas desde un AnimationEvent al final del clip
    public void AnimationFinished()
    {
        // Ejecuta la lógica
        OnAnimationEnd?.Invoke();

        // Si hay animación encadenada, lánzala
        if (!string.IsNullOrEmpty(nextAnimationTrigger))
        {
            animator.SetTrigger(nextAnimationTrigger);
            nextAnimationTrigger = null;
        }
    }
}
