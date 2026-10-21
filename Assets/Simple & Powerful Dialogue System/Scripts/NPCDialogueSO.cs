using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace RedstoneinventeGameStudio
{
    [CreateAssetMenu(fileName = "NPCDialogueSO", menuName = "Simple Dialogue System/NPC Dialogue")]
    public class NPCDialogueSO : ScriptableObject
    {
        public string title;
        public string lines;
        public Sprite npcSprite; // Sprite del NPC
    }
}
