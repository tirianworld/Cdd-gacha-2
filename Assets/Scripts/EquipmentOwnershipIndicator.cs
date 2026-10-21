using UnityEngine;
using System.Linq;

/// <summary>
/// Activa un conjunto de objetos distinto según el estado de un equipo concreto:
/// bloqueado (no está en el inventario de cuenta), desbloqueado pero no equipado
/// por el héroe indicado, o equipado por ese héroe.
/// Los tres estados son excluyentes: solo el conjunto correspondiente queda activo.
///
/// Uso: coloca este componente en cada fila/botón de equipo de tu UI y asigna:
///  - equipmentData: el Equipment_SO que representa este objeto.
///  - hero: el héroe sobre el que se comprueba si está equipado (déjalo vacío
///    si solo te interesa saber si el jugador lo posee, sin importar el héroe).
/// Se actualiza solo en cada frame, o puedes llamar a Refresh() manualmente
/// (por ejemplo, justo después de EquipHero/UnequipHero/AddToAccount).
/// </summary>
public class EquipmentOwnershipIndicator : MonoBehaviour
{
    [Tooltip("Referencia al EquipmentManager. Si se deja vacío, usa EquipmentManager.Instance.")]
    public EquipmentManager equipmentManager;

    [Tooltip("Equipment_SO (base data) que representa este objeto.")]
    public Equipment_SO equipmentData;

    [Tooltip("Héroe sobre el que se comprueba si el equipo está equipado. Si se deja vacío, 'equipped' se ignora y el objeto se considera como mucho 'owned'.")]
    public Hero hero;

    [Header("Si SE TIENE el equipo pero NO está equipado por el héroe indicado")]
    [Tooltip("Objetos que se activan cuando el equipo está en el inventario de cuenta pero no equipado. Se desactivan en cualquier otro estado.")]
    public GameObject[] ownedObjects;

    [Header("Si NO se tiene el equipo (no está en accountEquipment)")]
    [Tooltip("Objetos que se activan cuando el equipo todavía no ha sido conseguido. Se desactivan en cualquier otro estado.")]
    public GameObject[] lockedObjects;

    [Header("Si el equipo está EQUIPADO por el héroe indicado")]
    [Tooltip("Objetos que se activan cuando el equipo está equipado por 'hero'. Se desactivan en cualquier otro estado.")]
    public GameObject[] equippedObjects;

    private void Awake()
    {
        if (equipmentManager == null)
            equipmentManager = EquipmentManager.Instance;
    }

    private void Update()
    {
        Refresh();
    }

    // Puedes llamar a esto manualmente (p. ej. desde tu UI de equipo tras
    // EquipHero/UnequipHero/AddToAccount) si prefieres no depender de Update().
    public void Refresh()
    {
        if (equipmentManager == null || equipmentData == null) return;

        bool unlocked = equipmentManager.accountEquipment
            .Any(inst => inst != null && inst.baseData == equipmentData);

        bool equipped = hero != null && hero.equippedInstances != null &&
            hero.equippedInstances.Any(inst => inst != null && inst.baseData == equipmentData);

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