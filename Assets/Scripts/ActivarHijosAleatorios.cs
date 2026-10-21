using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;

public class GachaActivator : MonoBehaviour
{
    [Header("Cantidad aleatoria a activar")]
    public int minActivar = 1;
    public int maxActivar = 5;

    [Header("Tiempo entre activaciones")]
    public float delayEntreActivaciones = 0.15f;

    [Header("Animación Gacha")]
    public float duracionAparicion = 0.35f;
    public float punchForce = 0.15f;
    public float punchDuration = 0.2f;

    void Start()
    {
        StartCoroutine(ActivarConAnimacion());
    }

    IEnumerator ActivarConAnimacion()
    {
        int total = transform.childCount;
        int cantidad = Random.Range(minActivar, Mathf.Min(maxActivar, total) + 1);

        // Desactivar todos
        for (int i = 0; i < total; i++)
            transform.GetChild(i).gameObject.SetActive(false);

        // Crear lista de índices y mezclar
        List<int> indices = new List<int>();
        for (int i = 0; i < total; i++) indices.Add(i);

        for (int i = 0; i < indices.Count; i++)
        {
            int r = Random.Range(i, indices.Count);
            (indices[i], indices[r]) = (indices[r], indices[i]);
        }

        // Activar uno por uno con animación
        for (int i = 0; i < cantidad; i++)
        {
            Transform hijo = transform.GetChild(indices[i]);
            hijo.gameObject.SetActive(true);

            // Guardar escala original
            Vector3 escalaOriginal = hijo.localScale;

            // Reset de escala para animación
            hijo.localScale = Vector3.zero;

            // Animación principal hacia la escala original
            hijo.DOScale(escalaOriginal, duracionAparicion)
                .SetEase(Ease.OutBack);

            // Punch final proporcional a la escala original
            hijo.DOPunchScale(escalaOriginal * punchForce, punchDuration, 10, 1f);

            yield return new WaitForSeconds(delayEntreActivaciones);
        }
    }
}
