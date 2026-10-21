using UnityEngine;

// Estructura simple para representar un rango num�rico m�nimo/m�ximo.
// (Se usa en vez de Vector2 porque Vector2 se interpreta como posici�n/distancia,
// y aqu� lo que necesitamos es un rango de valores).
[System.Serializable]
public struct FloatRange
{
    public float min;
    public float max;

    public FloatRange(float min, float max)
    {
        this.min = min;
        this.max = max;
    }

    // Si hay sinergia elemental, el m�ximo efectivo se ensancha sum�ndole el m�nimo
    // (ej: 15-35 con sinergia pasa a rifar entre 15 y 50).
    public float GetRandom(bool synergy = false)
    {
        float effectiveMax = synergy ? max + min : max;
        return Random.Range(min, effectiveMax);
    }
}

[CreateAssetMenu(menuName = "SDP/Equipment", fileName = "NewEquipment")]
public class Equipment_SO : Item_SO
{
    // NUEVO: los bonus ahora son un rango (m�nimo/m�ximo) en lugar de un valor fijo.
    // El valor real se calcula al azar dentro de ese rango.
    [Header("Bonus (rango m�nimo - m�ximo)")]
    public FloatRange healthBonusRange = new FloatRange(0f, 0f);
    public FloatRange attackBonusRange = new FloatRange(0f, 0f);
    public FloatRange defenseBonusRange = new FloatRange(0f, 0f);

    public int durability = 100;
    public Sprite elementalAffinityIcon;
    // Cache en memoria (no se serializa) para que el valor aleatorio no cambie
    // cada vez que algo lo consulta durante la partida.
    private float? _cachedHealthBonus;
    private float? _cachedAttackBonus;
    private float? _cachedDefenseBonus;

    // COMPATIBILIDAD: mantenemos los mismos nombres que antes (healthBonus, attackBonus,
    // defenseBonus) como propiedades, para que el resto del c�digo (Hero.cs, EquipmentSlot.cs,
    // BonusEquipmentCalculator.cs, etc.) siga funcionando sin cambios. Cada una devuelve
    // un valor aleatorio dentro de su rango, calculado una sola vez y luego cacheado.
    public float healthBonus
    {
        get
        {
            _cachedHealthBonus ??= healthBonusRange.GetRandom();
            return _cachedHealthBonus.Value;
        }
        set => _cachedHealthBonus = value;
    }

    public float attackBonus
    {
        get
        {
            _cachedAttackBonus ??= attackBonusRange.GetRandom();
            return _cachedAttackBonus.Value;
        }
        set => _cachedAttackBonus = value;
    }

    public float defenseBonus
    {
        get
        {
            _cachedDefenseBonus ??= defenseBonusRange.GetRandom();
            return _cachedDefenseBonus.Value;
        }
        set => _cachedDefenseBonus = value;
    }

    // Fuerza a recalcular los tres bonus (por ejemplo, al obtener una nueva instancia del equipo).
    // Sin par�metro: no aplica sinergia elemental.
    public void RerollBonuses()
    {
        RollBonuses(elementalAffinity, forceNoSynergy: true);
    }

    // NUEVO: tira los bonus teniendo en cuenta la afinidad elemental del h�roe que equipa el �tem.
    // Si "heroAttribute" coincide con "elementalAffinity" del equipo, hay sinergia:
    // el rango se ensancha sum�ndole el m�nimo al m�ximo (ej: 15-35 -> 15-50).
    public void RollBonuses(HeroAttribute heroAttribute, bool forceNoSynergy = false)
    {
        bool synergy = !forceNoSynergy && elementalAffinity == heroAttribute;

        _cachedHealthBonus = healthBonusRange.GetRandom(synergy);
        _cachedAttackBonus = attackBonusRange.GetRandom(synergy);
        _cachedDefenseBonus = defenseBonusRange.GetRandom(synergy);
    }

    // NUEVO: habilidad que otorga este equipo
    public HeroSkill grantedHeroSkill;

    public Type type;
    public enum Type
    {
        anillo,
        sello,
        casco,
        arma,
        armadura
    }

    // NUEVO: afinidad elemental del equipo (mismo enum que usa Hero en "attribute").
    // Arcano, Profano, Natural, Divino.
    [Header("Afinidad elemental")]
    public HeroAttribute elementalAffinity;

    // NUEVO: rango intr�nseco del equipo (independiente de la instancia del jugador)
    public Rango rango = Rango.R;
    public enum Rango
    {
        R,
        SR,
        SSR
    }

    // NUEVO: despertar base del equipo (usado para mostrarlo en cat�logo,
    // cuando no hay una instancia real del jugador)
    public int awaken = 0;
}