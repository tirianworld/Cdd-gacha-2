using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ClikerEnemy : MonoBehaviour
{
    public static ClikerEnemy CurrentEnemy;

    public string enemyName = "Mutant";
    public float maxHealth = 50f;
    public float health;
    public long baseDrop = 5;

    [Header("UI")]
    public Sprite enemySprite;

    [Header("XP Orb Drop")]
    public Item_SO xpOrb;
    public bool dropXPOrb = true;
    public float orbDropChance = 0.25f;
    public int orbMin = 1;
    public int orbMax = 5;
    [Header("Item Drop")]
    public Item_SO dropItem;     // El item que dropea
    public int dropMin = 1;
    public int dropMax = 3;
    public float dropChance = 1f; // 100% por defecto

    void Awake()
    {
        ResetHealth();
    }

    public void ResetHealth()
    {
        health = maxHealth;
    }

    public bool TakeDamage(float amount)
    {
        health -= amount;
        if (health <= 0)
        {
            health = 0;
            Die();
            Destroy(gameObject);
        }
        return false;
    } 
    public void TakeDamageButton()
    {
        TakeDamage(10);
    }
    void Die()
    {
        TryDropItem();
        TryDropXPOrb();   // ⭐ AÑADIR ESTO

        EnemySpawnerCliker.Instance.OnEnemyDefeated();
    }

    public void TryDropItem()
    {
        if (dropItem == null) return;

        if (Random.value <= dropChance)
        {
            int amount = Random.Range(dropMin, dropMax + 1);

            // ⭐ Buscar el GameManager en la escena
            GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();

            gm.playerInventory.AddItem(dropItem, amount);
            gm.SaveInventory();

            Debug.Log($"[CLICKER] Drop: {dropItem.itemName} x{amount}");
        }
    }

    public void TryDropXPOrb()
    {
        if (!dropXPOrb || xpOrb == null)
            return;

        if (Random.value <= orbDropChance)
        {
            int amount = Random.Range(orbMin, orbMax + 1);

            // ⭐ Buscar el GameManager real
            GameManager gm = GameObject.Find("GameManager").GetComponent<GameManager>();

            gm.playerInventory.AddItem(xpOrb, amount);
            gm.SaveInventory();

            Debug.Log($"[CLICKER] Drop XP Orb x{amount}");
        }
    }


}
