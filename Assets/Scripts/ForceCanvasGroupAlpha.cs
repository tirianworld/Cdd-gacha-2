using UnityEngine;

[ExecuteAlways]
public class ForceCanvasGroupAlpha : MonoBehaviour
{
    public CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void Update()
    {
        
            canvasGroup.alpha = 1f;
    }
}
