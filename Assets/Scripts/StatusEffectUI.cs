using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;

public class StatusEffectUI : MonoBehaviour
{
    public Transform iconParent;
    public GameObject iconPrefab;

    private Dictionary<StatusEffect, GameObject> icons = new();

    public void Refresh(List<StatusEffect> effects)
    {
        foreach (var kv in icons)
            kv.Value.SetActive(false);

        foreach (var eff in effects)
        {
            if (!icons.ContainsKey(eff))
            {
                var obj = Instantiate(iconPrefab, iconParent);
                icons[eff] = obj;
            }

            var icon = icons[eff];
            icon.SetActive(true);

            icon.transform.Find("Icon").GetComponent<Image>().sprite = eff.icon;
            icon.transform.Find("Turns").GetComponent<TextMeshProUGUI>().text = eff.remainingTurns.ToString();
        }
    }
}
