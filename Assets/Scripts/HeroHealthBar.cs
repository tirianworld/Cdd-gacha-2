using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class HeroHealthBar : MonoBehaviour
{
    [SerializeField] private Slider slider;

    public CombatManager combatManager;
    public Hero heroActual;

    private float lastHealth;

    private void Awake()
    {
        combatManager = FindObjectOfType<CombatManager>();
    }

    private void Update()
    {
        if (combatManager == null) return;

        // Detectar si el héroe cambió (por muerte o swap)
        if (heroActual != combatManager.hero)
        {
            heroActual = combatManager.hero;

            if (heroActual != null)
            {
                slider.maxValue = heroActual.stats.maxHealth;
                slider.value = heroActual.stats.currentHealth;
                lastHealth = heroActual.stats.currentHealth;
            }
        }

        if (heroActual == null) return;

        float currentHealth = heroActual.stats.currentHealth;

        // Si la vida bajó → animar
        if (currentHealth < lastHealth)
        {
            AnimateDamage(currentHealth);
        }
        else
        {
            // Si sube, solo animamos suavemente sin shake
            slider.DOValue(currentHealth, 0.25f);
        }

        lastHealth = currentHealth;
    }

    private void AnimateDamage(float newValue)
    {
        // Shake del objeto de la barra
      

        // Animación suave al nuevo valor
        slider.DOValue(newValue, 0.35f).SetEase(Ease.OutCubic);
    }
}
