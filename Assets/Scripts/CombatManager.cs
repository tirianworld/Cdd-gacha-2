using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;
using System.Collections.Generic;
public enum DamageType
{
    Normal,
    Fire,
    Cold,
    Lightning,
    Thunder,
    Acid,
    Poison,
    Psychic,
    Radiant,
    Necrotic,
    Force,
    Slashing,
    Piercing,
    Bludgeoning
}
public class DamageTypeEffects
{
    public bool healingBlocked;      // Necrotic
    public float healingBlockedAmount;

    public float defenseReduction;   // Radiant
    public int defenseReductionTurns;

    public float damageReduction;    // Psychic
    public int damageReductionTurns;

    public float vulnerabilityBonus; // Fire, Cold, etc.
    public int vulnerabilityTurns;
}


public class DamageOverride
{
    public DamageType type = DamageType.Normal;
    public int remainingTurns = 0;
}

[System.Serializable]

public class EnemyStatus
{
    public int poisonTurns;
    public float poisonDamage;

    public int bleedTurns;
    public float bleedDamage;

    public int burnTurns;
    public float burnDamage;

    public int shockStacks;
    public float shockPercent;
    public int shockTurns;

    public bool frozen;
    public int freezeTurns;

    public bool petrified;
    public int petrifyTurns;

    public bool stunned;
    public int stunTurns;

    public int corrosionTurns;
    public float corrosionPercent;

    public bool webbed;
    public int webTurns;

    public bool sealedSkillActive = false;
    public int sealedTurns = 0;
    public HeroSkill sealedSkill = null;

}
[System.Serializable]
public class HeroStatus
{
    public int poisonTurns;
    public float poisonDamage;

    public int bleedTurns;
    public float bleedDamage;

    public int burnTurns;
    public float burnDamage;

    public int shockStacks;
    public float shockPercent;
    public int shockTurns;

    public bool frozen;
    public int freezeTurns;

    public bool petrified;
    public int petrifyTurns;

    public bool stunned;
    public int stunTurns;

    public int corrosionTurns;
    public float corrosionPercent;

    public bool webbed;
    public int webTurns;

    public bool stealth = false;
    public int stealthTurns = 0;

    public bool fullCounter = false;
    public int fullCounterTurns = 0;
    public float fullCounterDamageAccum = 0f;
    public int fullCounterTier = 1; // 1, 2 o 3


}


public class CombatManager : MonoBehaviour
{
    public Hero hero;
    public HeroVisual herovisual;
    public Enemy enemy;
    public HeroVisual[] heroVisuals;
    public HandManager handManager;
    public TextMeshProUGUI dialogue;
    public GameObject[] skillButtons;
    public DamageOverride damageOverride = new DamageOverride();
    public DamageTypeEffects damageTypeEffects = new DamageTypeEffects();

    private enum BattleState { Start, PlayerTurn, PlayerChoosingSkill, EnemyTurn, Busy, Won, Lost }
    private BattleState state;

    [Header("Sprites de combate")]
    public SpriteRenderer heroCombatSprite;
    public SpriteRenderer enemyCombatSprite;
    public CombatSpriteController heroSpriteController;
    public CombatSpriteController enemySpriteController;
    [Header("Puntos de spawn de efectos")]
    public Transform enemyEffectPoint;
    public Transform playerEffectPoint;
    public Transform playerBuffPoint;
    public GameObject damageTextPrefab;
    public Transform enemyDamageTextPool;
    public Transform heroDamageTextPool;

    [Header("Status")]
    public GameObject iconPoison;
    public GameObject iconBleed;
    public GameObject iconBurn;
    public GameObject iconShock;
    public GameObject iconFreeze;
    public GameObject iconPetrify;
    public GameObject iconStun;
    public GameObject iconCorrosion;
    public GameObject iconWeb;
    public EnemyStatusEventDispatcher statusEvents;
    [Header("Status HÃƒÂ©roe")]
    public GameObject heroIconPoison;
    public GameObject heroIconBleed;
    public GameObject heroIconBurn;
    public GameObject heroIconShock;
    public GameObject heroIconFreeze;
    public GameObject heroIconPetrify;
    public GameObject heroIconStun;
    public GameObject heroIconCorrosion;
    public GameObject heroIconWeb;
    public HeroStatusEventDispatcher heroStatusEvents;
    public HeroSkillEventDispatcher currentDispatcher;

    [Header("Ámbito de este combate")]
    public Transform combatRoot; // Asignar en el Inspector = "Combat Scene" de ESTE combate
                                 // (el padre común de Personajes, enemigos, Canvas, etc.)

    private void Awake()
    {
        if (combatRoot == null)
            combatRoot = transform.parent; // fallback: COMBAT MANAGER vive bajo "Combat Scene"

        var assign = FindObjectOfType<HeroAssignmentManager>();
        heroAssignmentManager = assign;

        // 🔧 antes: FindObjectsOfType<HeroVisual>(true) traía TODOS los combates
        heroVisuals = combatRoot.GetComponentsInChildren<HeroVisual>(true);

        SetHero(assign.ActiveHero);
        hero = assign.ActiveHero;
    }

    // 🔧 FIX: referencia cacheada al HeroAssignmentManager, usada en Update()
    // para mantener `hero` sincronizado con la fuente de verdad. Antes,
    // si algo (p.ej. HeroSwitcher) cambiaba el héroe activo sin llamar a
    // SetHero() en CombatManager, `hero` quedaba obsoleto (o null) y
    // HeroSkillEventDispatcher.TriggerSkillEvent() rechazaba silenciosamente
    // todos los eventos de skill por el chequeo `hero != combatManager.hero`.
    private HeroAssignmentManager heroAssignmentManager;

    public Canvas worldCanvas; // Canvas en World Space


    public void ShowDamageOnEnemy(float dmg)
    {
        var dt = GetFreeDamageText(enemyDamageTextPool);
        if (dt == null) return;

        dt.transform.SetAsLastSibling(); // Siempre encima
        dt.Play(dmg, Color.red);
    }
    void SetHero(Hero newHero)
    {
        hero = newHero;
        StartHero();
        RefreshHeroStatusUI();

        // Nada ms. El dispatcher ya funciona solo.
    }



    public void ShowDamageOnHero(float dmg)
    {
        var dt = GetFreeDamageText(heroDamageTextPool);
        if (dt == null) return;

        dt.transform.SetAsLastSibling();
        dt.Play(dmg, Color.yellow);
    }



    public void UpdateHeroStatusIcons()
    {
        if (hero == null || hero.status == null) return;

        heroIconPoison?.SetActive(hero.status.poisonTurns > 0);
        heroIconBleed?.SetActive(hero.status.bleedTurns > 0);
        heroIconBurn?.SetActive(hero.status.burnTurns > 0);
        heroIconShock?.SetActive(hero.status.shockTurns > 0);
        heroIconFreeze?.SetActive(hero.status.frozen);
        heroIconPetrify?.SetActive(hero.status.petrified);
        heroIconStun?.SetActive(hero.status.stunned);
        heroIconCorrosion?.SetActive(hero.status.corrosionTurns > 0);
        heroIconWeb?.SetActive(hero.status.webbed); // NUEVO
    }

    public void RefreshHeroStatusUI()
    {
        UpdateHeroStatusIcons();
        if (hero != null && heroStatusContainer != null && hero.status != null)
        {
            RefreshStatusIcons(heroStatusContainer, hero.status);
        }
    }
    public Transform heroStatusContainer;
    public Transform enemyStatusContainer;
    public GameObject statusIconPrefab;




    private DamageText2D GetFreeDamageText(Transform pool)
    {
        foreach (Transform child in pool)
        {
            if (!child.gameObject.activeSelf)
                return child.GetComponent<DamageText2D>();
        }

        return null; // Si quieres, aquÃƒÂ­ puedes expandir el pool
    }




    IEnumerator StartHero()
    {
        yield return new WaitUntil(() => herovisual.hero != null);

        SetHero(herovisual.hero);

        currentDispatcher = ResolveDispatcherFor(herovisual);
    }

    private void Start()
    {
        var assign = FindObjectOfType<HeroAssignmentManager>();
        SetHero(assign.ActiveHero);
        hero = assign.ActiveHero;

        handManager = FindObjectOfType<HandManager>();
        handManager.hero = hero;
        handManager.combat = this;

        SetupSkillButtons();
        StartCoroutine(StartBattle());


    }


    private void Update()
    {
        // 🔧 FIX (corrección): el resync anterior pisaba cualquier cambio de
        // héroe hecho por otros sistemas (p.ej. HeroSwitcher), porque
        // forzaba `hero` de vuelta a heroAssignmentManager.ActiveHero en
        // CADA frame. Ahora solo actúa como red de seguridad cuando `hero`
        // quedó en null (el bug original), sin interferir con cambios
        // válidos ya aplicados por otro script.
        if (hero == null)
        {
            if (heroAssignmentManager == null)
                heroAssignmentManager = FindObjectOfType<HeroAssignmentManager>();

            if (heroAssignmentManager != null && heroAssignmentManager.ActiveHero != null)
            {
                Debug.LogWarning("[CombatManager] hero era null; se restauró desde HeroAssignmentManager.ActiveHero.");
                hero = heroAssignmentManager.ActiveHero;
            }
        }

        foreach (var hv in heroVisuals)
        {
            if (hv != null)
            {
                // Try to get the Hero reference either from the field or the attached component
                var hvHero = hv.hero != null ? hv.hero : hv.GetComponent<Hero>();
                if (hvHero == hero)
                {
                    herovisual = hv;
                    herovisual.gameObject.SetActive(true);
                    heroStatusEvents = hv.gameObject.GetComponent<HeroStatusEventDispatcher>();
                }
                else
                {
                    hv.gameObject.SetActive(false);
                }
            }
        }

        // If still null, try to assign the first available visual as a fallback
        if (herovisual == null && heroVisuals.Length > 0)
        {
            herovisual = heroVisuals[0];
            herovisual.gameObject.SetActive(true);
            heroStatusEvents = herovisual.gameObject.GetComponent<HeroStatusEventDispatcher>();
            Debug.LogWarning("Fallback: assigned first HeroVisual as no matching visual found.");
        }

        if (herovisual == null)
        {
            Debug.LogError("No se encontró un HeroVisual que coincida con el Hero activo.");
            return;
        }

        heroSpriteController = herovisual.spriteController;
        if (herovisual != null)
        {
            currentDispatcher = ResolveDispatcherFor(herovisual);
        };
    }

    // ============================================================
    // 🔧 FIX: búsqueda robusta del dispatcher de skills
    // ============================================================
    // Antes se usaba GetComponent<HeroSkillEventDispatcher>() sobre
    // herovisual.gameObject, lo cual falla en silencio si el
    // dispatcher está en un hijo del objeto (o en otra jerarquía),
    // dejando currentDispatcher en null sin ningún log.
    // Esto es lo que hacía que TriggerSkillEvent nunca se llamara
    // para Arlem: currentDispatcher?.TriggerSkillEvent(...) no
    // hace nada si currentDispatcher es null.
    private HeroSkillEventDispatcher ResolveDispatcherFor(HeroVisual hv)
    {
        if (hv == null) return null;

        // 1) Buscar en el propio objeto o en sus hijos (caso más común)
        var dispatcher = hv.GetComponentInChildren<HeroSkillEventDispatcher>(true);

        // 2) Si no está ahí, buscar en el padre por si el dispatcher
        //    vive un nivel arriba en la jerarquía del héroe
        if (dispatcher == null)
            dispatcher = hv.GetComponentInParent<HeroSkillEventDispatcher>();

        // 3) Último recurso: buscar entre TODOS los dispatchers de la
        //    escena el que tenga el mismo Hero asignado que hv.hero
        if (dispatcher == null && hv.hero != null)
        {
            // 🔧 antes: FindObjectsOfType<HeroSkillEventDispatcher>(true) buscaba
            // en TODOS los combates de la escena, no solo en este.
            var allDispatchers = combatRoot.GetComponentsInChildren<HeroSkillEventDispatcher>(true);
            foreach (var d in allDispatchers)
            {
                if (d.hero == hv.hero)
                {
                    dispatcher = d;
                    break;
                }
            }
        }

        if (dispatcher == null)
        {
            Debug.LogError($"[CombatManager] No se encontró HeroSkillEventDispatcher para '{hv.name}' " +
                            $"(hero={hv.hero?.heroName ?? "null"}). Revisa que el componente exista en su jerarquía.");
        }

        return dispatcher;
    }



    IEnumerator StartBattle()
    {
        state = BattleState.Start;

        var hv = hero != null ? hero.GetComponent<HeroVisual>() : null; heroSpriteController = hv != null ? hv.spriteController : (herovisual != null ? herovisual.spriteController : null);
        if (enemyCombatSprite != null) enemySpriteController = enemyCombatSprite.GetComponent<CombatSpriteController>();

        heroCombatSprite.sprite = hero.combatSprite != null ? hero.combatSprite : hero.portrait;

        yield return new WaitForSeconds(1.2f);

        state = BattleState.PlayerTurn;
        PlayerTurn();
    }

    public void SetEnemy(Enemy newEnemy)
    {
        enemy = newEnemy;
        newEnemy.combatManager = this;

        enemySpriteController = newEnemy.GetComponent<CombatSpriteController>();
        enemySpriteController.enemy = newEnemy;
        statusEvents = enemy.GetComponent<EnemyStatusEventDispatcher>();

        // Reiniciar animaciones
        enemySpriteController.overrideEffects = false;
        enemySpriteController.HideAll(enemySpriteController.baseLayer);
        enemySpriteController.HideAll(enemySpriteController.effectLayer);
        enemySpriteController.ShowIdle();

        // Ã°Å¸â€Â¥ REINICIAR COMBATE
        state = BattleState.PlayerTurn;
        handManager.actionsLeft = 1;
        handManager.GenerateInitialHand();
        handManager.RefreshHandUI();

        dialogue.text = $"{enemy.enemyName} aparece!";
        PlayerTurn();

        Debug.Log($"Nuevo enemigo asignado: {newEnemy.enemyName}");
    }




    void SetupSkillButtons()
    {
        for (int i = 0; i < skillButtons.Length; i++)
        {
            if (i < hero.skills.Count)
            {
                skillButtons[i].gameObject.SetActive(true);

                var iconImage = skillButtons[i].GetComponentInChildren<Image>();
                if (iconImage != null && hero.skills[i].icon != null)
                    iconImage.sprite = hero.skills[i].icon;
            }
            else
            {
                skillButtons[i].gameObject.SetActive(false);
            }
        }
    }

    // ============================================================
    // EFECTOS VISUALES
    // ============================================================

    public void SpawnSkillVisualsOnEnemy(HeroSkill skill, int tier)
    {
        if (skill.visualPrefabs == null || skill.visualPrefabs.Count == 0)
            return;

        int index = Mathf.Clamp(tier - 1, 0, skill.visualPrefabs.Count - 1);
        var prefab = skill.visualPrefabs[index];
        if (prefab == null) return;

        Transform spawnPoint = enemyEffectPoint != null ? enemyEffectPoint : enemyCombatSprite.transform;
        Instantiate(prefab, spawnPoint.position, Quaternion.identity, spawnPoint);
    }

    public void SpawnSkillVisualsOnPlayer(HeroSkill skill)
    {
        if (skill.visualPrefabs == null || skill.visualPrefabs.Count == 0)
            return;

        var prefab = skill.visualPrefabs[0];
        if (prefab == null) return;

        Transform spawnPoint = playerEffectPoint != null ? playerEffectPoint : heroCombatSprite.transform;
        Instantiate(prefab, spawnPoint.position, Quaternion.identity, spawnPoint);
    }

    public void SpawnBuffVisualsOnPlayer(GameObject prefab)
    {
        if (prefab == null) return;

        Transform spawnPoint = playerBuffPoint != null ? playerBuffPoint : heroCombatSprite.transform;
        Instantiate(prefab, spawnPoint.position, Quaternion.identity, spawnPoint);
    }

    // ============================================================
    // PLAYER TURN
    // ============================================================

    void PlayerTurn()
    {
        if (state != BattleState.PlayerTurn)
            return;

        dialogue.text = "";

        // ============================================================
        // Ã°Å¸â€Â¥ 1. EFECTOS DE DAÃƒâ€˜O SOBRE EL HÃƒâ€°ROE (DOTs)
        // ============================================================


        // ============================================================
        // Ã°Å¸â€Â¥ 2. EFECTOS DE CONTROL SOBRE EL HÃƒâ€°ROE
        // ============================================================
        if (CheckFreeze_Hero() || CheckPetrify_Hero() || CheckStun_Hero() || CheckWeb_Hero())
        {
            hero.skipNextTurn = false;
            dialogue.text += $"\n{hero.heroName} no puede actuar este turno!";
            StartCoroutine(EnemyTurn());
            return;
        }

        // ============================================================
        // Ã°Å¸â€Â¥ 3. EFECTOS INTERNOS DEL HÃƒâ€°ROE (BUFFS/DEBUFFS)
        // ============================================================


        // ============================================================
        // Ã°Å¸â€Â¥ 4. ACTUALIZAR ICONOS (si tienes iconos del hÃƒÂ©roe)
        // ============================================================
        UpdateHeroStatusIcons(); // si aÃƒÂºn no existe, te lo puedo generar

        // ============================================================
        // Ã°Å¸â€Â¥ 5. INICIO NORMAL DEL TURNO DEL HÃƒâ€°ROE
        // ============================================================
        handManager.StartTurn();
        dialogue.text += "\nÃ‚Â¿QuÃƒÂ© deberÃƒÂ­a hacer tu hÃƒÂ©roe?";
        state = BattleState.PlayerChoosingSkill;
    }


    // ============================================================
    // ENEMY TURN (NUEVO SISTEMA CON HEROSKILLS)
    // ============================================================
    IEnumerator EnemyTurn()
    {


        TickPoison();
        TickBleed();
        TickBurn();
        TickCorrosion();
        TickShock();
        TickSeal();
        TickStealth_Hero();


        UpdateEnemyStatusIcons();

        state = BattleState.EnemyTurn;

        // Estados que bloquean acciÃƒÂ³n
        if (CheckFreeze() || CheckPetrify() || CheckStun())
        {
            yield return new WaitForSeconds(1f);
            state = BattleState.PlayerTurn;
            PlayerTurn();
            yield break;
        }
        // Ã°Å¸â€Â¥ STEALTH: el enemigo no puede atacar al hÃƒÂ©roe
        if (hero.status.stealth)
        {
            dialogue.text += $"\n{enemy.enemyName} no puede encontrar a {hero.heroName} debido al sigilo!";
            yield return new WaitForSeconds(1f);

            state = BattleState.PlayerTurn;
            PlayerTurn();
            yield break;
        }


        // Ã°Å¸â€Â¥ Elegir skill aleatoria del enemigo
        HeroSkill chosen = enemy.ChooseSmartSkill(hero);
        chosen.OnSkillExecuted?.Invoke();


        // Si no tiene HeroSkills, usar ataque bÃƒÂ¡sico
        if (chosen == null)
        {
            enemySpriteController.ShowAttack();
            float damage = enemy.stats.attack;
            float mitigated = hero.ApplyDamage(damage);
            ShowDamageOnHero(mitigated);
            heroSpriteController.ShowHurt();

            dialogue.text = $"{enemy.enemyName} ataca e inflige {damage:F0}!";
            yield return new WaitForSeconds(1f);

            state = BattleState.PlayerTurn;
            PlayerTurn();
            yield break;
        }

        // ============================================================
        // ANIMACIÃƒâ€œN DE SKILL
        // ============================================================
        int skillIndex = enemy.heroSkills.IndexOf(chosen);
        enemySpriteController.ShowSkill(skillIndex);

        dialogue.text = $"{enemy.enemyName} usa {chosen.skillName}!";
        yield return new WaitForSeconds(0.6f);

        // Efecto visual
        if (chosen.visualPrefabs != null && chosen.visualPrefabs.Count > 0)
        {
            var prefab = chosen.visualPrefabs[0];
            Instantiate(prefab, playerEffectPoint.position, Quaternion.identity, playerEffectPoint);
        }

        // ============================================================
        // CÃƒÂLCULO DE DAÃƒâ€˜O
        // ============================================================
        float dmg = chosen.basePower
                  + enemy.stats.attack * chosen.scalingAttack
                  - hero.stats.defense * chosen.scalingDefense;

        dmg = Mathf.Max(0, dmg);

        float finalDamage = hero.ApplyDamage(dmg);
        ShowDamageOnHero(finalDamage);
        heroSpriteController.ShowHurt();

        dialogue.text += $"\nInflige {finalDamage:F0} de daÃƒÂ±o!";

        yield return new WaitForSeconds(0.8f);

        // ============================================================
        // EFECTOS SECUNDARIOS
        // ============================================================
        if (chosen.statusEffect != null)
        {
            hero.AddStatusEffect(new StatusEffect(chosen.statusEffect));
            dialogue.text += $"\n{hero.heroName} sufre un efecto adicional!";
        }
        // Ã°Å¸â€Â¥Ã°Å¸â€Â¥Ã°Å¸â€Â¥ DISPARAR EVENTO DE LA SKILL DEL ENEMIGO Ã°Å¸â€Â¥Ã°Å¸â€Â¥Ã°Å¸â€Â¥
        // ============================================================
        // FIN DEL TURNO
        // ============================================================
        if (hero.stats.currentHealth <= 0)
        {
            heroSpriteController.ShowDead();
            var assign = FindObjectOfType<HeroAssignmentManager>();
            assign.OnHeroDeath();
            SetHero(assign.ActiveHero);
            hero = assign.ActiveHero;
            handManager.hero = hero;
            heroCombatSprite.sprite = hero.combatSprite != null ? hero.combatSprite : hero.portrait;
            handManager.actionsLeft = 1;
            handManager.GenerateInitialHand();
            handManager.RefreshHandUI();
            dialogue.text = $"{hero.heroName} entra en combate!";
            yield return new WaitForSeconds(1f);
        }
        TickPoison_Hero();
        TickBleed_Hero();
        TickBurn_Hero();
        TickCorrosion_Hero();
        TickShock_Hero();
        TickFullCounter_Hero();

        state = BattleState.PlayerTurn;
        PlayerTurn();
    }


    // ============================================================
    // CARTAS (HAND SYSTEM)
    // ============================================================

    public void PlayerUseSkillFromHand(HeroSkill skill, int tier)
    {
        StartCoroutine(PlayerUseSkillFromHandRoutine(skill, tier));
    }

    IEnumerator PlayerUseSkillFromHandRoutine(HeroSkill skill, int tier)
    {
        state = BattleState.Busy;

        int skillIndex = hero.skills.IndexOf(skill);
        heroSpriteController.ShowSkill(skillIndex);
        // Ensure the dispatcher is assigned (búsqueda robusta, ver ResolveDispatcherFor)
        if (currentDispatcher == null && herovisual != null)
        {
            currentDispatcher = ResolveDispatcherFor(herovisual);
        }

        if (currentDispatcher == null)
        {
            Debug.LogError($"[CombatManager] TriggerSkillEvent NO se ejecutó para '{hero.heroName}': " +
                            $"currentDispatcher es null. Skill '{skill.skillName}' (index {skillIndex}, tier {tier}).");
        }
        else
        {
            currentDispatcher.TriggerSkillEvent(skillIndex, tier);
        }
        yield return new WaitForSeconds(0.4f);

        float damage = 0f;

        if (skill.healAmount <= 0)
        {
            damage = skill.damageEffect != null
                ? skill.damageEffect.ComputeDamage(hero, enemy)
                : skill.basePower + hero.stats.attack * skill.scalingAttack + hero.stats.defense * skill.scalingDefense;

            // Ã°Å¸â€Â¥ INTEGRACIÃƒâ€œN CORRECTA DEL TIPO DE DAÃƒâ€˜O (AQUÃƒÂ)
            if (damageOverride.remainingTurns > 0)
            {
                dialogue.text += $"\nEl ataque se convierte en daÃƒÂ±o {damageOverride.type}!";
                ApplyDamageTypeEffects(damageOverride.type, ref damage);

                damageOverride.remainingTurns--;
                if (damageOverride.remainingTurns <= 0)
                    damageOverride.type = DamageType.Normal;
            }

            // Ã¢Å“â€ Ahora sÃƒÂ­, aplicar multiplicador de tier
            damage = handManager.ApplyTierMultiplier(damage, tier);
        }


        dialogue.text = $"{hero.heroName} usa {skill.skillName} (Tier {tier})!";
        yield return new WaitForSeconds(0.6f);

        if (skill.healAmount > 0)
        {
            SpawnSkillVisualsOnPlayer(skill);
        }
        else if (skill.buffAttack != 0 || skill.buffDefense != 0)
        {
            GameObject buffPrefab = null;

            if (skill.visualPrefabs != null && skill.visualPrefabs.Count > 0)
                buffPrefab = skill.visualPrefabs[0];

            SpawnBuffVisualsOnPlayer(buffPrefab);
        }

        else
        {
            SpawnSkillVisualsOnEnemy(skill, tier);
        }

        if (damage > 0)
        {
            enemy.ApplyDamage(damage);
            float mitigated = enemy.ApplyDamage(damage);
            ShowDamageOnEnemy(mitigated);
            enemySpriteController.ShowHurt();
            dialogue.text += $"\nHace {damage:F0} de daÃƒÂ±o!";
        }

        yield return new WaitForSeconds(0.8f);

        if (skill.damageEffect != null &&
     hero != null &&
     enemy != null &&
     enemy.activeEffects != null &&
     hero.activeEffects != null)
        {
            skill.damageEffect.ApplySecondaryEffects(hero, enemy);
            dialogue.text += "\nEfectos adicionales aplicados!";
        }



        if (skill.healAmount > 0)
        {
            float heal = handManager.ApplyTierMultiplier(skill.healAmount, tier);
            hero.RestoreHealth(heal);
            dialogue.text += $"\n{hero.heroName} se cura {heal:F0}!";
        }

        if (skill.buffAttack != 0 || skill.buffDefense != 0)
        {
            float buffAtk = handManager.ApplyTierMultiplier(skill.buffAttack, tier);
            float buffDef = handManager.ApplyTierMultiplier(skill.buffDefense, tier);

            var buff = new StatusEffect
            {
                effectName = "Buff",
                type = StatusEffectType.Buff,
                attackDelta = buffAtk,
                defenseDelta = buffDef,
                remainingTurns = skill.buffDuration
            };

            hero.activeEffects.Add(buff);
            dialogue.text += $"\n{hero.heroName} obtiene un buff!";
        }

        if (skill.statusEffect != null)
        {
            enemy.AddStatusEffect(new StatusEffect(skill.statusEffect));
            dialogue.text += $"\n{enemy.enemyName} es electrificado!";
        }


        if (skill.stanceEffect != null)
        {
            hero.activeEffects.Add(skill.stanceEffect.ToStatusEffect());
            dialogue.text += $"\n{hero.heroName} adopta una postura!";
        }


        if (!enemy.IsAlive)
        {
            enemySpriteController.ShowDead();
            enemy.OnEnemyDeath?.Invoke();
            state = BattleState.Won;
            dialogue.text = $"{enemy.enemyName} ha sido derrotado!";
            yield break;
        }

        StartCoroutine(EnemyTurn());
    }
    public void ApplyDamageTypeEffects(DamageType type, ref float dmg)
    {
        switch (type)
        {
            case DamageType.Necrotic:
                dmg *= 1.20f;
                ApplyNecroticEffect(dmg); // Bloquea curaciÃƒÂ³n equivalente al daÃƒÂ±o
                break;

            case DamageType.Radiant:
                dmg *= 1.15f;
                ApplyRadiantEffect(5f, 2); // Baja defensa 5 por 2 turnos
                break;

            case DamageType.Psychic:
                dmg *= 1.10f;
                ApplyPsychicEffect(0.30f, 2); // Enemigo hace 30% menos daÃƒÂ±o
                break;

            case DamageType.Fire:
                dmg *= 1.10f;
                ApplyVulnerability(0.15f, 2); // Recibe 15% mÃƒÂ¡s daÃƒÂ±o
                BURN();
                break;

            case DamageType.Cold:
                dmg *= 1.05f;
                ApplyVulnerability(0.10f, 2);
                FREEZE();
                break;

            case DamageType.Acid:
                dmg *= 1.10f;
                CORROSION();
                break;

            case DamageType.Lightning:
                dmg *= 1.15f;
                SHOCK();
                break;

            case DamageType.Thunder:
                dmg *= 1.20f;
                STUN();
                break;

            case DamageType.Force:
                dmg *= 1.15f;
                break;

            case DamageType.Slashing:
            case DamageType.Piercing:
            case DamageType.Bludgeoning:
                dmg *= 1.00f;
                break;
        }
    }


    private void OnDisable()
    {
        ResetAllHeroesOnCombatEnd();
    }

    public void ResetAllHeroesOnCombatEnd()
    {
        var assign = FindObjectOfType<HeroAssignmentManager>();
        if (assign != null)
        {
            for (int i = 0; i < 4; i++)
            {
                Hero h = assign.GetHeroByIndex(i);
                if (h != null)
                {
                    h.ClearStatusEffects();
                    h.stats.currentHealth = h.stats.maxHealth;
                }
            }
        }

        foreach (var hv in heroVisuals)
        {
            if (hv != null && hv.hero != null)
            {
                hv.hero.ClearStatusEffects();
                hv.hero.stats.currentHealth = hv.hero.stats.maxHealth;
            }
        }
    }

    public void ResetBattle()
    {
        StopAllCoroutines();

        // Reiniciar estado
        state = BattleState.Start;

        // Resetear todos los hÃƒÂ©roes
        foreach (var hv in heroVisuals)
        {
            if (hv != null && hv.hero != null)
            {
                hv.hero.ClearStatusEffects();
                hv.hero.stats.currentHealth = hv.hero.stats.maxHealth;
            }
        }

        // Resetear enemigo
        if (enemy != null)
        {
            enemy.stats.currentHealth = enemy.stats.maxHealth;
            enemy.ClearStatusEffects();
            enemy.skipNextTurn = false;

            // Resetear animaciones del enemigo
            if (enemySpriteController != null)
            {
                enemySpriteController.overrideEffects = false;
                enemySpriteController.HideAll(enemySpriteController.baseLayer);
                enemySpriteController.HideAll(enemySpriteController.effectLayer);
                enemySpriteController.ShowIdle();
            }
        }

        // Resetear animaciones del hÃƒÂ©roe
        if (heroSpriteController != null)
        {
            heroSpriteController.overrideEffects = false;
            heroSpriteController.HideAll(heroSpriteController.baseLayer);
            heroSpriteController.HideAll(heroSpriteController.effectLayer);
            heroSpriteController.ShowIdle();
        }

        // Resetear mano
        handManager.actionsLeft = 1;
        handManager.hand.Clear();
        handManager.GenerateInitialHand();
        handManager.RefreshHandUI();

        // Resetear sprites y refrescar UI de estados
        heroCombatSprite.sprite = hero.combatSprite != null ? hero.combatSprite : hero.portrait;
        RefreshHeroStatusUI();

        // Mensaje inicial
        dialogue.text = $"Ã‚Â¡{enemy.enemyName} aparece!";

        // Iniciar combate
        StartCoroutine(StartBattle());
    }
    // ============================================================
    // EFECTOS -> ENEMIGO
    // ============================================================
    public void POISON()
    {

        ApplyPoison(5f, 3);
        enemy.UpdateDebugStatus("Poison", 3, 5f);

    }



    public void ApplyPoison(float dmg, int turns)
    {
        enemy.status.poisonDamage += dmg;
        enemy.status.poisonTurns = Mathf.Max(enemy.status.poisonTurns, turns);
    }


    public void TickPoison()
    {
        if (enemy.status.poisonTurns <= 0) return;

        float mitigated = Mathf.Max(0, enemy.status.poisonDamage - enemy.stats.defense);

        if (mitigated > 0)
        {
            enemy.ApplyDamage(mitigated);
            ShowDamageOnEnemy(mitigated);
            enemySpriteController.ShowHurt();
            dialogue.text += $"\n{enemy.enemyName} sufre {mitigated:F0} por Veneno!";
            statusEvents?.TriggerPoison(); // Ã°Å¸â€Â¥ NUEVO
        }

        enemy.status.poisonTurns--;
        if (enemy.status.poisonTurns <= 0)
            enemy.status.poisonDamage = 0; // Ã°Å¸â€Â¥ LIMPIA EL ESTADO
        RefreshStatusIcons(enemyStatusContainer, enemy.status);
    }

    public void BLEED()
    {
        ApplyBleed(5f, 3);
        enemy.UpdateDebugStatus("Bleed", 3, 5f);

    }


    public void ApplyBleed(float dmg, int turns)
    {
        enemy.status.bleedDamage += dmg;
        enemy.status.bleedTurns = Mathf.Max(enemy.status.bleedTurns, turns);
    }


    public void TickBleed()
    {
        if (enemy.status.bleedTurns <= 0) return;

        float mitigated = Mathf.Max(0, enemy.status.bleedDamage - enemy.stats.defense);

        if (mitigated > 0)
        {
            enemy.ApplyDamage(mitigated);
            ShowDamageOnEnemy(mitigated);
            enemySpriteController.ShowHurt();
            dialogue.text += $"\n{enemy.enemyName} sangra {mitigated:F0}!";
            statusEvents?.TriggerBleed(); // Ã°Å¸â€Â¥ NUEVO
        }

        enemy.status.bleedTurns--;
        if (enemy.status.bleedTurns <= 0)
            enemy.status.bleedDamage = 0; // Ã°Å¸â€Â¥ LIMPIA EL ESTADO
        RefreshStatusIcons(enemyStatusContainer, enemy.status);

    }

    public void BURN()
    {
        ApplyBurn(5f, 3);
        enemy.UpdateDebugStatus("Burn", 3, 5f);


    }
    public void ApplyBurn(float dmg, int turns)
    {
        enemy.status.burnDamage += dmg;
        enemy.status.burnTurns = turns;
    }

    public void TickBurn()
    {
        if (enemy.status.burnTurns <= 0) return;

        float mitigated = Mathf.Max(0, enemy.status.burnDamage - enemy.stats.defense);

        if (mitigated > 0)
        {
            enemy.ApplyDamage(mitigated);
            ShowDamageOnEnemy(mitigated);
            enemySpriteController.ShowHurt();
            dialogue.text += $"\n{enemy.enemyName} se quema por {mitigated:F0}!";
            statusEvents?.TriggerBurn(); // Ã°Å¸â€Â¥ NUEVO
        }

        enemy.status.burnTurns--;
        if (enemy.status.burnTurns <= 0)
            enemy.status.burnDamage = 0; // Ã°Å¸â€Â¥ LIMPIA EL ESTADO
        RefreshStatusIcons(enemyStatusContainer, enemy.status);

    }

    public void SHOCK()
    {
        ApplyShock(0.85f); // o el porcentaje que quieras
        enemy.UpdateDebugStatus("Shock", 2, 0.85f);


    }


    public void ApplyShock(float percent, int turns = 2)
    {
        enemy.status.shockStacks++;
        enemy.status.shockPercent = percent;
        enemy.status.shockTurns = turns; // SIEMPRE refresca turnos
    }


    public void TickShock()
    {
        if (enemy.status.shockTurns <= 0) return;

        float baseDamage = enemy.stats.attack * enemy.status.shockPercent;
        float totalDamage = baseDamage * enemy.status.shockStacks;

        enemy.ApplyDamage(totalDamage);
        ShowDamageOnEnemy(totalDamage);
        enemySpriteController.ShowHurt();
        dialogue.text += $"\n{enemy.enemyName} recibe {totalDamage:F0} por Shock!";
        statusEvents?.TriggerShock(); // Ã°Å¸â€Â¥ NUEVO

        enemy.status.shockTurns--;
        if (enemy.status.shockTurns <= 0)
        {
            enemy.status.shockStacks = 0; // Ã°Å¸â€Â¥ LIMPIA
            enemy.status.shockPercent = 0; // Ã°Å¸â€Â¥ LIMPIA
        }
        RefreshStatusIcons(enemyStatusContainer, enemy.status);


        if (enemy.status.shockTurns <= 0)
        {
            enemy.status.shockStacks = 0; // Se limpia al terminar
            enemy.status.shockPercent = 0;
        }
    }
    public void RefreshStatusIcons(Transform container, HeroStatus status)
    {
        if (container == null || status == null) return;
        // Limpia iconos anteriores
        foreach (Transform child in container)
            GameObject.Destroy(child.gameObject);

        // AÃƒÂ±ade iconos segÃƒÂºn stacks o estados activos
        AddIcons(container, "Poison", status.poisonTurns, status.poisonDamage > 0 ? 1 : 0);
        AddIcons(container, "Bleed", status.bleedTurns, status.bleedDamage > 0 ? 1 : 0);
        AddIcons(container, "Burn", status.burnTurns, Mathf.CeilToInt(status.burnDamage / 5f)); // ejemplo
        AddIcons(container, "Shock", status.shockTurns, status.shockStacks);
        AddIcons(container, "Corrosion", status.corrosionTurns, Mathf.CeilToInt(status.corrosionPercent * 100));

        if (status.frozen) AddIcons(container, "Freeze", 1, 1);
        if (status.petrified) AddIcons(container, "Petrify", 1, 1);
        if (status.stunned) AddIcons(container, "Stun", 1, 1);
        if (status.webbed) AddIcons(container, "Web", 1, 1);
        if (status.stealth) AddIcons(container, "Stealth", status.stealthTurns, 1);
        if (status.fullCounter) AddIcons(container, "FullCounter", status.fullCounterTurns, status.fullCounterTier);

    }
    public void RefreshStatusIcons(Transform container, EnemyStatus status)
    {
        if (container == null || status == null) return;
        // Limpia iconos anteriores
        foreach (Transform child in container)
            GameObject.Destroy(child.gameObject);

        // AÃƒÂ±ade iconos segÃƒÂºn stacks o estados activos
        AddIcons(container, "Poison", status.poisonTurns, status.poisonDamage > 0 ? 1 : 0);
        AddIcons(container, "Bleed", status.bleedTurns, status.bleedDamage > 0 ? 1 : 0);
        AddIcons(container, "Burn", status.burnTurns, Mathf.CeilToInt(status.burnDamage / 5f)); // ejemplo
        AddIcons(container, "Shock", status.shockTurns, status.shockStacks);
        AddIcons(container, "Corrosion", status.corrosionTurns, Mathf.CeilToInt(status.corrosionPercent * 100));
        AddIcons(container, "Seal", status.sealedTurns, status.sealedSkillActive ? 1 : 0);

        if (status.frozen) AddIcons(container, "Freeze", 1, 1);
        if (status.petrified) AddIcons(container, "Petrify", 1, 1);
        if (status.stunned) AddIcons(container, "Stun", 1, 1);
        if (status.webbed) AddIcons(container, "Web", 1, 1);

    }
    private void AddIcons(Transform container, string iconName, int turns, int stacks)
    {
        if (turns <= 0 || stacks <= 0) return;

        for (int i = 0; i < stacks; i++)
        {
            GameObject icon = Instantiate(statusIconPrefab, container);
            icon.GetComponent<StatusIcon>().SetIcon(iconName, turns);
        }
    }

    public void FREEZE()
    {

        ApplyFreeze(2);
        enemy.UpdateDebugStatus("Freeze", 1, 0f);

    }

    public void ApplyFreeze(int turns)
    {
        enemy.status.frozen = true;
        enemy.status.freezeTurns = turns;
    }

    public bool CheckFreeze()
    {
        if (!enemy.status.frozen) return false;

        enemy.status.freezeTurns--;
        if (enemy.status.freezeTurns <= 0)
            enemy.status.frozen = false;
        if (enemy.status.freezeTurns <= 0)
            enemy.status.frozen = false; // Ã°Å¸â€Â¥ LIMPIA
        dialogue.text += $"\n{enemy.enemyName} estÃƒÂ¡ congelado y no puede actuar!";
        RefreshStatusIcons(enemyStatusContainer, enemy.status);

        return true;
    }
    public void PETRIFY()
    {

        ApplyPetrify(3);
        enemy.UpdateDebugStatus("Petrify", 1, 0f);


    }

    public void ApplyPetrify(int turns)
    {
        enemy.status.petrified = true;
        enemy.status.petrifyTurns = turns;
    }

    public bool CheckPetrify()
    {
        if (!enemy.status.petrified) return false;

        enemy.status.petrifyTurns--;

        RefreshStatusIcons(enemyStatusContainer, enemy.status);
        if (enemy.status.petrifyTurns <= 0)
            enemy.status.petrified = false; // Ã°Å¸â€Â¥ LIMPIA
        dialogue.text += $"\n{enemy.enemyName} estÃƒÂ¡ petrificado!";

        return true;
    }

    public void STUN()
    {
        ApplyStun(1);
    }
    public void ApplyStun(int turns)
    {
        enemy.status.stunned = true;
        enemy.status.stunTurns = turns;
    }

    public bool CheckStun()
    {
        if (!enemy.status.stunned) return false;

        enemy.status.stunTurns--;
        if (enemy.status.stunTurns <= 0)
            enemy.status.stunned = false; // Ã°Å¸â€Â¥ LIMPIA
        if (enemy.status.stunTurns <= 0)
            enemy.status.stunned = false;
        RefreshStatusIcons(enemyStatusContainer, enemy.status);

        dialogue.text += $"\n{enemy.enemyName} estÃƒÂ¡ aturdido!";
        return true;
    }

    public void COUNTER()
    {


    }
    public void ApplyCounter(StatusEffect eff)
    {
        dialogue.text += $"\n{enemy.enemyName} estÃƒÂ¡ listo para contraatacar!";

        eff.remainingTurns--;
    }
    public void CORROSION()
    {
        ApplyCorrosion(0.05f, 3); // 5% por 3 turnos, ejemplo
        enemy.UpdateDebugStatus("Corrosion", 3, 0.05f);

    }
    public void ApplyCorrosion(float percent, int turns)
    {
        enemy.status.corrosionPercent += percent; // acumula
        enemy.status.corrosionTurns = turns;
    }

    public void TickCorrosion()
    {
        if (enemy.status.corrosionTurns <= 0) return;

        float rawDamage = enemy.stats.maxHealth * enemy.status.corrosionPercent;
        float mitigated = Mathf.Max(0, rawDamage - enemy.stats.defense);

        if (mitigated > 0)
        {
            enemy.ApplyDamage(mitigated);
            ShowDamageOnEnemy(mitigated);
            enemySpriteController.ShowHurt();
            dialogue.text += $"\n{enemy.enemyName} sufre corrosiÃƒÂ³n por {mitigated:F0}!";
            statusEvents?.TriggerCorrosion(); // Ã°Å¸â€Â¥ NUEVO
        }

        enemy.status.corrosionTurns--;
        if (enemy.status.corrosionTurns <= 0)
            enemy.status.corrosionPercent = 0; // Ã°Å¸â€Â¥ LIMPIA
        RefreshStatusIcons(enemyStatusContainer, enemy.status);

    }
    public void SEAL()
    {
        ApplySeal(2); // ejemplo: 2 turnos
        enemy.UpdateDebugStatus("Seal", 2, 0f);
    }
    public void ApplySeal(int turns)
    {
        HeroSkill strongest = null;
        float highestDamage = -1f;

        foreach (var skill in enemy.heroSkills)
        {
            float dmg = skill.basePower
                      + enemy.stats.attack * skill.scalingAttack
                      - hero.stats.defense * skill.scalingDefense;

            if (dmg > highestDamage)
            {
                highestDamage = dmg;
                strongest = skill;
            }
        }

        if (strongest != null)
        {
            enemy.status.sealedSkill = strongest;
            enemy.status.sealedSkillActive = true;
            enemy.status.sealedTurns = turns;

            dialogue.text += $"\nÃ‚Â¡{enemy.enemyName} tiene sellada su habilidad mÃƒÂ¡s poderosa ({strongest.skillName})!";
        }

        RefreshStatusIcons(enemyStatusContainer, enemy.status);
    }
    public void TickSeal()
    {
        if (!enemy.status.sealedSkillActive) return;

        enemy.status.sealedTurns--;

        if (enemy.status.sealedTurns <= 0)
        {
            dialogue.text += $"\nEl sello sobre {enemy.enemyName} se desvanece.";
            enemy.status.sealedSkillActive = false;
            enemy.status.sealedSkill = null;
        }

        RefreshStatusIcons(enemyStatusContainer, enemy.status);
    }

    public void UpdateEnemyStatusIcons()
    {
        iconPoison?.SetActive(enemy.status.poisonTurns > 0);
        iconBleed?.SetActive(enemy.status.bleedTurns > 0);
        iconBurn?.SetActive(enemy.status.burnTurns > 0);
        iconShock?.SetActive(enemy.status.shockTurns > 0);
        iconFreeze?.SetActive(enemy.status.frozen);
        iconPetrify?.SetActive(enemy.status.petrified);
        iconStun?.SetActive(enemy.status.stunned);
        iconCorrosion?.SetActive(enemy.status.corrosionTurns > 0);
        iconWeb?.SetActive(enemy.status.webbed); // NUEVO
    }


    // ============================================================
    // DAÃƒâ€˜OS -> ENEMIGO
    // ============================================================
    public void ApplyVulnerability(float percent, int turns)
    {
        damageTypeEffects.vulnerabilityBonus = percent;
        damageTypeEffects.vulnerabilityTurns = turns;

        dialogue.text += $"\n{enemy.enemyName} recibe {percent * 100:F0}% mÃƒÂ¡s daÃƒÂ±o por {turns} turnos!";
    }

    public void DAMAGE_FIRE()
    {
        ApplyDamageOverride(DamageType.Fire, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o de Fuego!";
    }

    public void DAMAGE_COLD()
    {
        ApplyDamageOverride(DamageType.Cold, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o de FrÃƒÂ­o!";
    }

    public void DAMAGE_LIGHTNING()
    {
        ApplyDamageOverride(DamageType.Lightning, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o de RelÃƒÂ¡mpago!";
    }

    public void DAMAGE_THUNDER()
    {
        ApplyDamageOverride(DamageType.Thunder, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o de Trueno!";
    }

    public void DAMAGE_ACID()
    {
        ApplyDamageOverride(DamageType.Acid, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o de ÃƒÂcido!";
    }

    public void DAMAGE_POISON()
    {
        ApplyDamageOverride(DamageType.Poison, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o de Veneno!";
    }

    public void DAMAGE_PSYCHIC()
    {
        ApplyDamageOverride(DamageType.Psychic, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o PsÃƒÂ­quico!";
    }
    public void ApplyPsychicEffect(float percent, int turns)
    {
        damageTypeEffects.damageReduction = percent;
        damageTypeEffects.damageReductionTurns = turns;

        dialogue.text += $"\n{enemy.enemyName} infligirÃƒÂ¡ {percent * 100:F0}% menos daÃƒÂ±o por {turns} turnos!";
    }

    public void DAMAGE_RADIANT()
    {
        ApplyDamageOverride(DamageType.Radiant, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o Radiante!";
    }
    public void ApplyRadiantEffect(float defReduction, int turns)
    {
        damageTypeEffects.defenseReduction = defReduction;
        damageTypeEffects.defenseReductionTurns = turns;

        enemy.stats.defense -= defReduction;
        dialogue.text += $"\n{enemy.enemyName} pierde {defReduction:F0} de defensa por {turns} turnos!";
    }

    public void DAMAGE_NECROTIC()
    {
        ApplyDamageOverride(DamageType.Necrotic, 1);
        Debug.Log("\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o NecrÃƒÂ³tico!");
    }
    public void ApplyNecroticEffect(float amount)
    {
        damageTypeEffects.healingBlocked = true;
        damageTypeEffects.healingBlockedAmount = amount;
        dialogue.text += $"\n{enemy.enemyName} no podrÃƒÂ¡ curarse {amount:F0} de vida!";
    }

    public void DAMAGE_FORCE()
    {
        ApplyDamageOverride(DamageType.Force, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o de Fuerza!";
    }

    public void DAMAGE_SLASHING()
    {
        ApplyDamageOverride(DamageType.Slashing, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o Cortante!";
    }

    public void DAMAGE_PIERCING()
    {
        ApplyDamageOverride(DamageType.Piercing, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o Perforante!";
    }

    public void DAMAGE_BLUDGEONING()
    {
        ApplyDamageOverride(DamageType.Bludgeoning, 1);
        dialogue.text += "\nEl siguiente ataque infligirÃƒÂ¡ daÃƒÂ±o Contundente!";
    }

    public void ApplyDamageOverride(DamageType type, int turns)
    {
        damageOverride.type = type;
        damageOverride.remainingTurns = turns;
    }
    // ============================================================
    // BUFOS -> HEROE
    // ============================================================
    public void RANKUP(int level)
    {
        handManager.rankupMode = true;
        handManager.rankupLevel = level;

        handManager.rankupRemaining = level == 1 ? 1 :
                                      level == 2 ? 3 :
                                      level == 3 ? 4 : 1;

        skillButtons[0].GetComponent<ShineDOTween>().ActivarEfectos();
        skillButtons[1].GetComponent<ShineDOTween>().ActivarEfectos();
        skillButtons[2].GetComponent<ShineDOTween>().ActivarEfectos();
        skillButtons[3].GetComponent<ShineDOTween>().ActivarEfectos();

        dialogue.text = $"Selecciona {handManager.rankupRemaining} carta(s) para subir de Tier.";

        handManager.EnableRankupFeedback();
    }


    // ============================================================
    // EFECTOS -> HERO
    // ============================================================
    public void POISON_HERO()
    {

        ApplyPoison_Hero(5f, 3);
        hero.UpdateDebugStatus("Poison", 3, 5f);

    }


    public void ApplyPoison_Hero(float dmg, int turns)
    {
        hero.status.poisonDamage += dmg;
        hero.status.poisonTurns = turns;
    }

    public void TickPoison_Hero()
    {
        if (hero.status.poisonTurns <= 0) return;

        float mitigated = hero.status.poisonDamage; // IGNORA DEFENSA

        hero.ApplyDamage(mitigated);
        ShowDamageOnHero(mitigated);
        heroSpriteController.ShowHurt();
        dialogue.text += $"\n{hero.heroName} sufre {mitigated:F0} por Veneno!";
        heroStatusEvents?.TriggerPoison(); // Ã°Å¸â€Â¥ AHORA SÃƒÂ SE DISPARA
        hero.status.poisonTurns--;
        if (hero.status.poisonTurns <= 0)
            hero.status.poisonDamage = 0; // Ã°Å¸â€Â¥ LIMPIA
        RefreshStatusIcons(heroStatusContainer, hero.status);

    }



    public void BLEED_HERO()
    {
        ApplyBleed_Hero(5f, 3);
        hero.UpdateDebugStatus("Bleed", 3, 5f);

    }

    public void ApplyBleed_Hero(float dmg, int turns)
    {
        hero.status.bleedDamage += dmg;
        hero.status.bleedTurns = turns;
    }

    public void TickBleed_Hero()
    {
        if (hero.status.bleedTurns <= 0) return;

        float mitigated = hero.status.bleedDamage; // IGNORA DEFENSA

        hero.ApplyDamage(mitigated);
        ShowDamageOnHero(mitigated);
        heroSpriteController.ShowHurt();
        dialogue.text += $"\n{hero.heroName} sangra {mitigated:F0}!";
        heroStatusEvents?.TriggerBleed();

        hero.status.bleedTurns--;
        if (hero.status.bleedTurns <= 0)
            hero.status.bleedDamage = 0; // Ã°Å¸â€Â¥ LIMPIA
        RefreshStatusIcons(heroStatusContainer, hero.status);
    }


    public void BURN_HERO()
    {
        ApplyBurn_Hero(5f, 3);
        hero.UpdateDebugStatus("Burn", 3, 5f);


    }
    public void ApplyBurn_Hero(float dmg, int turns)
    {
        hero.status.burnDamage = dmg;
        hero.status.burnTurns = turns;
    }

    public void TickBurn_Hero()
    {
        if (hero.status.burnTurns <= 0) return;

        float mitigated = hero.status.burnDamage; // IGNORA DEFENSA

        hero.ApplyDamage(mitigated);
        ShowDamageOnHero(mitigated);
        heroSpriteController.ShowHurt();
        dialogue.text += $"\n{hero.heroName} se quema por {mitigated:F0}!";
        heroStatusEvents?.TriggerBurn();

        hero.status.burnTurns--;
        if (hero.status.burnTurns <= 0)
            hero.status.burnDamage = 0; // Ã°Å¸â€Â¥ LIMPIA
        RefreshStatusIcons(heroStatusContainer, hero.status);
    }

    public void SHOCK_HERO()
    {
        ApplyShock_Hero(0.85f); // o el porcentaje que quieras
        hero.UpdateDebugStatus("Shock", 2, 0.85f);


    }


    public void ApplyShock_Hero(float percent, int turns = 2)
    {
        hero.status.shockStacks++;
        hero.status.shockPercent = percent;
        hero.status.shockTurns = turns;
    }

    public void TickShock_Hero()
    {
        if (hero.status.shockTurns <= 0) return;

        float baseDamage = hero.stats.attack * hero.status.shockPercent;
        float totalDamage = baseDamage * hero.status.shockStacks;

        float mitigated = totalDamage; // IGNORA DEFENSA

        hero.ApplyDamage(mitigated);
        ShowDamageOnHero(mitigated);
        heroSpriteController.ShowHurt();
        dialogue.text += $"\n{hero.heroName} recibe {mitigated:F0} por Shock!";

        hero.status.shockTurns--;

        if (hero.status.shockTurns <= 0)
        {
            hero.status.shockStacks = 0;
            hero.status.shockPercent = 0;
        }
        heroStatusEvents?.TriggerShock();
        if (hero.status.shockTurns <= 0)
        {
            hero.status.shockStacks = 0; // Ã°Å¸â€Â¥ LIMPIA
            hero.status.shockPercent = 0; // Ã°Å¸â€Â¥ LIMPIA
        }
        RefreshStatusIcons(heroStatusContainer, hero.status);

    }




    public void FREEZE_HERO()
    {

        ApplyFreeze_Hero(2);
        hero.UpdateDebugStatus("Freeze", 1, 0f);

    }
    public void ApplyFreeze_Hero(int turns)
    {
        hero.status.frozen = true;
        hero.status.freezeTurns = turns;
    }

    public bool CheckFreeze_Hero()
    {
        if (!hero.status.frozen) return false;

        hero.status.freezeTurns--;
        if (hero.status.freezeTurns <= 0)
            hero.status.frozen = false; // Ã°Å¸â€Â¥ LIMPIA
        if (hero.status.freezeTurns <= 0)
            hero.status.frozen = false;

        dialogue.text += $"\n{hero.heroName} estÃƒÂ¡ congelado y no puede actuar!";
        RefreshStatusIcons(heroStatusContainer, hero.status);
        return true;

    }



    public void PETRIFY_HERO()
    {

        ApplyPetrify_Hero(3);
        hero.UpdateDebugStatus("Petrify", 1, 0f);


    }

    public void ApplyPetrify_Hero(int turns)
    {
        hero.status.petrified = true;
        hero.status.petrifyTurns = turns;
    }

    public bool CheckPetrify_Hero()
    {
        if (!hero.status.petrified) return false;

        hero.status.petrifyTurns--;
        if (hero.status.petrifyTurns <= 0)
            hero.status.petrified = false; // Ã°Å¸â€Â¥ LIMPIA
        dialogue.text += $"\n{hero.heroName} estÃƒÂ¡ petrificado!";
        RefreshStatusIcons(heroStatusContainer, hero.status);
        return true;
    }

    public void STUN_HERO()
    {
        ApplyStun_Hero(1);
    }
    public void ApplyStun_Hero(int turns)
    {
        hero.status.stunned = true;
        hero.status.stunTurns = turns;
    }

    public bool CheckStun_Hero()
    {
        if (!hero.status.stunned) return false;

        hero.status.stunTurns--;
        if (hero.status.stunTurns <= 0)
            hero.status.stunned = false; // Ã°Å¸â€Â¥ LIMPIA

        dialogue.text += $"\n{hero.heroName} estÃƒÂ¡ aturdido!";
        RefreshStatusIcons(heroStatusContainer, hero.status);
        return true;
    }
    public void COUNTER_HERO()
    {


    }
    public void ApplyCounter_HERO(StatusEffect eff)
    {
        dialogue.text += $"\n{enemy.enemyName} estÃƒÂ¡ listo para contraatacar!";
        eff.remainingTurns--;
    }
    public void CORROSION_HERO()
    {
        ApplyCorrosion_Hero(0.05f, 3); // 5% por 3 turnos, ejemplo
        hero.UpdateDebugStatus("Corrosion", 3, 0.05f);

    }
    public void ApplyCorrosion_Hero(float percent, int turns)
    {
        hero.status.corrosionPercent = percent;
        hero.status.corrosionTurns = turns;
    }

    public void TickCorrosion_Hero()
    {
        if (hero.status.corrosionTurns <= 0) return;

        float rawDamage = hero.stats.maxHealth * hero.status.corrosionPercent;

        float mitigated = rawDamage; // IGNORA DEFENSA

        hero.ApplyDamage(mitigated);
        ShowDamageOnHero(mitigated);
        heroSpriteController.ShowHurt();
        dialogue.text += $"\n{hero.heroName} sufre corrosiÃƒÂ³n por {mitigated:F0}!";
        heroStatusEvents?.TriggerCorrosion();
        hero.status.corrosionTurns--;
        if (hero.status.corrosionTurns <= 0)
            hero.status.corrosionPercent = 0; // Ã°Å¸â€Â¥ LIMPIA
        RefreshStatusIcons(heroStatusContainer, hero.status);
    }
    public void WEB_HERO()
    {
        ApplyWeb_Hero(1);
        hero.UpdateDebugStatus("Web", 1, 0f);
        RefreshStatusIcons(heroStatusContainer, hero.status); // Ã¢Å“â€Ã¯Â¸Â AHORA SÃƒÂ APARECE EL ICONO
    }

    public void ApplyWeb_Hero(int turns)
    {
        hero.status.webbed = true;
        hero.status.webTurns = turns;
    }

    public bool CheckWeb_Hero()
    {
        if (!hero.status.webbed) return false;

        hero.status.webTurns--;
        if (hero.status.webTurns <= 0)
            hero.status.webbed = false; // Ã°Å¸â€Â¥ LIMPIA

        dialogue.text += $"\n{hero.heroName} estÃƒÂ¡ atrapado en telaraÃƒÂ±as y no puede actuar!";
        RefreshStatusIcons(heroStatusContainer, hero.status);
        return true;
    }

    public void STEALTH_HERO()
    {
        ApplyStealth_Hero(2); // ejemplo: 2 turnos
        hero.UpdateDebugStatus("Stealth", 2, 0f);
        RefreshStatusIcons(heroStatusContainer, hero.status);
    }
    public void ApplyStealth_Hero(int turns)
    {
        hero.status.stealth = true;
        hero.status.stealthTurns = turns;
    }
    public void TickStealth_Hero()
    {
        if (!hero.status.stealth) return;

        hero.status.stealthTurns--;

        if (hero.status.stealthTurns <= 0)
        {
            hero.status.stealth = false;
            dialogue.text += $"\n{hero.heroName} deja de estar en sigilo.";
        }

        RefreshStatusIcons(heroStatusContainer, hero.status);
    }
    public void FULLCOUNTER_HERO(int tier = 1)
    {
        ApplyFullCounter_Hero(1, tier); // dura 1 turno por defecto
        hero.UpdateDebugStatus("FullCounter", 1, tier);
        RefreshStatusIcons(heroStatusContainer, hero.status);
    }

    public void ApplyFullCounter_Hero(int turns, int tier)
    {
        hero.status.fullCounter = true;
        hero.status.fullCounterTurns = turns;
        hero.status.fullCounterDamageAccum = 0f;
        hero.status.fullCounterTier = tier;
    }

    public void TickFullCounter_Hero()
    {
        if (!hero.status.fullCounter) return;

        hero.status.fullCounterTurns--;

        if (hero.status.fullCounterTurns <= 0)
        {
            float accumulated = hero.status.fullCounterDamageAccum;

            if (accumulated > 0)
            {
                float multiplier = 1f;

                switch (hero.status.fullCounterTier)
                {
                    case 1: multiplier = 1f; break; // Tier 1
                    case 2: multiplier = 4f; break; // Tier 2
                    case 3: multiplier = 6f; break; // Tier 3
                }

                float finalDamage = accumulated * multiplier;

                enemy.ApplyDamage(finalDamage);
                ShowDamageOnEnemy(finalDamage);
                enemySpriteController.ShowHurt();

                dialogue.text += $"\n{hero.heroName} contraataca con {finalDamage:F0} de daÃƒÂ±o (Tier {hero.status.fullCounterTier})!";
            }

            // limpiar estado
            hero.status.fullCounter = false;
            hero.status.fullCounterDamageAccum = 0f;
            hero.status.fullCounterTier = 1;
        }

        RefreshStatusIcons(heroStatusContainer, hero.status);
    }





}