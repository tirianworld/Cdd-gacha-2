using UnityEngine;

public class CameraController : MonoBehaviour
{
    [Header("Configuración")]
    public float panSpeed = 0.5f;       // velocidad de arrastre
    public float zoomSpeed = 5f;        // velocidad de zoom
    public float minZoom = 5f;          // zoom mínimo
    public float maxZoom = 50f;         // zoom máximo

    private Camera cam;
    private Vector3 lastPanPosition;
    private int panFingerId; // ID del dedo que arrastra
    private bool wasZoomingLastFrame;
    private Vector2[] lastZoomPositions;

    void Awake()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
#if UNITY_EDITOR || UNITY_STANDALONE
        HandleMouse();
#endif
        if (Application.isMobilePlatform)
        {
            HandleTouch();
        }
    }

    // -------------------
    // RATÓN
    // -------------------
    void HandleMouse()
    {
        // Arrastre con botón izquierdo
        if (Input.GetMouseButtonDown(0))
        {
            lastPanPosition = Input.mousePosition;
        }
        else if (Input.GetMouseButton(0))
        {
            PanCamera(Input.mousePosition);
        }

        // Zoom con rueda
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        ZoomCamera(scroll * zoomSpeed);
    }

    // -------------------
    // TOUCH
    // -------------------
    void HandleTouch()
    {
        if (Input.touchCount == 1)
        {
            // Pan
            Touch touch = Input.GetTouch(0);

            if (touch.phase == TouchPhase.Began)
            {
                lastPanPosition = touch.position;
                panFingerId = touch.fingerId;
            }
            else if (touch.fingerId == panFingerId && touch.phase == TouchPhase.Moved)
            {
                PanCamera(touch.position);
            }

            wasZoomingLastFrame = false;
        }
        else if (Input.touchCount == 2)
        {
            // Zoom
            Vector2[] newPositions = new Vector2[] {
                Input.GetTouch(0).position,
                Input.GetTouch(1).position
            };

            if (!wasZoomingLastFrame)
            {
                lastZoomPositions = newPositions;
                wasZoomingLastFrame = true;
            }
            else
            {
                float oldDistance = Vector2.Distance(lastZoomPositions[0], lastZoomPositions[1]);
                float newDistance = Vector2.Distance(newPositions[0], newPositions[1]);
                float offset = newDistance - oldDistance;

                ZoomCamera(offset * 0.01f); // factor de sensibilidad

                lastZoomPositions = newPositions;
            }
        }
        else
        {
            wasZoomingLastFrame = false;
        }
    }

    // -------------------
    // FUNCIONES COMUNES
    // -------------------
    void PanCamera(Vector3 newPanPosition)
    {
        Vector3 offset = cam.ScreenToViewportPoint(lastPanPosition - newPanPosition);
        Vector3 move = new Vector3(offset.x * panSpeed, offset.y * panSpeed, 0);

        transform.Translate(move, Space.World);
        lastPanPosition = newPanPosition;
    }

    void ZoomCamera(float offset)
    {
        if (offset == 0) return;

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - offset, minZoom, maxZoom);
    }
}
