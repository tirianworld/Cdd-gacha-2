using UnityEngine;
using System.Collections.Generic;

public class CombatSpriteController : MonoBehaviour
{
    [Header("Capas")]
    public Transform baseLayer;
    public Transform effectLayer;

    [Header("Base")]
    public GameObject idleGO;
    public GameObject attackGO;
    public GameObject hurtGO;
    public GameObject deadGO;

    [Header("Efectos")]
    public GameObject freezeGO;
    public GameObject petrifyGO;
    public GameObject burnGO;
    public GameObject ShockGO;
    public GameObject tauntGO;
    public GameObject counterGO;
    public GameObject reflectGO;

    [Header("Daño")]
    public GameObject pierceGO;
    public GameObject critGO;
    public GameObject floodGO;
    public GameObject ruptureGO;
    public GameObject spikeGO;
    [Header("Duraciones")]
    public float idleReturnTime = 0.3f;
    public float attackReturnTime = 0.6f;
    public float hurtReturnTime = 0.5f;
    public float deadLockTime = 999f; // No vuelve a idle
    public float damageEffectTime = 0.5f;
    [Header("Skills")]
    public GameObject[] skillGO; // Un GO por skill
    public float skillReturnTime = 0.6f;

    public bool overrideEffects = false;

    public Hero hero;
    public Enemy enemy;
    // Tracks if a skill animation is currently playing
    private bool skillInProgress = false;

    void Start()
    {
        // Si este objeto tiene Hero, borra cualquier referencia heredada
        if (hero == null)
            print("hEROE nULO");
        else if(hero != null)
        {
            print("es " + hero.name);
        }

        

        HideAll(baseLayer);
        HideAll(effectLayer);
        ShowBase(idleGO);
    }


    void Update()
    {
        RefreshSprite();
    }

    // ============================================================
    // UTILIDADES
    // ============================================================
    public void HideAll(Transform layer)
    {
        foreach (Transform child in layer)
            child.gameObject.SetActive(false);
    }

    void ShowBase(GameObject go)
    {
        foreach (Transform child in baseLayer)
            child.gameObject.SetActive(child.gameObject == go);
    }

    void ShowEffect(GameObject go)
    {
        HideAll(effectLayer);
        if (go != null) go.SetActive(true);
    }
    public void ShowSkill(int index)
    {
        // Guard against invalid indices early and log helpful info.
        if (index < 0 || index >= skillGO.Length)
        {
            Debug.LogWarning($"ShowSkill called with invalid index {index}. Range: 0-{skillGO.Length - 1}");
            return;
        }
        if (skillGO[index] == null)
        {
            Debug.LogWarning($"Skill GO at index {index} is null.");
            return;
        }

        // Cancel any pending ReturnToIdle from a previous skill/animation.
        CancelInvoke(nameof(ReturnToIdle));
        CancelInvoke(nameof(EndSkill));

        overrideEffects = true;
        skillInProgress = true;

        // Ocultar todo
        HideAll(baseLayer);
        HideAll(effectLayer);

        // Activar solo el GO de la skill
        skillGO[index].SetActive(true);

        // Volver a Idle después de que la animación termine.
        Invoke(nameof(EndSkill), skillReturnTime);
    }

    // ============================================================
    // ANIMACIONES BASE
    // ============================================================
    public void ShowIdle()
    {
        overrideEffects = true;
        ShowBase(idleGO);
        Invoke(nameof(AllowEffects), idleReturnTime);
    }


    public void ShowAttack()
    {
        overrideEffects = true;
        ShowBase(attackGO);
        HideAll(effectLayer);

        Invoke(nameof(ReturnToIdle), attackReturnTime);
    }

    public void ShowHurt()
    {
        overrideEffects = true;
        ShowBase(hurtGO);
        HideAll(effectLayer);

        Invoke(nameof(ReturnToIdle), hurtReturnTime);
    }


    public void ShowDead()
    {
        overrideEffects = true;
        ShowBase(deadGO);
        HideAll(effectLayer);

        // deadLockTime evita que vuelva a Idle
        Invoke(nameof(ReturnToIdle), deadLockTime);
    }


    void ReturnToIdle()
    {
        ShowBase(idleGO);
        overrideEffects = false;
    }

    // Called by Invoke after a skill animation finishes.
    private void EndSkill()
    {
        skillInProgress = false;
        ReturnToIdle();
    }

    // ============================================================
    // EFECTOS
    // ============================================================
    void ApplyEffectSprite(List<StatusEffect> effects)
    {
        if (effects == null || effects.Count == 0)
        {
            HideAll(effectLayer);
            return;
        }

        if (effects.Exists(e => e.type == StatusEffectType.Freeze)) { ShowEffect(freezeGO); return; }
        if (effects.Exists(e => e.type == StatusEffectType.Petrify)) { ShowEffect(petrifyGO); return; }
        if (effects.Exists(e => e.type == StatusEffectType.Burn)) { ShowEffect(burnGO); return; }
        if (effects.Exists(e => e.type == StatusEffectType.Shock)) { ShowEffect(ShockGO); return; }
        if (effects.Exists(e => e.taunt)) { ShowEffect(tauntGO); return; }
        if (effects.Exists(e => e.counterAttack)) { ShowEffect(counterGO); return; }
        if (effects.Exists(e => e.reflectDamage)) { ShowEffect(reflectGO); return; }

        HideAll(effectLayer);
    }

    // ============================================================
    // EFECTOS DE DAÑO
    // ============================================================
    public void ShowDamageSprite(DamageEffectDefinition dmg)
    {
        overrideEffects = true;

        switch (dmg.damageType)
        {
            case DamageEffectDefinition.DamageType.Pierce: ShowEffect(pierceGO); break;
            case DamageEffectDefinition.DamageType.Crit: ShowEffect(critGO); break;
            case DamageEffectDefinition.DamageType.Flood: ShowEffect(floodGO); break;
            case DamageEffectDefinition.DamageType.Rupture: ShowEffect(ruptureGO); break;
            case DamageEffectDefinition.DamageType.Spike: ShowEffect(spikeGO); break;
            default: HideAll(effectLayer); break;
        }

        Invoke(nameof(AllowEffects), damageEffectTime);
    }


    // ============================================================
    // CONTROL DE PRIORIDAD
    // ============================================================
    public void AllowEffects()
    {
        // If a skill animation is still active we keep the lock.
        if (skillInProgress)
            return;

        overrideEffects = false;
        RefreshSprite();
    }

    public void RefreshSprite()
    {
        if (overrideEffects)
            return;

        if (hero != null && hero.stats.currentHealth <= 0)
        {
            ShowBase(deadGO);
            HideAll(effectLayer);
            return;
        }

        if (enemy != null && enemy.stats.currentHealth <= 0)
        {
            ShowBase(deadGO);
            HideAll(effectLayer);
            return;
        }

        if (hero != null && hero.activeEffects.Count > 0)
        {
            ApplyEffectSprite(hero.activeEffects);
        }
        else if (enemy != null && enemy.activeEffects.Count > 0)
        {
            ApplyEffectSprite(enemy.activeEffects);
        }
        else
        {
            HideAll(effectLayer);
        }
    }
}
