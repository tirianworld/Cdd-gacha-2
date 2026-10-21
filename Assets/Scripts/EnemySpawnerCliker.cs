using UnityEngine;


public class EnemySpawnerCliker : MonoBehaviour
{
    public static EnemySpawnerCliker Instance;

    public ClikerEnemy[] enemyPool; // PREFABS, no objetos de escena
    public Transform enemyContainer; // un panel vacío en la UI

    public int enemiesPerRoom = 10;

    private int killedInRoom = 0;
    public ClikerEnemy currentEnemy;
    public RectTransform enemyFreeArea; // arrástralo desde el inspector
    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SpawnEnemy();
    }

    public void SpawnEnemy()
    {
        int idx = Random.Range(0, enemyPool.Length);

        // Instanciar el prefab
        currentEnemy = Instantiate(enemyPool[idx], enemyContainer);

        currentEnemy.ResetHealth();
        ClikerEnemy.CurrentEnemy = currentEnemy;

        GameManager_Cliker.Instance.ShowEnemyUI(currentEnemy);
    }


    public void OnEnemyDefeated()
    {
        killedInRoom++;

        if (currentEnemy != null)
            Destroy(currentEnemy.gameObject);

        if (killedInRoom >= enemiesPerRoom)
        {
            killedInRoom = 0;
            IncreaseDifficulty();
        }

        SpawnEnemy();
    }


    void IncreaseDifficulty()
    {
        foreach (var e in enemyPool)
        {
            e.maxHealth *= 1.12f;
            e.baseDrop = Mathf.RoundToInt(e.baseDrop * 1.15f);
        }
    }
}
