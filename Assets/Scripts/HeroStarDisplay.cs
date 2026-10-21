using UnityEngine;
using UnityEngine.UI;

public class HeroStarDisplay : MonoBehaviour
{
    [Header("Heroe objetivo")]
    public Hero targetHero;

    [Header("Sprites de estrellas")]
    public Image[] starImages; // Array de imágenes de estrellas (ej. 5 slots)
    public Sprite activeStarSprite;
    public Sprite inactiveStarSprite;

    private void Update()
    {
        if (targetHero == null || starImages.Length == 0) return;

        // ?? Leer cuántas estrellas tiene el héroe
        int currentStars = targetHero.Stars;

        // ?? Activar/desactivar sprites en consecuencia
        for (int i = 0; i < starImages.Length; i++)
        {
            if (i < currentStars)
                starImages[i].sprite = activeStarSprite;   // Estrella encendida
            else
                starImages[i].sprite = inactiveStarSprite; // Estrella apagada
        }
    }
}
