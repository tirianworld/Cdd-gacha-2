using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events; // Necesario para UnityEvent

public class LockPick : MonoBehaviour
{
    public Camera cam;
    public Transform innerLock;
    public Transform pickPosition;

    public float maxAngle = 90;
    public float lockSpeed = 10;

    [Range(1, 25)]
    public float lockRange = 10;

    private float eulerAngle;
    private float unlockAngle;
    private Vector2 unlockRange;

    private float keyPressTime = 0;
    private bool movePick = true;

    // --- Nuevo: evento público ---
    public UnityEvent OnUnlock;

    void Start()
    {
        newLock();
    }

    void Update()
    {
        transform.localPosition = pickPosition.position;

        // --- PC (mouse/teclado) ---
        if (movePick && Input.touchCount == 0)
        {
            Vector3 dir = Input.mousePosition - cam.WorldToScreenPoint(transform.position);
            RotatePick(dir);
        }

        if (Input.GetKeyDown(KeyCode.D))
        {
            movePick = false;
            keyPressTime = 1;
        }
        if (Input.GetKeyUp(KeyCode.D))
        {
            movePick = true;
            keyPressTime = 0;
        }

        // --- Móvil (touch) ---
        if (Input.touchCount > 0)
        {
            Touch touch = Input.GetTouch(0);
            Vector2 dir = touch.position - (Vector2)cam.WorldToScreenPoint(transform.position);
            RotatePick(dir);

            if (touch.phase == TouchPhase.Began)
            {
                movePick = false;
                keyPressTime = 1;
            }
            if (touch.phase == TouchPhase.Ended)
            {
                movePick = true;
                keyPressTime = 0;
            }
        }

        // --- Rotación de la cerradura ---
        float percentage = Mathf.Round(100 - Mathf.Abs(((eulerAngle - unlockAngle) / 100) * 100));
        float lockRotation = ((percentage / 100) * maxAngle) * keyPressTime;
        float maxRotation = (percentage / 100) * maxAngle;

        float lockLerp = Mathf.Lerp(innerLock.eulerAngles.z, lockRotation, Time.deltaTime * lockSpeed);
        innerLock.eulerAngles = new Vector3(0, 0, lockLerp);

        if (lockLerp >= maxRotation - 1)
        {
            if (eulerAngle < unlockRange.y && eulerAngle > unlockRange.x)
            {
                // --- Aquí se dispara el evento ---
                OnUnlock?.Invoke();

                newLock();
                movePick = true;
                keyPressTime = 0;
            }
            else
            {
                float randomRotation = Random.insideUnitCircle.x;
                transform.eulerAngles += new Vector3(0, 0, Random.Range(-randomRotation, randomRotation));
            }
        }
    }

    void RotatePick(Vector2 dir)
    {
        Vector3 dir3 = new Vector3(dir.x, dir.y, 0);

        eulerAngle = Vector3.Angle(dir3, Vector3.up);
        Vector3 cross = Vector3.Cross(Vector3.up, dir3);
        if (cross.z < 0) { eulerAngle = -eulerAngle; }

        eulerAngle = Mathf.Clamp(eulerAngle, -maxAngle, maxAngle);
        Quaternion rotateTo = Quaternion.AngleAxis(eulerAngle, Vector3.forward);
        transform.rotation = rotateTo;
    }

    void newLock()
    {
        unlockAngle = Random.Range(-maxAngle + lockRange, maxAngle - lockRange);
        unlockRange = new Vector2(unlockAngle - lockRange, unlockAngle + lockRange);
    }
}
