using System.Collections.Generic;
using UnityEngine;

public class HeroGridManager : MonoBehaviour
{
    [System.Serializable]
    public class HeroSlotData
    {
        public string heroName;
        public Sprite lockedSprite;
        public Sprite unlockedSprite;
        public bool unlocked = false;
        public Hero heroRef;
    }

    [Header("Referencias")]
    public GameObject heroSlotPrefab;
    public Transform gridParent; // Panel con GridLayoutGroup

    [Header("Datos")]
    public List<HeroSlotData> heroSlots = new List<HeroSlotData>();

    void Start()
    {
        GenerateGrid();
    }

    void GenerateGrid()
    {
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        foreach (var slotData in heroSlots)
        {
            GameObject slotObj = Instantiate(heroSlotPrefab, gridParent);
            HeroSlotUI slotUI = slotObj.GetComponent<HeroSlotUI>();
            slotUI.Setup(slotData.unlocked ? slotData.heroRef : null, slotData.lockedSprite, slotData.unlockedSprite);
        }
    }

    public void UnlockHero(string heroName, Hero heroInstance)
    {
        var slot = heroSlots.Find(h => h.heroName == heroName);
        if (slot != null && !slot.unlocked)
        {
            slot.unlocked = true;
            slot.heroRef = heroInstance;
            GenerateGrid(); // refresca la vista
            Debug.Log($"Héroe desbloqueado: {heroName}");
        }
    }
}
