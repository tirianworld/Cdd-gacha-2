using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Linq;

public class GachaCardResultUI : MonoBehaviour
{
    public GachaManager gacha;
    public Transform cardsParent;
    public GameObject cardPrefab;

    private void OnEnable()
    {
        if (gacha == null) gacha = GachaManager.Instance;
        if (gacha != null)
            gacha.OnSummonFinished += HandleSummonFinished;
    }

    private void OnDisable()
    {
        if (gacha != null)
            gacha.OnSummonFinished -= HandleSummonFinished;
    }

    private void HandleSummonFinished(GachaPullResult[] results)
    {
        // Limpiar cartas previas
        foreach (Transform child in cardsParent)
            Destroy(child.gameObject);

        foreach (var res in results)
        {
            if (res.hero == null) continue;
            CreateCard(res);
        }
    }

    private void CreateCard(GachaPullResult res)
    {
        GameObject card = Instantiate(cardPrefab, cardsParent);

        // Asume que el prefab tiene estos componentes opcionales:
        var nameText = card.GetComponentInChildren<TextMeshProUGUI>();
        var portraitImage = card.GetComponentInChildren<Image>();

        if (nameText != null)
        {
            string flags = "";
            if (res.isNew) flags += " [NUEVO]";
            else flags += " [DUP]";
            if (res.isRateUp) flags += " [RATE-UP]";
            if (res.triggeredPity) flags += " [PITY]";

            nameText.text = $"{res.rarity} - {res.hero.heroName}{flags}";
        }

        // Puedes usar el portrait del héroe o un sprite de rareza
        if (portraitImage != null)
        {
            // portrait por defecto
            portraitImage.sprite = res.hero.portrait;
        }

        // Aquí puedes cambiar el borde/efecto según rareza:
        // - SSR ? color dorado, glow, etc.
        // - SR ? púrpura
        // - R ? azul
        if (!res.isNew)
        {
            var coinSO = gacha.gameManager.heroCoinCatalog.FirstOrDefault(c => c.heroID == res.hero.heroID);

            if (coinSO != null)
                nameText.text += $" (+1 {coinSO.coinName})";
        }


    }

}
