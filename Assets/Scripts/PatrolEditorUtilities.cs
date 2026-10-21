#if UNITY_EDITOR
using UnityEngine;
using UnityEditor;
public class PatrolEditorUtilities
{
    [MenuItem("SDP/Create Sample Assets/Patrol Mission Example")]
    public static void CreateSamplePatrolMission(){
        var asset = ScriptableObject.CreateInstance<PatrolMission_SO>();
        asset.missionName = "Forest Watch";
        asset.description = "Patrol the nearby woods for bandit activity.";
        asset.requiredTeamSize = 3;
        asset.durationSeconds = 60f * 30f; // 30 minutes
        asset.baseDifficulty = 2;
        asset.baseGoldReward = 200;
        asset.baseXpReward = 80;
        asset.staminaCostPerHero = 25f;
        AssetDatabase.CreateAsset(asset, "Assets/SDP/Sample_PatrolForest.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        Debug.Log("Sample PatrolMission created at Assets/SDP/Sample_PatrolForest.asset");
    }

    [MenuItem("SDP/Create Sample Assets/Equipment Example")]
    public static void CreateSampleEquipment(){
        var asset = ScriptableObject.CreateInstance<Equipment_SO>();
        asset.itemName = "Leveled Leather Armor";
        asset.description = "A simple but sturdy leather armor.";
        asset.healthBonus = 15f;
        asset.attackBonus = 0f;
        asset.defenseBonus = 5f;
        AssetDatabase.CreateAsset(asset, "Assets/SDP/Sample_LeatherArmor.asset");
        AssetDatabase.SaveAssets();
        EditorUtility.FocusProjectWindow();
        Selection.activeObject = asset;
        Debug.Log("Sample Equipment created at Assets/SDP/Sample_LeatherArmor.asset");
    }
}
#endif
