using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using UnityEngine.SocialPlatforms;

public class GooglePlayLogin : MonoBehaviour
{
    void Start()
    {
        PlayGamesPlatform.Activate();
        SignIn();
    }

    public void SignIn()
    {
        PlayGamesPlatform.Instance.Authenticate(ProcessAuthentication);
    }

    private void ProcessAuthentication(SignInStatus status)
    {
        if (status == SignInStatus.Success)
        {
            Debug.Log("✔️ Sesión iniciada: " + Social.localUser.userName);
        }
        else
        {
            Debug.Log("❌ Error al iniciar sesión: " + status);
        }
    }

    // No existe logout real en GPGS v2
    public void FakeLogout()
    {
        Debug.Log("⚠️ Google Play Games no permite cerrar sesión desde Unity.");
        Debug.Log("El usuario debe cerrar sesión desde la app de Google Play Games.");
    }
}
