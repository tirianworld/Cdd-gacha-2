using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using TMPro;

public class GameManager_Cliker : MonoBehaviour
{
    public static GameManager_Cliker Instance;

    public long sparks = 0;
    public TextMeshProUGUI sparkText;

    public float clickDamage = 1f;
    public float autoDPS = 0f;
    public TextMeshProUGUI autoLevelText;

    public Slider enemyHealthSlider;
    public TextMeshProUGUI enemyNameText;
    public Image enemyImage;

    public float relicDamageMultiplier = 1f;
    public float relicResourceMultiplier = 1f;

    void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    void Start()
    {
        UpdateSparkUI();
        StartCoroutine(AutoDPSLoop());
    }

    public void AddSparks(long amount)
    {
        sparks += amount;
        UpdateSparkUI();
    }

    public bool SpendSparks(long amount)
    {
        if (sparks >= amount)
        {
            sparks -= amount;
            UpdateSparkUI();
            return true;
        }
        return false;
    }
    public void ApplySpeedBuff(float speedMul, float duration)
    {
        StopCoroutine("SpeedBuffCoroutine");
        StartCoroutine(SpeedBuffCoroutine(speedMul, duration));
    }

    IEnumerator SpeedBuffCoroutine(float speedMul, float duration)
    {
        float originalDPS = autoDPS;
        autoDPS *= speedMul;
        UpdateSparkUI();

        yield return new WaitForSeconds(duration);

        autoDPS = originalDPS;
        UpdateSparkUI();
    }

    public void UpdateSparkUI()
    {
        if (sparkText != null) sparkText.text = sparks.ToString();
        if (autoLevelText != null) autoLevelText.text = $"Auto DPS: {autoDPS:F1}";
    }

    IEnumerator AutoDPSLoop()
    {
        while (true)
        {
            yield return new WaitForSeconds(1f);
            ClikerEnemy current = ClikerEnemy.CurrentEnemy;
            if (current != null)
            {
                float dmg = autoDPS * relicDamageMultiplier;
                current.TakeDamage(dmg);
            }
        }
    }
    public void ShowEnemyUI(ClikerEnemy enemy)
    {
        if (enemyNameText != null)
            enemyNameText.text = enemy.enemyName;

        if (enemyHealthSlider != null)
        {
            enemyHealthSlider.maxValue = enemy.maxHealth;
            enemyHealthSlider.value = enemy.health;
        }

        if (enemyImage != null)
            enemyImage.sprite = enemy.enemySprite;
    }

    public void DoClick()
    {
        var enemy = EnemySpawnerCliker.Instance.currentEnemy;
        if (enemy == null) return;

        float dmg = clickDamage * relicDamageMultiplier;

        bool died = enemy.TakeDamage(dmg);

        enemyHealthSlider.value = enemy.health;

        if (died)
            HandleEnemyDeath(enemy);
    }
    public void HandleEnemyDeath(ClikerEnemy enemy)
    {
        enemy.TryDropItem();
        EnemySpawnerCliker.Instance.OnEnemyDefeated();
    }



    public void ApplyRelicBuff(float damageMul, float resourceMul, float duration)
    {
        StopCoroutine("RelicBuffCoroutine");
        StartCoroutine(RelicBuffCoroutine(damageMul, resourceMul, duration));
    }

    IEnumerator RelicBuffCoroutine(float dmgMul, float resMul, float duration)
    {
        relicDamageMultiplier = dmgMul;
        relicResourceMultiplier = resMul;
        yield return new WaitForSeconds(duration);
        relicDamageMultiplier = 1f;
        relicResourceMultiplier = 1f;
    }
    void TryDropXPOrb(ClikerEnemy enemy)
    {
        if (!enemy.dropXPOrb || enemy.xpOrb == null)
            return;

        if (Random.value <= enemy.orbDropChance)
        {
            int amount = Random.Range(enemy.orbMin, enemy.orbMax + 1);

            GameManager.Instance.playerInventory.AddItem(enemy.xpOrb, amount);
            GameManager.Instance.SaveInventory();

            Debug.Log($"Drop XP Orb x{amount}");
        }
    }

}
