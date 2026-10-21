using UnityEngine;
using UnityEngine.Events;

public class RunOnceOnFirstLaunch : MonoBehaviour
{
    [Header("Evento que se ejecutará solo la primera vez")]
    public UnityEvent onFirstLaunch;

    private const string FirstLaunchKey = "Game_FirstLaunchDone";

    private void Start()
    {
        // Si NO existe la clave, significa que es la primera vez
        if (!PlayerPrefs.HasKey(FirstLaunchKey))
        {
            // Ejecutamos el evento
            onFirstLaunch?.Invoke();

            // Guardamos que ya se ejecutó
            PlayerPrefs.SetInt(FirstLaunchKey, 1);
            PlayerPrefs.Save();
        }
    }
}
