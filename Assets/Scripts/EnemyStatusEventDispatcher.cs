using UnityEngine;
using UnityEngine.Events;

public class EnemyStatusEventDispatcher : MonoBehaviour
{
    [Header("Eventos por efecto periódico")]
    public UnityEvent onPoisonTick;
    public UnityEvent onBleedTick;
    public UnityEvent onBurnTick;
    public UnityEvent onShockTick;
    public UnityEvent onCorrosionTick;

    public void TriggerPoison() => onPoisonTick?.Invoke();
    public void TriggerBleed() => onBleedTick?.Invoke();
    public void TriggerBurn() => onBurnTick?.Invoke();
    public void TriggerShock() => onShockTick?.Invoke();
    public void TriggerCorrosion() => onCorrosionTick?.Invoke();
}
