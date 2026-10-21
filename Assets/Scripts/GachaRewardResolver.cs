using UnityEngine;

public static class GachaRewardResolver
{
    // Config "tipo 7DS": dupes ? suben Stars hasta 5, luego dan AstralShards
    public static void ResolveDuplicate(Hero hero, GachaRarity rarity, GameManager gm)
    {
        if (hero == null || gm == null) return;

        // Cantidad de monedas según rareza
        int coins = 0;
        switch (rarity)
        {
            case GachaRarity.R: coins = 1; break;
            case GachaRarity.SR: coins = 1; break;
            case GachaRarity.SSR: coins = 1; break;
        }

        gm.AddHeroCoins(hero.heroID, coins);

        Debug.Log($"Duplicado de {hero.heroName}. Recibes {coins} monedas del héroe (HeroCoin_{hero.heroID}).");
    }

}
