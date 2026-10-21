using UnityEngine;
using UnityEngine.U2D.Animation;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine.SceneManagement;

namespace SpriteSkinUtility
{
    public static class SpriteSkinFixer
    {
        [MenuItem("Tools/Sprite Skin/Fix All SpriteSkins in Active Scene")]
        public static void FixAllSpriteSkins()
        {
            var scene = SceneManager.GetActiveScene();
            if (!scene.isLoaded)
            {
                Debug.LogError("No active scene loaded. Cannot fix SpriteSkins.");
                return;
            }

            // Undo group for proper editor undo support
            Undo.SetCurrentGroupName("Fix SpriteSkins");
            int undoGroup = Undo.GetCurrentGroup();

            var allSpriteSkins = GameObject.FindObjectsOfType<SpriteSkin>();
            int processedCount = 0;
            foreach (var skin in allSpriteSkins)
            {
                // Ensure component is enabled
                if (!skin.enabled)
                {
                    Undo.RecordObject(skin, "Enable SpriteSkin");
                    skin.enabled = true;
                }

                // Force skinning (use reflection for older Unity versions)
                var type = skin.GetType();
                var forceProp = type.GetProperty("forceSkinning");
                if (forceProp != null && forceProp.CanWrite)
                {
                    bool current = (bool)forceProp.GetValue(skin, null);
                    if (!current)
                    {
                        Undo.RecordObject(skin, "Enable ForceSkinning");
                        forceProp.SetValue(skin, true, null);
                    }
                }

                // Ensure SpriteRenderer has a sprite (copy from SpriteSkin if missing)
                var sr = skin.GetComponent<SpriteRenderer>();
                if (sr != null && sr.sprite == null)
{
    // Try to get sprite from SpriteSkin via reflection
    Sprite fallbackSprite = null;
    var skinType = skin.GetType();
    var spriteProp = skinType.GetProperty("sprite");
    if (spriteProp != null)
        fallbackSprite = (Sprite)spriteProp.GetValue(skin, null);
    else
    {
        var spriteField = skinType.GetField("m_Sprite");
        if (spriteField != null)
            fallbackSprite = (Sprite)spriteField.GetValue(skin);
    }
    if (fallbackSprite != null)
    {
        Undo.RecordObject(sr, "Assign missing sprite");
        sr.sprite = fallbackSprite;
    }
}

                // Ensure rootBone is assigned (fallback to own transform)
                if (skin.rootBone == null)
                {
                    Undo.RecordObject(skin, "Assign root bone");
                    // Use reflection to set rootBone if possible
var rootProp = skin.GetType().GetProperty("rootBone");
if (rootProp != null && rootProp.CanWrite)
{
    var currentRoot = rootProp.GetValue(skin, null) as Transform;
    if (currentRoot == null)
    {
        Undo.RecordObject(skin, "Assign root bone");
        rootProp.SetValue(skin, skin.transform, null);
    }
}
                }

                // Force a skinning update – public method available in most versions
                // Force a skinning update via reflection (compatible across Unity versions)
var forceMethod = skin.GetType().GetMethod("ForceSkinning") ??
                  skin.GetType().GetMethod("ForceDeform") ??
                  skin.GetType().GetMethod("Deform");
if (forceMethod != null)
    forceMethod.Invoke(skin, null);
else
{
    // Fallback: toggle enabled to trigger a refresh
    skin.enabled = false;
    skin.enabled = true;
}
                EditorUtility.SetDirty(skin);
                processedCount++;
            }

            // Mark scene dirty so Unity prompts to save
            if (processedCount > 0)
                EditorSceneManager.MarkSceneDirty(scene);

            Debug.Log($"SpriteSkinFixer: Processed {processedCount} SpriteSkin component(s) in scene '{scene.name}'.");

            Undo.CollapseUndoOperations(undoGroup);
        }
    }
}
