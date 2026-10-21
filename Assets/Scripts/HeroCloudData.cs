using System;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class HeroCloudData
{
    public int heroID;

    public int level;
    public float maxHealth;
    public float attack;
    public float defense;
    public float currentHealth;
    public float stamina;

    public int bondLevel;
    public int stars;
    public int skinIndex;

    public long saveTimestamp;

    public List<string> equippedIDs = new List<string>();

    public List<HeroRace> races = new List<HeroRace>();
    public List<string> subRaces = new List<string>();
    public List<HeroClass> classes = new List<HeroClass>();

    public HeroReligion religion;
    public Alianza alianza;
}
