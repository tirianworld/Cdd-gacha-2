using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Genera en un grid la lista visual de materiales requeridos para la mejora
/// actual (nivel, estrella o despertar) de un EquipmentUpgrader. Los materiales
/// se resuelven automáticamente según el Equipment_SO.Type del ítem que esté
/// asignado como objetivo en el upgrader (anillo/sello/casco/arma/armadura
/// tienen listas independientes).
///
/// Uso: coloca este componente en el panel de mejora, asigna el
/// EquipmentUpgrader correspondiente, el prefab del slot (con
/// MaterialRequirementSlot) y el gridParent. Llama a SpawnMaterials()
/// cada vez que cambie el objetivo (SetTarget) o el tipo de mejora
/// seleccionado.
/// </summary>
public class EquipmentUpgradeMaterialsPanel : MonoBehaviour
{
    [Header("Upgrader del que se leen los requisitos")]
    public EquipmentUpgrader upgrader;

    [Header("Qué etapa de mejora mostrar")]
    public EquipmentUpgrader.UpgradeStage upgradeStage = EquipmentUpgrader.UpgradeStage.Level;

    [Header("Grid Parent")]
    public Transform gridParent;

    [Header("Prefab de slot de material (debe tener MaterialRequirementSlot)")]
    public GameObject slotPrefab;

    private void OnEnable()
    {
        SpawnMaterials();
    }

    // Enganchar a botones de pestaña (Nivel / Estrella / Despertar)
    public void MostrarMaterialesDeNivel()
    {
        upgradeStage = EquipmentUpgrader.UpgradeStage.Level;
        SpawnMaterials();
    }

    public void MostrarMaterialesDeEstrella()
    {
        upgradeStage = EquipmentUpgrader.UpgradeStage.Star;
        SpawnMaterials();
    }

    public void MostrarMaterialesDeDespertar()
    {
        upgradeStage = EquipmentUpgrader.UpgradeStage.Awakening;
        SpawnMaterials();
    }

    // ------------------------------------------------------------
    //   INSTANCIAR UN SLOT POR CADA MATERIAL REQUERIDO
    // ------------------------------------------------------------
    public void SpawnMaterials()
    {
        if (gridParent == null || slotPrefab == null || upgrader == null)
            return;

        // Limpiar grid actual
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        // 🔥 Los materiales dependen del tipo de equipo del objetivo actual
        // (anillo/sello/casco/arma/armadura tienen listas distintas).
        List<EquipmentUpgrader.UpgradeRequirement> materials =
            upgrader.GetMaterialsForCurrentTarget(upgradeStage);

        if (materials == null) return;

        foreach (var req in materials)
        {
            if (req.item == null) continue;

            GameObject slotObj = Instantiate(slotPrefab, gridParent);
            MaterialRequirementSlot slot = slotObj.GetComponent<MaterialRequirementSlot>();
            if (slot == null) continue;

            // 🔥 Asigna el Item_SO del requisito -> el slot actualiza su ícono solo
            slot.SetRequirement(req.item, req.amount);
        }
    }
}
