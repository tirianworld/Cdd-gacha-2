using System;
using System.Text;
using UnityEngine;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;

public class GooglePlayCloudSave : MonoBehaviour
{
    public static GooglePlayCloudSave Instance;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else Destroy(gameObject);
    }

    private void Start()
    {
        PlayGamesPlatform.Activate();
        Social.localUser.Authenticate(success =>
        {
            Debug.Log(success ? "GPGS: Login correcto" : "GPGS: Error al iniciar sesión");
        });
    }

    // ============================================================
    // GUARDAR EN LA NUBE
    // ============================================================
    public void SaveToCloud(string fileName, string jsonData)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        savedGameClient.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    byte[] data = Encoding.UTF8.GetBytes(jsonData);

                    SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                        .WithUpdatedDescription("Última actualización: " + DateTime.Now)
                        .Build();

                    savedGameClient.CommitUpdate(game, update, data, (status2, game2) =>
                    {
                        Debug.Log(status2 == SavedGameRequestStatus.Success
                            ? "☁️ Guardado en la nube completado"
                            : "❌ Error al guardar en la nube");
                    });
                }
                else
                {
                    Debug.LogError("❌ Error al abrir archivo en la nube");
                }
            });
    }

    // ============================================================
    // CARGAR DESDE LA NUBE
    // ============================================================
    public void LoadFromCloud(string fileName, Action<string> onLoaded)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;

        savedGameClient.OpenWithAutomaticConflictResolution(
            fileName,
            DataSource.ReadCacheOrNetwork,
            ConflictResolutionStrategy.UseLongestPlaytime,
            (status, game) =>
            {
                if (status == SavedGameRequestStatus.Success)
                {
                    savedGameClient.ReadBinaryData(game, (status2, data) =>
                    {
                        if (status2 == SavedGameRequestStatus.Success)
                        {
                            string json = Encoding.UTF8.GetString(data);
                            Debug.Log("☁️ Datos cargados desde la nube");
                            onLoaded?.Invoke(json);
                        }
                        else
                        {
                            Debug.LogError("❌ Error al leer datos de la nube");
                            onLoaded?.Invoke(null);
                        }
                    });
                }
                else
                {
                    Debug.LogError("❌ Error al abrir archivo en la nube");
                    onLoaded?.Invoke(null);
                }
            });
    }
}
