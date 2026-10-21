using UnityEngine;
using TMPro;
using System.Linq;

public class EquipmentGridSorter : MonoBehaviour
{
    [Header("GameManager (de donde se lee el equipo disponible)")]
    public GameManager gamemanager;

    [Header("Grid Parent")]
    public Transform gridParent;

    [Header("Prefab de Slot de Equipo (debe tener EquipmentSlot)")]
    public GameObject slotPrefab;

    [Header("Héroe al que se equipará (opcional)")]
    public Hero targetHero;

    [Header("Name Search Input")]
    public TMP_InputField nameInputField;

    [Header("Filtro por Tipo de Equipo")]
    public bool filtrarPorTipo = false;
    public Equipment_SO.Type currentType = Equipment_SO.Type.anillo;

    // -------------------------------
    // UNITY LIFECYCLE
    // -------------------------------

    void Start()
    {
        if (gamemanager == null)
            gamemanager = GameManager.Instance;

        if (nameInputField != null)
            nameInputField.onValueChanged.AddListener(delegate { ApplyFilters(); });

        SpawnAllEquipment();
    }

    // 🔥 NUEVO: si este grid pertenece a un panel por-héroe que se activa/
    // desactiva con SetActive() (en vez de destruirse), Start() solo corre
    // la PRIMERA vez que el panel se activa. Si esa primera vez ocurre antes
    // de que GameManager termine de cargar equipmentInventory (orden de
    // ejecución no garantizado en Unity), el grid se queda vacío para
    // siempre y nunca se reconstruye al volver a mostrar ese panel.
    // OnEnable() se dispara CADA vez que el panel se muestra, así que
    // reconstruimos el grid con los datos ya cargados.
    void OnEnable()
    {
        if (gamemanager == null)
            gamemanager = GameManager.Instance;

        SpawnAllEquipment();
    }

    void Update()
    {
        ApplyFilters();
    }

    // ------------------------------------------------------------
    //   INSTANCIAR TODO EL EQUIPO QUE SE POSEE (equipmentInventory)
    //   🔥 Un slot equipable por cada entrada real del inventario.
    //   No depende del catálogo allEquipment: si algo está en
    //   equipmentInventory, aparece en el grid.
    // ------------------------------------------------------------
    public void SpawnAllEquipment()
    {
        if (gamemanager == null)
            gamemanager = GameManager.Instance;

        if (gridParent == null || slotPrefab == null || gamemanager == null)
            return;

        // 1. Limpiar grid actual
#if UNITY_EDITOR
        // Evita que el Inspector intente redibujar un objeto que estamos a punto de destruir
        // (causa del error SerializedObjectNotCreatableException al entrar en Play).
        if (UnityEditor.Selection.activeGameObject != null
            && UnityEditor.Selection.activeGameObject.transform.IsChildOf(gridParent))
        {
            UnityEditor.Selection.activeGameObject = null;
        }
#endif
        foreach (Transform child in gridParent)
            Destroy(child.gameObject);

        // 2. Un slot equipable por CADA entrada real del inventario del jugador.
        //    🔥 Recorremos directamente gamemanager.equipmentInventory (no allEquipment),
        //    así se muestran TODAS las copias poseídas aunque el catálogo allEquipment
        //    esté incompleto o desactualizado respecto a lo que el jugador realmente tiene.
        foreach (var entry in gamemanager.equipmentInventory)
        {
            if (entry == null || entry.instance == null || entry.instance.baseData == null)
                continue;

            CreateSlot(entry.instance.baseData, entry.instance);
        }

        ApplyFilters();
    }

    // ------------------------------------------------------------
    //   CREAR UN SLOT INDIVIDUAL EN EL GRID
    // ------------------------------------------------------------
    private void CreateSlot(Equipment_SO equipmentSO, EquipmentInstance instance)
    {
        GameObject slotObj = Instantiate(slotPrefab, gridParent);
        EquipmentSlot slot = slotObj.GetComponent<EquipmentSlot>();
        if (slot == null) return;

        slot.equipmentInstance = instance;
        slot.baseEquipment = equipmentSO;
        slot.assignedHero = targetHero;
        slot.gamemanager = gamemanager;

        // 🔥 Marca este slot como perteneciente al catálogo. EquipmentSlot.EquipToHero()
        // usará esto para reconstruir vía SpawnAllEquipment() en vez de
        // RefreshSlot()/GenerateClones(), evitando slots duplicados.
        slot.gridSorter = this;

        // Cada slot ya representa UNA copia concreta del equipo,
        // no necesitamos que RefreshSlot() genere clones aquí.
        slot.isOriginal = true;

        // 🔥 Todos los slots del grid representan copias reales poseídas,
        // así que el botón de equipar siempre está habilitado.
        if (slot.equipButton != null)
            slot.equipButton.interactable = true;

        slot.UpdateUI();
    }

    // -------------------------------
    // FILTRO POR TIPO DE EQUIPO
    // -------------------------------

    public void FiltrarPorTipo(Equipment_SO.Type tipo)
    {
        currentType = tipo;
        filtrarPorTipo = true;
        ApplyFilters();
    }

    public void MostrarTodos()
    {
        filtrarPorTipo = false;
        ApplyFilters();
    }

    // Atajos de filtro por tipo (para enganchar directo a botones UI)
    public void ANILLO() => FiltrarPorTipo(Equipment_SO.Type.anillo);
    public void SELLO() => FiltrarPorTipo(Equipment_SO.Type.sello);
    public void CASCO() => FiltrarPorTipo(Equipment_SO.Type.casco);
    public void ARMA() => FiltrarPorTipo(Equipment_SO.Type.arma);
    public void ARMADURA() => FiltrarPorTipo(Equipment_SO.Type.armadura);

    // Combina filtro de tipo (currentType) + búsqueda por nombre
    private void ApplyFilters()
    {
        if (gridParent == null) return;

        string searchText = nameInputField != null ? nameInputField.text.Trim().ToLower() : "";

        foreach (Transform child in gridParent)
        {
            var slot = child.GetComponent<EquipmentSlot>();

            if (slot == null || slot.equipmentInstance == null || slot.equipmentInstance.baseData == null)
            {
                child.gameObject.SetActive(false);
                continue;
            }

            bool typeMatch = !filtrarPorTipo || slot.equipmentInstance.baseData.type == currentType;

            bool nameMatch = string.IsNullOrEmpty(searchText)
                || slot.equipmentInstance.baseData.itemName.ToLower().Contains(searchText);

            // 🔥 Se oculta la INSTANCIA concreta si está equipada por
            // CUALQUIER héroe del roster (no solo por targetHero). Así,
            // lo que no está equipado sale igual en el catálogo de todos
            // los héroes, y lo que ya está puesto por alguien no aparece
            // como disponible en el catálogo de otro.
            bool yaEquipado = slot.equipmentInstance != null
                && gamemanager.roster != null
                && gamemanager.roster.Any(h => h != null
                    && h.equippedInstances != null
                    && h.equippedInstances.Contains(slot.equipmentInstance));

            child.gameObject.SetActive(typeMatch && nameMatch && !yaEquipado);
        }
    }
}