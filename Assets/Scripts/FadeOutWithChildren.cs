using UnityEngine;
using DG.Tweening;
using System.Collections.Generic;

public class FadeOutWithChildren : MonoBehaviour
{
    public float fadeDuration = 1f;

    private bool isFading = false;
    private bool pendingDisable = false;
    private List<Material> allMaterials = new List<Material>();

    void Awake()
    {
        // Guardar todos los materiales del objeto y sus hijos
        Renderer[] renderers = GetComponentsInChildren<Renderer>();

        foreach (Renderer r in renderers)
        {
            foreach (Material m in r.materials)
            {
                allMaterials.Add(m);
            }
        }
    }

    // Este método se ejecuta cuando alguien intenta activar/desactivar el objeto
    void OnEnable()
    {
        // Si estaba pendiente de desactivarse, lo hacemos ahora
        if (pendingDisable)
        {
            pendingDisable = false;
            StartFadeOut();
        }
    }

    void OnDisable()
    {
        // Si se está desactivando sin fade, lo interceptamos
        if (!isFading)
        {
            pendingDisable = true;

            // Reactivar temporalmente para poder hacer el fade
            gameObject.SetActive(true);

            StartFadeOut();
        }
    }

    private void StartFadeOut()
    {
        if (isFading) return;
        isFading = true;

        Sequence seq = DOTween.Sequence();

        foreach (Material mat in allMaterials)
        {
            SetMaterialToFadeMode(mat);

            Color c = mat.color;
            seq.Join(mat.DOColor(new Color(c.r, c.g, c.b, 0f), fadeDuration));
        }

        seq.OnComplete(() =>
        {
            gameObject.SetActive(false);
        });
    }

    private void SetMaterialToFadeMode(Material mat)
    {
        if (mat.HasProperty("_Mode"))
        {
            mat.SetFloat("_Mode", 2); // Fade
            mat.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);
            mat.SetInt("_DstBlend", (int)UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha);
            mat.SetInt("_ZWrite", 0);
            mat.DisableKeyword("_ALPHATEST_ON");
            mat.EnableKeyword("_ALPHABLEND_ON");
            mat.DisableKeyword("_ALPHAPREMULTIPLY_ON");
            mat.renderQueue = 3000;
        }
    }
}
