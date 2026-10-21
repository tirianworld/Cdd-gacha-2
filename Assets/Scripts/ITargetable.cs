// File: ITargetable.cs
public interface ITargetable
{
    bool IsAlive { get; }
    void ApplyDamage(float amount);
    void RestoreHealth(float amount);
}
