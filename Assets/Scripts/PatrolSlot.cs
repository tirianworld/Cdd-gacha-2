using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PatrolSlot
{
    public bool occupied = false;
    public PatrolMission_SO mission;
    public List<Hero> heroes = new List<Hero>();
    public float endTimeUnix = 0f; // Time.time at completion
    public bool autoClaim = false;

    public float RemainingTime => Mathf.Max(0f, endTimeUnix - Time.time);
    public bool IsComplete => Time.time >= endTimeUnix && occupied;
    public void ClearSlot(){
        occupied = false;
        mission = null;
        heroes.Clear();
        endTimeUnix = 0f;
        autoClaim = false;
    }
}
