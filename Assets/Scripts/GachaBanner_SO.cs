using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "NewGachaBanner", menuName = "Gacha/Gacha Banner")]
public class GachaBanner_SO : ScriptableObject
{
    [Header("Identidad del banner")]
    public string bannerId = "default_banner";
    public string bannerName = "Banner estándar";

    [Header("Costes (Cristales del Recuerdo)")]
    public int costSingle = 3;   // 3 MemoryCristals
    public int costMulti = 30;   // 30 MemoryCristals (10+1)

    [Header("Invocaciones tipo 7DS")]
    public int multiSize = 11;
    public bool guaranteeSRInMulti = true;

    [Header("Pity tipo 7DS (pasos)")]
    public int softPitySteps = 300;   // a 300 pasos: SSR garantizado
    public int hardPitySteps = 600;   // a 600 pasos: SSR rate-up garantizado
    // Cada tirada = 1 paso. (Si quieres, luego puedes hacer que solo multis den más pasos)

    [Header("Pool de héroes")]
    public List<GachaHeroEntry> heroes = new List<GachaHeroEntry>();

    [Header("Ratios por rareza (suma 100)")]
    [Range(0f, 100f)] public float rateR = 60f;
    [Range(0f, 100f)] public float rateSR = 35f;
    [Range(0f, 100f)] public float rateSSR = 5f;

    [Header("Rate-Up SSR")]
    public int rateUpHeroID = -1;
    [Range(0f, 1f)]
    public float rateUpSSROverride = 0.5f; // probabilidad de que un SSR normal sea el rate-up
    [Header("Rotación tipo 7DS")]
    public GachaRotation_SO rotation;
    public bool useRotation = false;

}
