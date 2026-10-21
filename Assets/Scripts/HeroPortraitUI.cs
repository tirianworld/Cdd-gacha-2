using UnityEngine;
using UnityEngine.UI;

public class HeroPortraitUI : MonoBehaviour
{
    [Header("Referencia de la UI")]
    public Image targetImage;
    public HeroAssignmentManager HAM;

    [Header("Sprites de héroes")]
    private Sprite hero1Sprite;
    private Sprite hero2Sprite;
    private Sprite hero3Sprite;
    private Sprite hero4Sprite;

    [Header("Bools de activación (solo uno activo)")]
    public bool hero1Active;
    public bool hero2Active;
    public bool hero3Active;
    public bool hero4Active;

    void Update()
    {
        if (HAM == null || targetImage == null) return;

        // Actualiza sprites solo si hay héroes asignados
        if (HAM.Hero1 != null) hero1Sprite = HAM.Hero1.portrait;
        if (HAM.Hero2 != null) hero2Sprite = HAM.Hero2.portrait;
        if (HAM.Hero3 != null) hero3Sprite = HAM.Hero3.portrait;
        if (HAM.Hero4 != null) hero4Sprite = HAM.Hero4.portrait;

        Sprite newSprite = null;
        if (hero1Active) newSprite = hero1Sprite;
        else if (hero2Active) newSprite = hero2Sprite;
        else if (hero3Active) newSprite = hero3Sprite;
        else if (hero4Active) newSprite = hero4Sprite;

        if (targetImage.sprite != newSprite)
            targetImage.sprite = newSprite;
    }

       
    

    // Método auxiliar opcional para cambiar héroe activamente desde otros scripts
    public void SetActiveHero(int heroIndex)
    {
        hero1Active = heroIndex == 1;
        hero2Active = heroIndex == 2;
        hero3Active = heroIndex == 3;
        hero4Active = heroIndex == 4;
    }
}
