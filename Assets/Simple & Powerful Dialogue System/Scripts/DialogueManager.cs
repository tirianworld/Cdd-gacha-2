using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;

namespace RedstoneinventeGameStudio
{
    public class DialogueManager : MonoBehaviour
    {
        public static DialogueManager instance;

        [System.Serializable]
        public class DialogueLine
        {
            [TextArea(2, 5)]
            public string text;
            public UnityEvent onLineStart;
            public UnityEvent onLineEnd;
        }

        public TMP_Text title;
        public TMP_Text content;
        public Image npcImage;

        public float characterDelay = 0.1f;
        public float punctuationDelay = 0.5f;

        public Button skipNextButton;   // 🔹 Botón directo
        public Canvas dialogueCanvas;

        public List<DialogueLine> dialogueLines = new List<DialogueLine>();
        private int currentIndex = 0;

        private bool isAnimating = false;
        private bool lineFinished = false;

        private void Awake()
        {
            instance = this;
        }

        private void Start()
        {
            skipNextButton.onClick.AddListener(OnSkipOrNext);
            ShowDialogue();
        }

        /// <summary>
        /// Método que se llama desde el botón de la UI
        /// </summary>
        public void OnSkipOrNext()
        {
            if (isAnimating)
            {
                // 🔹 Mostrar todo el texto instantáneamente
                StopAllCoroutines();
                content.text = dialogueLines[currentIndex].text;

                // 🔹 Ejecutar evento de fin aunque se haya saltado
                dialogueLines[currentIndex].onLineEnd?.Invoke();

                isAnimating = false;
                lineFinished = true;

                // 🔹 Aquí NO avanzamos todavía, el jugador debe pulsar otra vez
            }
            else if (lineFinished)
            {
                // 🔹 Si ya terminó, avanzar normalmente
                AdvanceLine();
            }
        }

        /// <summary>
        /// Avanza a la siguiente línea de diálogo
        /// </summary>
        private void AdvanceLine()
        {
            lineFinished = false;

            currentIndex++;
            if (currentIndex < dialogueLines.Count)
            {
                ShowDialogue();
            }
            else
            {
                dialogueCanvas.enabled = false;
                skipNextButton.gameObject.SetActive(false);
            }
        }

        /// <summary>
        /// Inicia la rutina para mostrar la línea actual
        /// </summary>
        public void ShowDialogue()
        {
            StartCoroutine(ShowDialogueC());
        }

        IEnumerator ShowDialogueC()
        {
            if (currentIndex >= dialogueLines.Count)
            {
                dialogueCanvas.enabled = false;
                skipNextButton.gameObject.SetActive(false);
                yield break;
            }

            var currentLine = dialogueLines[currentIndex];
            content.text = "";
            dialogueCanvas.enabled = true;

            currentLine.onLineStart?.Invoke();

            isAnimating = true;
            lineFinished = false;

            skipNextButton.gameObject.SetActive(true);

            foreach (char character in currentLine.text)
            {
                content.text += character;
                yield return new WaitForSeconds(IsPunctuation(character) ? punctuationDelay : characterDelay);
            }

            currentLine.onLineEnd?.Invoke();

            isAnimating = false;
            lineFinished = true;
        }

        bool IsPunctuation(char character)
        {
            return character == '.' || character == '?' || character == '!';
        }
    }
}
