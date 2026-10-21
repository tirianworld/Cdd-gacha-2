using UnityEngine;
using UnityEngine.EventSystems;

public class HeroAssignmentButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    public float tiempoHold = 0.5f; // Umbral para "mantener pulsado"
    private float tiempoPresionado;
    private bool accionEjecutada;

    public HeroAssignmentManager manager;
    public Hero heroToAssign;
    public GameObject UpgradeMenu;
    public bool IsForEquipHeroes;

    [Range(1, 4)]
    public int slotNumber;

    // ↓↓↓ Detecta cuando el botón se presiona ↓↓↓
    public void OnPointerDown(PointerEventData eventData)
    {
        tiempoPresionado = Time.time;
        accionEjecutada = false;
    }

    // ↓↓↓ Detecta cuando el botón se suelta ↓↓↓
    public void OnPointerUp(PointerEventData eventData)
    {
        float duracion = Time.time - tiempoPresionado;

        if (!accionEjecutada)
        {
           
                PulsarAccion();
        }
    }

    void PulsarAccion()
    {
        if (IsForEquipHeroes == true)
        {
            Hero heroActual = manager.GetHeroByIndex(slotNumber - 1);

            // Si ya está asignado este héroe en este slot → DESASIGNAR
            if (heroActual == heroToAssign)
            {
                manager.AssignOrSwapHero(slotNumber - 1, null); // Deja el hueco vacío
            }
            else
            {
                // Si no está asignado → asignarlo normalmente
                AssignThisHero();
            }
        }
        else if (IsForEquipHeroes == false)
        {
            // Aquí puedes poner otra lógica si lo necesitas
        }
    }





    public void AssignThisHero()
    {
        if (manager != null && heroToAssign != null)
        {
            manager.AssignOrSwapHero(slotNumber - 1, heroToAssign);
        }
        else
        {
            Debug.LogWarning("Faltan referencias en HeroAssignmentButton.");
        }
    }

}






