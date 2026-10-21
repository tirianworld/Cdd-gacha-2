using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class GachaPityUI : MonoBehaviour
{
    public GachaManager gacha;
    [Header("UI")]
    public Slider pitySlider;
    public TextMeshProUGUI stepsText;
    public Image iconImage;
    public Sprite normalIcon;
    public Sprite softPityIcon;  // 300+
    public Sprite hardPityIcon;  // 600

    private void OnEnable()
    {
        if (gacha == null) gacha = GachaManager.Instance;
        if (gacha != null)
        {
            gacha.OnPityUpdated += HandlePityUpdated;
            // Forzar actualización inicial
            HandlePityUpdated(gacha.GetPitySteps(),
                              gacha.activeBanner.hardPitySteps,
                              gacha.GetPitySteps() >= gacha.activeBanner.softPitySteps,
                              gacha.GetPitySteps() >= gacha.activeBanner.hardPitySteps);
        }
    }

    private void OnDisable()
    {
        if (gacha != null)
            gacha.OnPityUpdated -= HandlePityUpdated;
    }

    private void HandlePityUpdated(int currentSteps, int hardPity, bool softReached, bool hardReached)
    {
        if (pitySlider != null)
        {
            pitySlider.maxValue = hardPity;
            pitySlider.value = currentSteps;
        }

        if (stepsText != null)
        {
            stepsText.text = $"{currentSteps}/{hardPity}";
        }

        if (iconImage != null)
        {
            if (hardReached && hardPityIcon != null)
                iconImage.sprite = hardPityIcon;
            else if (softReached && softPityIcon != null)
                iconImage.sprite = softPityIcon;
            else if (normalIcon != null)
                iconImage.sprite = normalIcon;
        }
    }
}
