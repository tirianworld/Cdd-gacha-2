using UnityEngine;

[System.Serializable]
public class EquipmentInstance
{
    public string uniqueID;        // 🔥 ID único persistente
    public Equipment_SO baseData;

    public int level = 1;
    public int stars = 0;
    public int awakening = 0;

    public EquipmentInstance(Equipment_SO data)
    {
        baseData = data;

        // Si no existe ID, generar uno nuevo
        uniqueID = System.Guid.NewGuid().ToString();
    }
}
