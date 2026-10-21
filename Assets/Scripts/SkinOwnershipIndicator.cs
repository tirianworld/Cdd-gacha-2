using UnityEngine;

/// <summary>
/// Activa un conjunto de objetos distinto según el estado de una skin concreta:
/// bloqueada (no unlocked), desbloqueada pero sin equipar, o equipada.
/// Los tres estados son excluyentes: solo el conjunto correspondiente queda activo.
///
/// Uso: coloca este componente en cada fila/botón de skin de tu UI (o en la escena)
/// y asigna el índice de la skin que representa. Se actualiza solo en cada frame,
/// o puedes llamar a Refresh() manualmente (por ejemplo, justo después de UnlockSkin/EquipSkin).
/// </summary>
public class SkinOwnershipIndicator : MonoBehaviour
{
    [Tooltip("Referencia al SkinManager que controla las skins del héroe.")]
    public SkinManager skinManager;

    [Tooltip("Índice de la skin (dentro de skinManager.skins) que representa este objeto.")]
    public int skinIndex;

    [Header("Si SE TIENE la skin pero NO está equipada (unlocked = true, equipped = false)")]
    [Tooltip("Objetos que se activan cuando la skin está desbloqueada pero no equipada. Se desactivan en cualquier otro estado.")]
    public GameObject[] ownedObjects;

    [Header("Si NO se tiene la skin (unlocked = false)")]
    [Tooltip("Objetos que se activan cuando la skin todavía no está desbloqueada. Se desactivan en cualquier otro estado.")]
    public GameObject[] lockedObjects;

    [Header("Si la skin está EQUIPADA (equipped = true)")]
    [Tooltip("Objetos que se activan cuando la skin está equipada. Se desactivan en cualquier otro estado.")]
    public GameObject[] equippedObjects;

    private void Update()
    {
        Refresh();
    }

    // Puedes llamar a esto manualmente (p. ej. desde SkinChanger tras UnlockSkin/EquipSkin)
    // si prefieres no depender de Update().
    public void Refresh()
    {
        if (skinManager == null || skinManager.skins == null) return;
        if (skinIndex < 0 || skinIndex >= skinManager.skins.Length) return;

        bool unlocked = skinManager.skins[skinIndex].unlocked;
        bool equipped = skinManager.skins[skinIndex].equipped;

        SetActiveAll(lockedObjects, !unlocked);
        SetActiveAll(ownedObjects, unlocked && !equipped);
        SetActiveAll(equippedObjects, unlocked && equipped);
    }

    private void SetActiveAll(GameObject[] objects, bool active)
    {
        if (objects == null) return;

        foreach (var obj in objects)
        {
            if (obj != null && obj.activeSelf != active)
                obj.SetActive(active);
        }
    }
}