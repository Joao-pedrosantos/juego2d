using UnityEngine;
using UnityEngine.UI;

public class EnemyHealthBar : MonoBehaviour
{
    public Damageable enemyDamageable;  // Reference to the boss's Damageable component
    public Image healthBarFill;         // The fill image of the health bar

    private void Start()
    {
        // Initialize the health bar with the enemy's starting health
        UpdateHealthBar();
    }

    private void OnEnable()
    {
        // Subscribe to the enemy's health change event
        if (enemyDamageable != null)
        {
            enemyDamageable.damageableHit.AddListener(OnEnemyHit);
        }
    }

    private void OnDisable()
    {
        // Unsubscribe from the health change event
        if (enemyDamageable != null)
        {
            enemyDamageable.damageableHit.RemoveListener(OnEnemyHit);
        }
    }

    // Called when the enemy takes damage
    private void OnEnemyHit(int damage, Vector2 knockback)
    {
        UpdateHealthBar();
    }

    // Update the health bar fill based on current health
    private void UpdateHealthBar()
    {
        if (enemyDamageable != null && healthBarFill != null)
        {
            float healthPercentage = (float)enemyDamageable.Health / enemyDamageable.MaxHealth;
            healthBarFill.fillAmount = healthPercentage;
        }
    }
}
