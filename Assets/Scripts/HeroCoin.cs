using UnityEngine;

[System.Serializable]
public class HeroCoin
{
    public int heroID;      // ID del héroe al que pertenece esta moneda
    public int amount;      // Cantidad de monedas

    public HeroCoin(int heroID, int amount = 0)
    {
        this.heroID = heroID;
        this.amount = amount;
    }
}
