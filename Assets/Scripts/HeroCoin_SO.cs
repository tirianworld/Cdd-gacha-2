using UnityEngine;

[CreateAssetMenu(fileName = "NewHeroCoin", menuName = "Gacha/Hero Coin")]
public class HeroCoin_SO : ScriptableObject
{
    public int heroID;              // ID del héroe al que pertenece esta moneda
    public string coinName = "Hero Coin";
    public Sprite icon;             // Sprite de la moneda
    public string description = "Moneda usada para mejorar al héroe.";
}
