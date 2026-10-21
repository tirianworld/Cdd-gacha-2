using UnityEngine;
using UnityEngine.Events;

public class HeroStatusEventDispatcher : MonoBehaviour
{
    [Header("Eventos por efecto periódico (Héroe)")]
    public UnityEvent onPoisonTick;
    public UnityEvent onBleedTick;
    public UnityEvent onBurnTick;
    public UnityEvent onShockTick;
    public UnityEvent onCorrosionTick;

    [Header("Eventos por control de estado (Héroe)")]
    public UnityEvent onFreeze;
    public UnityEvent onPetrify;
    public UnityEvent onStun;

    // Métodos para disparar los eventos desde el CombatManager
    public void TriggerPoison() => onPoisonTick?.Invoke();
    public void TriggerBleed() => onBleedTick?.Invoke();
    public void TriggerBurn() => onBurnTick?.Invoke();
    public void TriggerShock() => onShockTick?.Invoke();
    public void TriggerCorrosion() => onCorrosionTick?.Invoke();

    public void TriggerFreeze() => onFreeze?.Invoke();
    public void TriggerPetrify() => onPetrify?.Invoke();
    public void TriggerStun() => onStun?.Invoke();
}