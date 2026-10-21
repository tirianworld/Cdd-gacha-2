using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PatrolResult
{
    public bool success;
    public int goldEarned;
    public int xpEarned;
    public List<Item_SO> foundItems = new List<Item_SO>();
    public List<string> log = new List<string>();
}
