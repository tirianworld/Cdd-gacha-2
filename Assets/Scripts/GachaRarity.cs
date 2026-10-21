using UnityEngine;

public enum GachaRarity
{
    R,
    SR,
    SSR
}

[System.Serializable]
public class GachaHeroEntry
{
    [Header("Referencia al héroe en la cuenta")]
    public int heroID;            // Debe coincidir con Hero.heroID en tu roster
    public GachaRarity rarity = GachaRarity.R;

    [Range(0f, 100f)]
    public float weight = 1f;     // Peso relativo dentro de su raridad

    [Header("Rate-up")]
    public bool isRateUp = false;
}

[System.Serializable]
public struct GachaPullResult
{
    public Hero hero;             // Héroe de la cuenta (instancia del roster)
    public GachaRarity rarity;
    public bool isNew;
    public bool isRateUp;
    public bool triggeredPity;
}
