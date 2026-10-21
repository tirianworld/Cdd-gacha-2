using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;

public class CardDropHandler : MonoBehaviour, IDropHandler
{
    public int cardIndex;
    public HandManager handManager;

    public void OnDrop(PointerEventData eventData)
    {
        // Asegurar que realmente se soltó encima
        if (!RectTransformUtility.RectangleContainsScreenPoint(
                GetComponent<RectTransform>(),
                eventData.position,
                eventData.pressEventCamera))
            return;

        var drag = eventData.pointerDrag?.GetComponent<CardDragHandler>();
        if (drag == null || drag == this.GetComponent<CardDragHandler>())
            return;

        drag.droppedOnCard = true;

        int from = drag.cardIndex;
        int to = cardIndex;

        // Asegurar rango válido
        to = Mathf.Clamp(to, 0, handManager.hand.Count - 1);

        // Actualizar el índice del objeto arrastrado
        drag.cardIndex = to;

        // Mover visualmente
        drag.transform.SetParent(handManager.handPanel, false);
        drag.transform.SetSiblingIndex(to);

        // Mover en la lista lógica
        handManager.MoveCardInternal(from, to);

        StartCoroutine(DelayedMerge(to));

    }


    private IEnumerator DelayedRefresh()
    {
        yield return new WaitForEndOfFrame();   // ← más seguro
        yield return null;                      // ← doble frame para garantizar fin del drag
        handManager.RefreshHandUI();
    }
    private IEnumerator DelayedMerge(int index)
    {
        // Espera a que el drag termine
        yield return new WaitUntil(() => !CardDragHandler.IsDragging);

        // Espera un frame extra para seguridad
        yield return null;

        handManager.TryManualMerge(index);
        StartCoroutine(DelayedRefreshAfterMerge());
    }
    private IEnumerator DelayedRefreshAfterMerge()
    {
        yield return null; // esperar 1 frame
        yield return null; // esperar 2 frames (importante)
        handManager.RefreshHandUI();
    }


}
