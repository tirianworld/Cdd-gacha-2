using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteInEditMode]
public class PatrolUIBootstrap : MonoBehaviour
{
    public PatrolManager_v2 patrolManager;
    [HideInInspector] public GameObject canvasObj;
    [HideInInspector] public PatrolFormationUI formationUI;
    [HideInInspector] public HeroSelectionMenu selectionMenu;

#if UNITY_EDITOR
    [ContextMenu("Generar Interfaz de Patrulla TMP")]
    public void GenerateInterface()
    {
        if (patrolManager == null)
        {
            Debug.LogError("Asigna un PatrolManager_v2 antes de generar la interfaz.");
            return;
        }

        ClearOldUI();
        CreateCanvas();
        CreateFormationUI();
        CreateHeroSelectionMenu();
        LinkMenus();

        Debug.Log("✅ Interfaz TMP generada correctamente en el editor.");
    }

    void ClearOldUI()
    {
        if (canvasObj != null)
            DestroyImmediate(canvasObj);
    }

    void CreateCanvas()
    {
        canvasObj = new GameObject("PatrolCanvas_TMP");
        var canvas = canvasObj.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvasObj.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        canvasObj.AddComponent<GraphicRaycaster>();

        canvasObj.transform.SetParent(transform, false);
    }

    void CreateFormationUI()
    {
        var panel = new GameObject("FormationScreen", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        panel.transform.SetParent(canvasObj.transform, false);
        var img = panel.GetComponent<Image>();
        img.color = new Color(0.12f, 0.12f, 0.12f, 0.9f);

        formationUI = panel.AddComponent<PatrolFormationUI>();
        formationUI.patrolManager = patrolManager;

        var layout = panel.AddComponent<VerticalLayoutGroup>();
        layout.childAlignment = TextAnchor.MiddleCenter;
        layout.spacing = 10f;

        // Slots de héroe
        formationUI.heroSlots = new Button[4];
        for (int i = 0; i < 4; i++)
        {
            var slotObj = new GameObject($"HeroSlot_{i}", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            slotObj.transform.SetParent(panel.transform, false);

            var imgSlot = slotObj.GetComponent<Image>();
            imgSlot.color = new Color(0.2f, 0.2f, 0.2f, 1f);

            // Texto con TMP
            var textObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
            textObj.transform.SetParent(slotObj.transform, false);
            var txt = textObj.GetComponent<TextMeshProUGUI>();
            txt.text = "Vacío";
            txt.alignment = TextAlignmentOptions.Center;
            txt.fontSize = 24;
            txt.color = Color.white;

            formationUI.heroSlots[i] = slotObj.GetComponent<Button>();
        }

        // Botón de iniciar
        var startBtnObj = new GameObject("StartButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        startBtnObj.transform.SetParent(panel.transform, false);
        startBtnObj.GetComponent<Image>().color = new Color(0.1f, 0.6f, 1f, 1f);

        var startTextObj = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        startTextObj.transform.SetParent(startBtnObj.transform, false);
        var txt2 = startTextObj.GetComponent<TextMeshProUGUI>();
        txt2.text = "Comenzar Patrulla";
        txt2.alignment = TextAlignmentOptions.Center;
        txt2.fontSize = 28;
        txt2.color = Color.white;

        formationUI.startButton = startBtnObj.GetComponent<Button>();
    }

    void CreateHeroSelectionMenu()
    {
        var menu = new GameObject("HeroSelectionMenu_TMP", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        menu.transform.SetParent(canvasObj.transform, false);
        var img = menu.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.8f);

        selectionMenu = menu.AddComponent<HeroSelectionMenu>();

        var scrollObj = new GameObject("ScrollView", typeof(ScrollRect), typeof(RectTransform));
        scrollObj.transform.SetParent(menu.transform, false);
        var scrollRect = scrollObj.GetComponent<ScrollRect>();
        var content = new GameObject("Content", typeof(RectTransform), typeof(GridLayoutGroup));
        content.transform.SetParent(scrollObj.transform, false);
        scrollRect.content = content.GetComponent<RectTransform>();
        scrollRect.vertical = true;

        var grid = content.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(160, 60);
        grid.spacing = new Vector2(5, 5);
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 2;

        selectionMenu.heroGridParent = content.transform;

        // Prefab de botón TMP
        var heroBtnPrefab = new GameObject("HeroButtonPrefab_TMP", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
        var label = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        label.transform.SetParent(heroBtnPrefab.transform, false);
        var txt = label.GetComponent<TextMeshProUGUI>();
        txt.text = "Hero";
        txt.alignment = TextAlignmentOptions.Center;
        txt.fontSize = 20;
        txt.color = Color.white;

        heroBtnPrefab.SetActive(false);
        selectionMenu.heroButtonPrefab = heroBtnPrefab;
    }

    void LinkMenus()
    {
        formationUI.heroMenu = selectionMenu;
    }
#endif
}
