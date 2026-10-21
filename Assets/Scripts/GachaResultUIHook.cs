using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class GachaResultUIHook : MonoBehaviour
{
    public GachaManager gachaManager;
    public TextMeshProUGUI logText;

    public void PullSingleButton()
    {
        if (!gachaManager.CanPullSingle())
        {
            AppendLog("No tienes recursos para tirar 1 vez.");
            return;
        }

        var res = gachaManager.PullSingle();
        ShowResult(res);
    }

    public void PullMultiButton()
    {
        if (!gachaManager.CanPullMulti())
        {
            AppendLog("No tienes recursos para el multi.");
            return;
        }

        var results = gachaManager.PullMulti();
        foreach (var r in results)
            ShowResult(r);
    }

    private void ShowResult(GachaPullResult res)
    {
        if (res.hero == null)
        {
            AppendLog("Error en la tirada (héroe null).");
            return;
        }

        string rarityStr = res.rarity.ToString();
        string newOrDupe = res.isNew ? "NUEVO" : "DUPLICADO";
        string pity = res.triggeredPity ? " (PITY)" : "";
        string rateUp = res.isRateUp ? " [RATE-UP]" : "";

        AppendLog($"{rarityStr} - {res.hero.heroName} - {newOrDupe}{rateUp}{pity}");
    }

    private void AppendLog(string msg)
    {
        if (logText == null) return;
        logText.text += msg + "\n";
    }
}
