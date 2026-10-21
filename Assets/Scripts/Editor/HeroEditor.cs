using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

[CustomEditor(typeof(Hero))]
public class HeroEditor : Editor
{
    // Dibuja una propiedad de forma segura: si el campo no existe en HeroSkill
    // (fue borrado o renombrado), muestra un aviso en vez de lanzar
    // NullReferenceException en cada repintado del inspector.
    private static void SafeRelativeField(SerializedProperty parent, string fieldName, string label)
    {
        SerializedProperty prop = parent.FindPropertyRelative(fieldName);
        if (prop == null)
        {
            EditorGUILayout.HelpBox($"Campo '{fieldName}' no encontrado en HeroSkill.", MessageType.Warning);
            return;
        }
        EditorGUILayout.PropertyField(prop, new GUIContent(label));
    }

    public override void OnInspectorGUI()
    {
        Hero hero = (Hero)target;

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("⚔️ Hero Race Settings", EditorStyles.boldLabel);

        // --- MULTI-RAZA ---
        SerializedProperty racesProp = serializedObject.FindProperty("races");
        EditorGUILayout.PropertyField(racesProp, new GUIContent("Razas"), true);

        // --- MULTI-SUBRAZA (DESPLEGABLES DINÁMICOS) ---
        EditorGUILayout.LabelField("Subrazas", EditorStyles.boldLabel);

        SerializedProperty subRacesProp = serializedObject.FindProperty("subRaces");
        SerializedProperty racesProp2 = serializedObject.FindProperty("races");

        // Asegurar que ambas listas tengan el mismo tamaño
        while (subRacesProp.arraySize < racesProp2.arraySize)
            subRacesProp.InsertArrayElementAtIndex(subRacesProp.arraySize);

        while (subRacesProp.arraySize > racesProp2.arraySize)
            subRacesProp.DeleteArrayElementAtIndex(subRacesProp.arraySize - 1);

        for (int i = 0; i < subRacesProp.arraySize; i++)
        {
            EditorGUILayout.BeginVertical("box");

            // Obtener la raza correspondiente
            HeroRace race = HeroRace.DESCONOCIDO;
            if (i < racesProp2.arraySize)
                race = (HeroRace)racesProp2.GetArrayElementAtIndex(i).enumValueIndex;

            // Obtener lista de subrazas válidas
            List<string> opciones = RaceDatabase.SubRaces.ContainsKey(race)
                ? RaceDatabase.SubRaces[race]
                : new List<string> { "Ninguna" };

            // Obtener propiedad de subraza
            SerializedProperty subRaceProp = subRacesProp.GetArrayElementAtIndex(i);

            // Determinar índice actual
            int indexActual = opciones.IndexOf(subRaceProp.stringValue);
            if (indexActual < 0) indexActual = 0;

            // Mostrar popup
            int nuevoIndex = EditorGUILayout.Popup($"Subraza {i + 1}", indexActual, opciones.ToArray());

            // Guardar selección
            subRaceProp.stringValue = opciones[nuevoIndex];

            EditorGUILayout.EndVertical();
        }



        EditorGUILayout.Space();
        EditorGUILayout.LabelField("📜 Other Hero Data", EditorStyles.boldLabel);

        // --- Mostrar el resto de campos manualmente ---
        hero.attribute = (HeroAttribute)EditorGUILayout.EnumPopup("Atributo", hero.attribute);
        hero.portrait = (Sprite)EditorGUILayout.ObjectField("Portrait", hero.portrait, typeof(Sprite), false);
        hero.combatSprite = (Sprite)EditorGUILayout.ObjectField("Combat Sprite", hero.combatSprite, typeof(Sprite), false);
        hero.heroName = EditorGUILayout.TextField("Hero Name", hero.heroName);
        hero.heroID = EditorGUILayout.IntField("Hero ID", hero.heroID);
        SerializedProperty tagsProp = serializedObject.FindProperty("tags");
        EditorGUILayout.PropertyField(tagsProp, new GUIContent("Tags"), true);
        hero.isUnlocked = EditorGUILayout.Toggle("Is Unlocked", hero.isUnlocked);

        hero.Stars = EditorGUILayout.IntField("Stars", hero.Stars);

        // ❤️ Bond Level
        hero.BondLevel = EditorGUILayout.IntField("❤️ Bond Level", hero.BondLevel);

        // Lista de clases
        SerializedProperty fateClassesProp = serializedObject.FindProperty("fateClasses");
        EditorGUILayout.PropertyField(fateClassesProp, new GUIContent("Fate Classes"), true);


        // Lista de religiones
        SerializedProperty religionesProp = serializedObject.FindProperty("heroReligion");
        EditorGUILayout.PropertyField(religionesProp, new GUIContent("Hero Religion"), true);
        // Lista de alianzas
        SerializedProperty alianzasProp = serializedObject.FindProperty("alianza");
        EditorGUILayout.PropertyField(alianzasProp, new GUIContent("Hero Aliances"), true);
        // Support
        EditorGUILayout.LabelField("Configuración de Soporte", EditorStyles.boldLabel);
        hero.supportHero = (Hero)EditorGUILayout.ObjectField("Héroe de Apoyo", hero.supportHero, typeof(Hero), true);

        // Combat Class Level (readonly)
        EditorGUILayout.LabelField("Combat Class Level", hero.CC.ToString());

        // Stats
        SerializedProperty statsProp = serializedObject.FindProperty("stats");
        EditorGUILayout.PropertyField(statsProp, new GUIContent("Stats"), true);

        // Equipamiento
        SerializedProperty equippedInstancesProp = serializedObject.FindProperty("equippedInstances");
        EditorGUILayout.PropertyField(equippedInstancesProp, new GUIContent("Equipped Instances"), true);


        serializedObject.ApplyModifiedProperties();

        // --- NUEVO BLOQUE: EquipmentManager ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🛡 Equipment Manager Reference", EditorStyles.boldLabel);
        hero.EM = (EquipmentManager)EditorGUILayout.ObjectField("Equipment Manager", hero.EM, typeof(EquipmentManager), true);

        serializedObject.ApplyModifiedProperties();
        // --- Guardar cambios ---

        // --- NUEVO BLOQUE: Habilidades ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("✨ Hero Skills", EditorStyles.boldLabel);

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("✨ Hero Skills", EditorStyles.boldLabel);

        SerializedProperty skillsProp = serializedObject.FindProperty("skills");

        for (int i = 0; i < skillsProp.arraySize; i++)
        {
            SerializedProperty skillProp = skillsProp.GetArrayElementAtIndex(i);

            EditorGUILayout.BeginVertical("box");
            EditorGUILayout.LabelField($"Skill {i + 1}", EditorStyles.boldLabel);

            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("skillName"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("description"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("icon"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("targetType"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("effectType"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("basePower"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("scalingAttack"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("scalingDefense"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("animationTrigger"));

            // ⭐⭐ AQUÍ SE AÑADE EL NUEVO CAMPO ⭐⭐
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("statusEffect"), new GUIContent("Status Effect"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("stanceEffect"), new GUIContent("Stance Effect"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("damageEffect"), new GUIContent("Damage Effect"));
            // --- NUEVO SISTEMA DE SOPORTE ---
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("healAmount"), new GUIContent("Heal Amount"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("buffAttack"), new GUIContent("Buff Attack"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("buffDefense"), new GUIContent("Buff Defense"));
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("buffDuration"), new GUIContent("Buff Duration"));


            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("usableFromPositions"), true);
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("targetPositions"), true);
            EditorGUILayout.PropertyField(skillProp.FindPropertyRelative("visualPrefabs"), new GUIContent("Visual Prefabs"), true);
            if (GUILayout.Button("Eliminar Skill"))
                skillsProp.DeleteArrayElementAtIndex(i);

            EditorGUILayout.EndVertical();
            // --- SPRITES POR TIER ---
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Sprites por Tier", EditorStyles.boldLabel);

            SafeRelativeField(skillProp, "tier1Sprite", "Tier 1 Sprite");
            SafeRelativeField(skillProp, "tier2Sprite", "Tier 2 Sprite");
            SafeRelativeField(skillProp, "tier3Sprite", "Tier 3 Sprite");

        }

        if (GUILayout.Button("Añadir Nueva Skill"))
        {
            skillsProp.InsertArrayElementAtIndex(skillsProp.arraySize);
        }

        serializedObject.ApplyModifiedProperties();


        serializedObject.ApplyModifiedProperties();
        // ===============================
        // 🛡 Skills otorgadas por Equipo
        // ===============================
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🛡 Skills otorgadas por Equipo", EditorStyles.boldLabel);

        SerializedProperty equipSkillsProp = serializedObject.FindProperty("equipmentGrantedSkills");

        // Dibuja la lista completa, igual que Unity lo haría
        EditorGUILayout.PropertyField(equipSkillsProp, new GUIContent("Equipment Skills"), true);

        serializedObject.ApplyModifiedProperties();
        // --- NUEVO BLOQUE: Sprites de Combate ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🎨 Combat Sprites", EditorStyles.boldLabel);
        SerializedProperty combatSpritesProp = serializedObject.FindProperty("combatSprites");
        EditorGUILayout.PropertyField(combatSpritesProp, new GUIContent("Combat Sprites"), true);
        // --- NUEVO BLOQUE: Objetos visuales en combate ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🖼 Combat Sprite Objects", EditorStyles.boldLabel);

        serializedObject.Update(); // <-- refresca el objeto serializado
        SerializedProperty combatSpriteObjectsProp = serializedObject.FindProperty("combatSpriteObjects");
        EditorGUILayout.PropertyField(combatSpriteObjectsProp, new GUIContent("Combat Sprite Objects"), true);
        serializedObject.ApplyModifiedProperties(); // <-- guarda cambios

        // 🔥 Botones para guardar/cargar equipo con EquipmentManager (NUEVO SISTEMA)
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("💾 Guardado de Equipo", EditorStyles.boldLabel);

        if (GUILayout.Button("Save Hero Equipment"))
        {
            foreach (var inst in hero.equippedInstances)
            {
                if (inst != null)
                    EquipmentManager.Instance.EquipHero(hero, inst);
            }

            PlayerPrefs.Save();
            Debug.Log($"Equipo de {hero.heroName} guardado en PlayerPrefs (instancias).");
        }

        if (GUILayout.Button("Load Hero Equipment"))
        {
            EquipmentManager.Instance.LoadHeroEquipment(hero);
            Debug.Log($"Equipo de {hero.heroName} cargado desde PlayerPrefs (instancias).");
        }

        // --- NUEVO BLOQUE: Sistema de Skins ---
        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🎭 Sistema de Skins", EditorStyles.boldLabel);

        // Índice de skin activa
        hero.currentSkinIndex = EditorGUILayout.IntField("Skin Activa (Index)", hero.currentSkinIndex);

        serializedObject.ApplyModifiedProperties();

        EditorGUILayout.Space();
        EditorGUILayout.LabelField("🩸 Hero Status", EditorStyles.boldLabel);

        SerializedProperty statusProp = serializedObject.FindProperty("status");
        EditorGUILayout.PropertyField(statusProp, new GUIContent("Status Effects"), true);

    }

}