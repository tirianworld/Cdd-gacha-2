using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class EnemyHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public CombatManager combatManager;
    public Enemy enemyActual;

    private float lastHealth;

    private void Awake()
    {
        combatManager = FindObjectOfType<CombatManager>();
    }

    private void Update()
    {
        if (combatManager == null) return;

        // Detectar si el enemigo cambió (nuevo enemigo en combate)
        if (enemyActual != combatManager.enemy)
        {
            enemyActual = combatManager.enemy;

            if (enemyActual != null)
            {
                slider.maxValue = enemyActual.stats.maxHealth;
                slider.value = enemyActual.stats.currentHealth;
                lastHealth = enemyActual.stats.currentHealth;
            }
        }

        if (enemyActual == null) return;

        float currentHealth = enemyActual.stats.currentHealth;

        // Si la vida baja → animación suave
        if (currentHealth < lastHealth)
        {
            slider.DOValue(currentHealth, 0.35f).SetEase(Ease.OutCubic);
        }
        else
        {
            // Si sube (curación), también animamos suavemente
            slider.DOValue(currentHealth, 0.25f);
        }

        lastHealth = currentHealth;
    }
}
