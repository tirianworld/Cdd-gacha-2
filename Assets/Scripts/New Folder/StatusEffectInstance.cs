[System.Serializable]
public class StatusEffectInstance
{
    public StatusEffectDefinition def;
    public int remainingTurns;

    public StatusEffectInstance(StatusEffectDefinition definition)
    {
        def = definition;
        remainingTurns = def.durationTurns;
    }
}
