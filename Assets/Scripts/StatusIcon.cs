using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StatusIcon : MonoBehaviour
{
    [Header("UI")]
    public Image iconImage;
    public TextMeshProUGUI turnsText;

    [Header("Sprites")]
    public Sprite poisonSprite;
    public Sprite bleedSprite;
    public Sprite burnSprite;
    public Sprite shockSprite;
    public Sprite freezeSprite;
    public Sprite petrifySprite;
    public Sprite stunSprite;
    public Sprite corrosionSprite;
    public Sprite webSprite;
    public Sprite sealSprite;
    public Sprite stealthSprite;
    public Sprite fullCounterSprite;

    public void SetIcon(string statusName, int turns)
    {
        // Mostrar turnos
        turnsText.text = turns.ToString();

        // Elegir sprite según el estado
        switch (statusName)
        {
            case "Poison": iconImage.sprite = poisonSprite; break;
            case "Bleed": iconImage.sprite = bleedSprite; break;
            case "Burn": iconImage.sprite = burnSprite; break;
            case "Shock": iconImage.sprite = shockSprite; break;
            case "Freeze": iconImage.sprite = freezeSprite; break;
            case "Petrify": iconImage.sprite = petrifySprite; break;
            case "Stun": iconImage.sprite = stunSprite; break;
            case "Corrosion": iconImage.sprite = corrosionSprite; break;
            case "Web": iconImage.sprite = webSprite; break;
            case "Seal": iconImage.sprite = sealSprite; break;
            case "Stealth": iconImage.sprite = stealthSprite; break;
            case "FullCounter": iconImage.sprite = fullCounterSprite; break;

            default:
                Debug.LogWarning("Estado no reconocido: " + statusName);
                break;
        }
    }
}
