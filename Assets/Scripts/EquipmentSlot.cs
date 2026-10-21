using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

// Reemplaza al array de sprites indexado "a ciegas" por el orden del enum HeroAttribute.
// Cada entrada elige explícitamente, vía desplegable en el inspector, a qué atributo
// (Arcano, Profano, Natural, Divino...) corresponde el sprite. Así el orden en la lista
// ya no importa y no hay riesgo de desincronizarse si el enum cambia.
[System.Serializable]
public struct AttributeIconEntry
{
    public HeroAttribute attribute;
    public Sprite icon;
}

public class EquipmentSlot : MonoBehaviour
{
    [Header("Referencias")]
    public Hero assignedHero;
    public EquipmentInstance equipmentInstance;
    public GameManager gamemanager;

    // 🔥 NUEVO: si este slot pertenece al catálogo (EquipmentGridSorter),
    // se asigna aquí. Cuando existe, el refresco tras equipar debe pasar
    // SIEMPRE por SpawnAllEquipment(), nunca por RefreshSlot()/GenerateClones(),
    // que son exclusivos del inventario personal del héroe.
    [Header("Catálogo (solo si este slot pertenece al EquipmentGridSorter)")]
    public EquipmentGridSorter gridSorter;

    [Header("UI")]
    public Button equipButton;
    public Image iconImage;
    public TextMeshProUGUI levelText;

    [Header("Estrellas (rango 0-5): un GameObject por estrella")]
    public GameObject[] starObjects = new GameObject[5];

    [Header("Despertar (Awaken): un GameObject por rango")]
    public GameObject awakeningR;
    public GameObject awakeningSR;
    public GameObject awakeningSSR;

    [Header("Datos del Item")]
    public Equipment_SO baseEquipment;

    [Header("Icono de Atributo Elemental")]
    // Cada entrada elige explícitamente (por desplegable) a qué HeroAttribute corresponde
    // el sprite. Ya no depende del orden del enum: se puede agregar/reordenar sin romper nada.
    public List<AttributeIconEntry> attributeIcons = new List<AttributeIconEntry>();
    // Image donde se pinta el sprite del atributo elemental del equipo mostrado en este slot.
    public Image attributeImage;

    [Header("Slot base por item")]
    public EquipmentSlot baseSlot;
    public bool isOriginal = false;

    // ============================================================
    //   🔥 NUEVO: MODO "PARA MEJORAR"
    //   Si forUpgrade = true, este slot NO equipa al héroe al pulsar
    //   el botón: en su lugar, selecciona su equipmentInstance como
    //   objetivo del EquipmentUpgradeManager (mejora de nivel/despertar).
    //   Se usa típicamente en el panel de "Mejorar Equipamento", donde
    //   el jugador elige QUÉ pieza quiere mejorar de su inventario.
    // ============================================================
    [Header("Modo Mejora (panel de mejorar equipamento)")]
    public bool forUpgrade = false;
    public EquipmentUpgradeManager upgradeManager;

    private void Start()
    {
        if (gamemanager == null)
            gamemanager = GameManager.Instance;

        // Este slot es el original si NO tiene baseSlot asignado
        isOriginal = (baseSlot == null);

        if (forUpgrade)
            equipButton.onClick.AddListener(SelectForUpgrade);
        else
            equipButton.onClick.AddListener(EquipToHero);
    }

    // ============================================================
    //   SELECCIONAR ESTE EQUIPO COMO OBJETIVO DE MEJORA
    //   (Solo para slots con forUpgrade = true)
    // ============================================================
    public void SelectForUpgrade()
    {
        if (equipmentInstance == null)
        {
            Debug.LogWarning("No hay equipo asignado en este slot para mejorar.");
            return;
        }

        // 🔥 Si el slot viene de un PREFAB clonado en runtime, el campo
        // "upgradeManager" del inspector suele venir vacío (un asset de
        // prefab no puede referenciar un objeto de la escena). Usamos el
        // singleton como respaldo antes de rendirnos.
        if (upgradeManager == null)
            upgradeManager = EquipmentUpgradeManager.Instance;

        if (upgradeManager == null)
        {
            Debug.LogWarning($"El slot '{name}' tiene forUpgrade activado pero no hay ningún EquipmentUpgradeManager asignado ni en escena.");
            return;
        }

        upgradeManager.SelectEquipment(equipmentInstance, assignedHero);
    }

    // ============================================================
    //   REFRESCAR SOLO EL SLOT BASE
    //   (Exclusivo del inventario personal del héroe, NO del catálogo)
    // ============================================================
    public void RefreshSlot()
    {
        if (!isOriginal)
            return; // ← SOLO EL ORIGINAL REFRESCA

        if (baseEquipment == null)
        {
            gameObject.SetActive(false);
            return;
        }

        // Buscar todas las instancias de este item
        var entries = gamemanager.equipmentInventory
            .Where(e => e.instance.baseData == baseEquipment)
            .ToList();

        if (entries.Count == 0)
        {
            gameObject.SetActive(false);
            return;
        }

        gameObject.SetActive(true);

        // Generar clones
        GenerateClones(entries);

        // Asignar la primera instancia al slot base
        equipmentInstance = entries[0].instance;
        UpdateUI();
    }

    // ============================================================
    //   GENERAR CLONES EXACTOS (Ctrl + D)
    // ============================================================
    private void GenerateClones(List<EquipmentInventoryEntry> entries)
    {
        // Borrar clones anteriores (solo los de ESTE mismo baseEquipment)
        for (int i = transform.parent.childCount - 1; i >= 0; i--)
        {
            var child = transform.parent.GetChild(i).GetComponent<EquipmentSlot>();
            if (child != null && !child.isOriginal && child.baseEquipment == this.baseEquipment)
                Destroy(child.gameObject);
        }

        // Crear clones para todas las instancias excepto la primera
        for (int i = 1; i < entries.Count; i++)
        {
            CloneForInstance(entries[i].instance);
        }
    }

    // ============================================================
    //   CLON EXACTO DEL SLOT (Ctrl + D)
    // ============================================================
    public EquipmentSlot CloneForInstance(EquipmentInstance inst)
    {
        GameObject cloneObj = Instantiate(gameObject, transform.parent);
        EquipmentSlot clone = cloneObj.GetComponent<EquipmentSlot>();

        clone.isOriginal = false;

        // Copiar referencias externas
        clone.assignedHero = this.assignedHero;
        clone.gamemanager = this.gamemanager;
        clone.gridSorter = this.gridSorter;

        // Copiar datos del item
        clone.baseEquipment = this.baseEquipment;
        clone.baseSlot = this.baseSlot;

        // Asignar instancia
        clone.equipmentInstance = inst;

        // Actualizar UI (NO refrescar)
        clone.UpdateUI();

        return clone;
    }

    // ============================================================
    //   ACTUALIZAR UI
    // ============================================================
    public void UpdateUI()
    {
        if (equipmentInstance == null || equipmentInstance.baseData == null) return;

        iconImage.sprite = equipmentInstance.baseData.icon;
        levelText.text = "Nv " + equipmentInstance.level;

        UpdateStars(equipmentInstance.stars);
        UpdateAwakening(equipmentInstance.baseData.rango);
        UpdateAttributeIcon(equipmentInstance.baseData.elementalAffinity);
    }

    // ============================================================
    //   ESTRELLAS: activa los primeros N GameObjects del array (0-5)
    // ============================================================
    private void UpdateStars(int stars)
    {
        int activeStars = Mathf.Clamp(stars, 0, starObjects.Length);

        for (int i = 0; i < starObjects.Length; i++)
        {
            if (starObjects[i] != null)
                starObjects[i].SetActive(i < activeStars);
        }
    }

    // ============================================================
    //   DESPERTAR: activa el GameObject del rango correspondiente
    //   y desactiva los demás. Rango viene del Equipment_SO (intrínseco
    //   al equipo, no de la instancia del jugador).
    // ============================================================
    private void UpdateAwakening(Equipment_SO.Rango rango)
    {
        bool isR = rango == Equipment_SO.Rango.R;
        bool isSR = rango == Equipment_SO.Rango.SR;
        bool isSSR = rango == Equipment_SO.Rango.SSR;

        if (awakeningR != null) awakeningR.SetActive(isR);
        if (awakeningSR != null) awakeningSR.SetActive(isSR);
        if (awakeningSSR != null) awakeningSSR.SetActive(isSSR);
    }

    // ============================================================
    //   ICONO DE ATRIBUTO ELEMENTAL
    //   Cambia la imagen según la afinidad elemental (HeroAttribute)
    //   del equipo mostrado en este slot.
    // ============================================================
    private void UpdateAttributeIcon(HeroAttribute attribute)
    {
        if (attributeImage == null)
            return;

        foreach (var entry in attributeIcons)
        {
            if (entry.attribute == attribute && entry.icon != null)
            {
                attributeImage.sprite = entry.icon;
                attributeImage.enabled = true;
                return;
            }
        }

        // No hay ninguna entrada configurada para este atributo: ocultar el icono.
        attributeImage.sprite = null;
        attributeImage.enabled = false;
    }

    // ============================================================
    //   EQUIPAR AL HÉROE
    // ============================================================
    public void EquipToHero()
    {
        if (assignedHero == null || equipmentInstance == null)
        {
            Debug.LogWarning("No hay héroe o equipo asignado.");
            return;
        }

        if (assignedHero.equippedInstances.Contains(equipmentInstance))
        {
            Debug.LogWarning($"{assignedHero.heroName} ya tiene equipado {equipmentInstance.baseData.itemName}.");
            return;
        }

        // 🔥 SEGURO DE PERSISTENCIA: si esta instancia (p.ej. viene de un slot "de vista"
        // del catálogo en EquipmentGridSorter) todavía no existe en el inventario global,
        // la registramos aquí. Sin esto, SaveHeroData_Prefs() guarda un uniqueID que
        // LoadHeroData_Prefs() nunca podrá encontrar al recargar la partida.
        if (string.IsNullOrEmpty(equipmentInstance.uniqueID))
        {
            equipmentInstance.uniqueID = System.Guid.NewGuid().ToString();
        }

        bool yaRegistrado = gamemanager.equipmentInventory
            .Any(e => e.instance.uniqueID == equipmentInstance.uniqueID);

        if (!yaRegistrado)
        {
            gamemanager.equipmentInventory.Add(new EquipmentInventoryEntry { instance = equipmentInstance });
        }

        // 🔥 Solo se puede tener UNA pieza equipada por categoría (anillo, sello, casco, arma, armadura).
        // Si ya hay algo puesto de esa categoría, lo desequipamos primero (revierte sus stats/skill)
        // y así el nuevo equipo lo reemplaza automáticamente en vez de bloquear la acción.
        var equipoExistente = assignedHero.equippedInstances
            .FirstOrDefault(i => i != null && i.baseData != null && i.baseData.type == equipmentInstance.baseData.type);

        if (equipoExistente != null)
        {
            Debug.Log($"{assignedHero.heroName} ya tenía {equipoExistente.baseData.itemName} equipado en {equipoExistente.baseData.type}. Se reemplaza por {equipmentInstance.baseData.itemName}.");
            assignedHero.Unequip(equipoExistente);
        }

        assignedHero.equippedInstances.Add(equipmentInstance);

        assignedHero.stats.maxHealth += equipmentInstance.baseData.healthBonus;
        assignedHero.stats.attack += equipmentInstance.baseData.attackBonus;
        assignedHero.stats.defense += equipmentInstance.baseData.defenseBonus;

        ApplyInstanceBonuses(assignedHero, equipmentInstance);

        if (equipmentInstance.baseData.grantedHeroSkill != null)
            assignedHero.AddEquipmentSkill(equipmentInstance.baseData.grantedHeroSkill);

        assignedHero.UpdateCombatClass();

        // 🔥 Ya NO eliminamos el ítem de gamemanager.equipmentInventory aquí.
        // Se queda en la lista global para que al cargar partida el sistema pueda encontrar el ID.

        assignedHero.SaveHeroData_Prefs();
        gamemanager.SaveEquipmentInventory();
        gamemanager.SaveEquipment();

        // 🔥 FIX: si este slot pertenece al catálogo (EquipmentGridSorter),
        // reconstruimos el catálogo entero con SpawnAllEquipment(), que limpia
        // y recrea correctamente todos los slots (soporta múltiples copias).
        // NUNCA usamos RefreshSlot()/GenerateClones() para slots del catálogo:
        // ese flujo es exclusivo del inventario personal del héroe y provocaba
        // slots duplicados cada vez que se equipaba desde el catálogo.
        if (gridSorter != null)
        {
            gridSorter.SpawnAllEquipment();
        }
        else if (isOriginal)
        {
            RefreshSlot();
        }
        else
        {
            baseSlot.RefreshSlot();
        }

        // 🔥 CLAVE: refrescar el panel de "equipo puesto" (EquippedEquipmentSlot) del héroe.
        // Sin esto, el icono/estrellas del slot equipado no se actualizan hasta que
        // el objeto se desactiva/reactiva (OnEnable) o se recarga la escena.
        gamemanager.RefreshEquippedSlots(assignedHero);
    }

    private void ApplyInstanceBonuses(Hero hero, EquipmentInstance inst)
    {
        hero.stats.maxHealth += inst.level * 2f;
        hero.stats.attack += inst.stars * 1.5f;
        hero.stats.defense += inst.awakening * 3f;
    }
}