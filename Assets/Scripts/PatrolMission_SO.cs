using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "SDP/PatrolMission", fileName = "NewPatrolMission")]
public class PatrolMission_SO : ScriptableObject
{
    [Header("Identificación")]
    [Tooltip("ID único de esta misión (no se repite entre misiones). Se usa para guardar datos en PlayerPrefs.")]
    public int missionID;

    [Header("Datos básicos")]
    public string missionName = "Patrol";
    [TextArea] public string description = "";

    [Header("Requisitos")]
    [Tooltip("Número de héroes requeridos para iniciar la misión.")]
    public int requiredTeamSize = 3;

    [Tooltip("Rareza mínima requerida (por ejemplo, estrellas o rango).")]
    public int minRequiredRarityStars = 0;

    [Tooltip("Dificultad base de la misión.")]
    public int baseDifficulty = 1;

    [Header("Duración y recompensas")]
    [Tooltip("Duración base en segundos (por defecto 1 hora).")]
    public float durationSeconds = 3600f; // 1 hora

    [Tooltip("Recompensa base en oro.")]
    public int baseGoldReward = 100;

    [Tooltip("Recompensa base en experiencia.")]
    public int baseXpReward = 50;

    [Tooltip("Coste de stamina por héroe asignado.")]
    public float staminaCostPerHero = 20f;

    [Header("Objetos obtenibles")]
    [Tooltip("Probabilidad (0..1) de encontrar un ítem al completar la misión.")]
    [Range(0f, 1f)] public float findItemChance = 0.25f;

    [Tooltip("Posibles recompensas que se pueden encontrar durante la misión.")]
    public Item_SO[] possibleFinds;
    public List<Hero> unlockableHeroes = new List<Hero>();

    [Header("Parámetros específicos de misión")]
    public List<HeroClass> requiredClasses = new List<HeroClass>();
    public CombatClassLevel missionCombatClass;

    [Header("Afinidades óptimas")]
    public List<HeroRace> optimalRaces = new List<HeroRace>();
    public List<HeroClass> optimalClasses = new List<HeroClass>();
    public List<HeroReligion> optimalReligions = new List<HeroReligion>();

    [Header("Requisitos estrictos para iniciar")]
    public List<HeroRace> requiredRaces = new List<HeroRace>();
    public List<HeroClass> requiredClassesStrict = new List<HeroClass>();
    public List<HeroReligion> requiredReligions = new List<HeroReligion>();

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (missionID == 0)
        {
            missionID = Mathf.Abs(GetInstanceID());
            UnityEditor.EditorUtility.SetDirty(this);
        }
    }
#endif
}
