using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class PatrolManager_v2 : MonoBehaviour
{
    [Header("Missions")]
    public List<PatrolMission_SO> missions = new List<PatrolMission_SO>();
    public List<PatrolSlot> slots = new List<PatrolSlot>();

    [Header("Roster & Inventory")]
    public List<Hero> roster = new List<Hero>();
    public Inventory globalInventory = new Inventory();

    [Header("Limits")]
    public int maxConcurrentSlots = 3;
    public int dailyDispatchLimit = 10;
    private int dailyDispatchCount = 0;

    [Header("Constant Patrol")]
    [Tooltip("A persistent meter that fills over time and grants a selectable reward.")]
    public float constantPatrolMeter = 0f; // 0..1
    public float constantPatrolFillPerHour = 0.04f; // fill rate per real hour
    public PatrolMission_SO constantPatrolReward;

    public delegate void PatrolCompleted(PatrolResult result, PatrolSlot slot);
    public event PatrolCompleted OnPatrolCompleted;

    private void Awake(){
        // ensure slots initialized
        while(slots.Count < maxConcurrentSlots){
            slots.Add(new PatrolSlot());
        }
    }

    private void Update(){
        // auto-claim slots that have autoClaim = true and are complete
        for(int i=0;i<slots.Count;i++){
            var s = slots[i];
            if(s.occupied && s.IsComplete && s.autoClaim){
                var res = ResolveSlotRewards(s);
                OnPatrolCompleted?.Invoke(res, s);
                ReleaseHeroesFromSlot(s);
                s.ClearSlot();
            }
        }

        // fill constant patrol gradually
        constantPatrolMeter = Mathf.Clamp01(constantPatrolMeter + constantPatrolFillPerHour * (Time.deltaTime / 3600f));
    }

    public bool CanDispatch(PatrolMission_SO mission){
        if(dailyDispatchLimit > 0 && dailyDispatchCount >= dailyDispatchLimit) return false;
        return slots.Any(s => !s.occupied);
    }

    // Try to dispatch heroes to a mission. Returns true if assigned successfully.
    public bool DispatchToMission(PatrolMission_SO mission, List<Hero> heroes, bool autoClaim = false){
        if(mission == null || heroes == null) return false;
        if(heroes.Count < mission.requiredTeamSize) return false;
        // ensure heroes available
        foreach(var h in heroes){
            if(h.IsExhausted) return false; // cannot dispatch exhausted or currently on mission
            if(roster.Contains(h) == false) return false;
        }
        var slot = slots.FirstOrDefault(s => !s.occupied);
        if(slot == null) return false;

        // occupy slot
        slot.occupied = true;
        slot.mission = mission;
        slot.heroes = new List<Hero>(heroes);
        slot.endTimeUnix = Time.time + mission.durationSeconds;
        slot.autoClaim = autoClaim;

        // mark heroes as 'dispatched' by setting a flag (we'll use stamina lock and invisible component)
        foreach(var h in heroes){
            h.gameObject.AddComponent<DispatchedMarker>().slot = slot;
        }

        dailyDispatchCount++;
        return true;
    }

    // Auto-assign best available heroes up to requirement
    public List<Hero> AutoAssignForMission(PatrolMission_SO mission){
        var candidates = roster.Where(h => !h.IsExhausted && h.GetComponent<DispatchedMarker>()==null).OrderByDescending(h => h.stats.level + h.stats.attack).ToList();
        return candidates.Take(mission.requiredTeamSize).ToList();
    }

    // Claim results for a specific slot (manual)
    public bool ClaimSlot(int slotIndex){
        if(slotIndex < 0 || slotIndex >= slots.Count) return false;
        var s = slots[slotIndex];
        if(!s.occupied || !s.IsComplete) return false;
        var res = ResolveSlotRewards(s);
        OnPatrolCompleted?.Invoke(res, s);
        ReleaseHeroesFromSlot(s);
        s.ClearSlot();
        return true;
    }

    // Internal reward resolution (damage, stamina, xp, item finds)
    private PatrolResult ResolveSlotRewards(PatrolSlot slot){
        PatrolResult result = new PatrolResult();
        result.success = true;
        int totalXp = 0;
        int totalGold = 0;

        var mission = slot.mission;
        foreach(var h in slot.heroes){
            if(h==null) continue;
            float staminaCost = mission.staminaCostPerHero * (1f + (mission.baseDifficulty*0.2f)) * (1f + (h.stats.level-1)*0.05f);
            h.UseStamina(staminaCost);

            // damage calculation simpler: scale with difficulty and subtract defense
            float hitPower = mission.baseDifficulty * 12f;
            float damage = Mathf.Max(0f, hitPower - h.stats.defense * 0.6f);
            damage *= Mathf.Max(0.5f, 1f - (h.stats.level-1)*0.03f);
            h.ApplyDamage(damage);

            int xp = Mathf.RoundToInt(mission.baseXpReward * mission.baseDifficulty * (1f + h.stats.level*0.03f));
            h.GainXP(xp);
            totalXp += xp;
            totalGold += Mathf.RoundToInt(mission.baseGoldReward * mission.baseDifficulty);

            result.log.Add($"{h.heroName} consumió {staminaCost:F1} stamina y recibió {damage:F1} daño durante '{mission.missionName}'.");

            if(h.IsExhausted){
                result.log.Add($"{h.heroName} está exhausto y necesitará descansar.");
            }
        }

        // items found
        if(Random.value < mission.findItemChance && mission.possibleFinds != null && mission.possibleFinds.Length>0){
            var pick = mission.possibleFinds[Random.Range(0, mission.possibleFinds.Length)];
            if(pick != null){
                globalInventory.AddItem(pick);
                result.foundItems.Add(pick);
                result.log.Add($"Se encontró {pick.itemName}.");
            }
        }

        result.xpEarned = totalXp;
        result.goldEarned = totalGold;
        return result;
    }

    private void ReleaseHeroesFromSlot(PatrolSlot slot){
        foreach(var h in slot.heroes){
            if(h==null) continue;
            var mark = h.GetComponent<DispatchedMarker>();
            if(mark!=null) Destroy(mark);
        }
    }

    // For the player to claim the constant patrol reward when meter full
    public bool ClaimConstantPatrol(){
        if(constantPatrolMeter < 1f) return false;
        if(constantPatrolReward == null) return false;
        // grant reward instantly (resolve like a very short mission)
        PatrolResult res = new PatrolResult();
        res.success = true;
        res.goldEarned = constantPatrolReward.baseGoldReward;
        res.xpEarned = constantPatrolReward.baseXpReward;
        if(constantPatrolReward.possibleFinds != null && constantPatrolReward.possibleFinds.Length>0){
            var pick = constantPatrolReward.possibleFinds[Random.Range(0, constantPatrolReward.possibleFinds.Length)];
            if(pick!=null){ globalInventory.AddItem(pick); res.foundItems.Add(pick); }
        }
        constantPatrolMeter = 0f;
        OnPatrolCompleted?.Invoke(res, null);
        return true;
    }

    // Utility to cancel an occupied slot and return heroes (with partial penalties)
    public bool CancelSlot(int slotIndex){
        if(slotIndex<0 || slotIndex>=slots.Count) return false;
        var s = slots[slotIndex];
        if(!s.occupied) return false;
        // apply partial stamina usage (half) and some XP penalty maybe
        foreach(var h in s.heroes){
            if(h==null) continue;
            h.UseStamina(s.mission.staminaCostPerHero * 0.5f);
            // small morale message
        }
        ReleaseHeroesFromSlot(s);
        s.ClearSlot();
        return true;
    }

    // Reset daily counters (call from a daily reset manager)
    public void ResetDaily(){
        dailyDispatchCount = 0;
    }
}
