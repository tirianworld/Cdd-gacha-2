using UnityEngine;

[CreateAssetMenu(menuName = "SDP/XP Potion", fileName = "NewXPPotion")]
public class XPPotion_SO : ScriptableObject
{
    public string potionName;
    public Sprite icon;
    public int xpValue;
}
