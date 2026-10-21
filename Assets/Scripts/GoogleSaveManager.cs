using UnityEngine;
using System.Collections.Generic;
using TMPro;
using GooglePlayGames;
using GooglePlayGames.BasicApi;
using GooglePlayGames.BasicApi.SavedGame;
using System.Text;
using System;

public class GoogleSaveManager : MonoBehaviour
{
    // Referencia al GameManager para obtener los datos
    public GameManager gameManager;

    // Clase serializable para estructurar los datos en JSON [1]
    [System.Serializable]
    public class SaveData
    {
        public int gold;
        public int memoryCristals;
        public int astralShards;
        public string playerName;
        public List<GameManager.GachaHistoryEntry> gachaHistory;
    }

    // Nombre del archivo en la nube de Google [1]
    private string fileName = "game_save_data.json";

    public void SaveGame()
    {
        if (Social.localUser.authenticated)
        {
            // 1. Preparar los datos desde el GameManager
            SaveData data = new SaveData
            {
                gold = gameManager.Gold,
                memoryCristals = gameManager.MemoryCristals,
                astralShards = gameManager.AstralShards,
                playerName = gameManager.PlayerName,
                gachaHistory = gameManager.gachaHistory
            };

            // 2. Convertir a JSON y luego a bytes [2]
            string json = JsonUtility.ToJson(data);
            byte[] dataToSave = Encoding.UTF8.GetBytes(json);

            // 3. Iniciar el proceso de guardado en Google Play
            OpenSavedGame(fileName, true, dataToSave);
            Debug.Log("Iniciando guardado de datos: " + json);
        }
        else
        {
            Debug.LogError("El usuario no está autenticado en Google Play.");
        }
    }

    public void LoadGame()
    {
        if (Social.localUser.authenticated)
        {
            OpenSavedGame(fileName, false, null);
        }
    }

    // Método para abrir el archivo en la nube
    private void OpenSavedGame(string filename, bool isSaving, byte[] data)
    {
        ISavedGameClient savedGameClient = PlayGamesPlatform.Instance.SavedGame;
        savedGameClient.OpenWithManualConflictResolution(filename, DataSource.ReadCacheOrNetwork,
            true, ResolveConflict, (status, game) => OnSavedGameOpened(status, game, isSaving, data));
    }

    private void OnSavedGameOpened(SavedGameRequestStatus status, ISavedGameMetadata game, bool isSaving, byte[] data)
    {
        if (status == SavedGameRequestStatus.Success)
        {
            if (isSaving)
            {
                // Realizar el guardado físico [2]
                SavedGameMetadataUpdate update = new SavedGameMetadataUpdate.Builder()
                    .WithUpdatedDescription("Saved at " + DateTime.Now.ToString())
                    .Build();

                PlayGamesPlatform.Instance.SavedGame.CommitUpdate(game, update, data, (statusUpdate, metadata) => {
                    if (statusUpdate == SavedGameRequestStatus.Success)
                        Debug.Log("¡Datos guardados con éxito en Google!");
                });
            }
            else
            {
                // Realizar la carga de datos [2]
                PlayGamesPlatform.Instance.SavedGame.ReadBinaryData(game, (statusRead, loadedData) => {
                    if (statusRead == SavedGameRequestStatus.Success)
                    {
                        string json = Encoding.UTF8.GetString(loadedData);
                        SaveData loadedSave = JsonUtility.FromJson<SaveData>(json);

                        // Aplicar datos al GameManager
                        gameManager.Gold = loadedSave.gold;
                        gameManager.MemoryCristals = loadedSave.memoryCristals;
                        gameManager.AstralShards = loadedSave.astralShards;
                        gameManager.PlayerName = loadedSave.playerName;
                        gameManager.gachaHistory = loadedSave.gachaHistory;

                        Debug.Log("Datos cargados correctamente: " + json);
                    }
                });
            }
        }
    }

    private void ResolveConflict(IConflictResolver resolver, ISavedGameMetadata original, byte[] originalData, ISavedGameMetadata unmerged, byte[] unmergedData)
    {
        // Resolución simple: se queda con el archivo más reciente
        resolver.ChooseMetadata(original);
    }
}