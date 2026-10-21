using UnityEngine;

public class HeroSwitcher : MonoBehaviour
{
    [Header("Nuevo héroe a asignar")]
    public Hero newHero;
    public HeroVisual newHeroVisual;

    [Header("Referencias del combate")]
    public CombatManager combatManager;
    public HandManager handManager;

    [Header("Roster (se busca automáticamente si se deja vacío)")]
    public HeroAssignmentManager assignmentManager;

    public void ApplyHeroChange()
    {
        if (newHero == null || newHeroVisual == null)
        {
            Debug.LogError("HeroSwitcher: Falta asignar newHero o newHeroVisual.");
            return;
        }

        if (newHero.stats.currentHealth <= 0)
        {
            Debug.LogWarning($"No se puede cambiar a {newHero.heroName} porque está muerto.");
            return;
        }

        Hero previousHero = combatManager.hero;
        HeroVisual previousVisual = combatManager.herovisual;

        // ============================
        // 1. Cambiar héroe en CombatManager
        // ============================
        combatManager.hero = newHero;
        combatManager.herovisual = newHeroVisual;
        combatManager.heroSpriteController = newHeroVisual.spriteController;

        // Cambiar sprite de combate
        if (combatManager.heroCombatSprite != null)
            combatManager.heroCombatSprite.sprite =
                newHero.combatSprite != null ? newHero.combatSprite : newHero.portrait;

        // ============================
        // 2. Cambiar héroe en HandManager
        // ============================
        handManager.hero = newHero;
        handManager.combat = combatManager;

        // Resetear mano para evitar errores
        handManager.hand.Clear();
        handManager.GenerateInitialHand();
        handManager.RefreshHandUI();

        // ============================
        // 3. Actualizar HeroVisual y su CombatSpriteController
        // ============================
        // FIX: antes solo se actualizaba spriteController.hero, pero el propio
        // campo "hero" del HeroVisual (el que usa CombatManager para resetear
        // vidas al terminar el combate) se quedaba apuntando al héroe viejo.
        newHeroVisual.hero = newHero;
        newHeroVisual.spriteController.hero = newHero;
        combatManager.RefreshHeroStatusUI();

        // Ocultar el visual anterior si era uno distinto, para no dejarlo
        // "colgado" y visible en escena
        if (previousVisual != null && previousVisual != newHeroVisual)
            previousVisual.gameObject.SetActive(false);

        newHeroVisual.gameObject.SetActive(true);

        // ============================
        // 4. Sincronizar con HeroAssignmentManager
        // ============================
        // FIX: antes el swap nunca se reflejaba en el roster (Hero1..Hero4),
        // así que el reseteo de vida al terminar el combate (que recorre
        // assign.GetHeroByIndex(i)) podía no encontrar al héroe recién metido
        // y dejarlo "muerto" también en el siguiente combate.
        if (assignmentManager == null)
            assignmentManager = FindObjectOfType<HeroAssignmentManager>();

        if (assignmentManager != null)
        {
            assignmentManager.ReplaceHeroInCombat(previousHero, newHero);
        }
        else
        {
            Debug.LogWarning("HeroSwitcher: No se encontró HeroAssignmentManager en escena; " +
                              "el roster no quedará sincronizado con el cambio de héroe.");
        }

        Debug.Log($"HeroSwitcher: Héroe cambiado correctamente a {newHero.heroName}");
    }
}