using System.Collections.Generic;

public static class RelationshipDatabase
{
    // Negative values: penalties to success chance.
    public static readonly Dictionary<(HeroReligion, HeroReligion), float> ReligionConflicts =
        new Dictionary<(HeroReligion, HeroReligion), float>
    {
        { (HeroReligion.LuzSolar, HeroReligion.LunaOscura), +15f },
        { (HeroReligion.LunaOscura, HeroReligion.LuzSolar), -15f },
        { (HeroReligion.CultoALosDragones, HeroReligion.Piedad), 0f },
        { (HeroReligion.Piedad, HeroReligion.CultoALosDragones), 0f },
        { (HeroReligion.LosTresSables, HeroReligion.Piedad), -15f },
        { (HeroReligion.Piedad, HeroReligion.LosTresSables), -15f },
    };

    public static readonly Dictionary<(HeroRace, HeroRace), float> RaceConflicts =
        new Dictionary<(HeroRace, HeroRace), float>
    {
        { (HeroRace.Elf, HeroRace.Dwarf), -10f },
        { (HeroRace.Dwarf, HeroRace.Elf), -10f },
        { (HeroRace.Human, HeroRace.HalfOrc), -8f },
        { (HeroRace.HalfOrc, HeroRace.Human), -8f },
        { (HeroRace.Tiefling, HeroRace.Aasimar), -12f },
        { (HeroRace.Aasimar, HeroRace.Tiefling), -12f },
    };

    public static readonly Dictionary<(HeroClass, HeroClass), float> ClassConflicts =
        new Dictionary<(HeroClass, HeroClass), float>
    {
        { (HeroClass.Clerigo, HeroClass.Brujo), 0f },
        { (HeroClass.Brujo, HeroClass.Clerigo), 0f },
        { (HeroClass.Paladin, HeroClass.BlackGuard), -14f },
        { (HeroClass.BlackGuard, HeroClass.Paladin), -14f },
        { (HeroClass.Bardo, HeroClass.Barbaro), +10f },
        { (HeroClass.Barbaro, HeroClass.Bardo), +10f },
    };
}
