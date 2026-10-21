using UnityEngine;
using System.Linq;

public static class EnemyHeroExtensions
{
    private static int turnCount = 0;

    public static int GetTurnCount()
    {
        return turnCount;
    }

    public static void IncrementTurn()
    {
        turnCount++;
    }
}
