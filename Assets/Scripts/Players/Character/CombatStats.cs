using UnityEngine;

public class CombatStats : MonoBehaviour
{
    [Header("Configuration")]
    [Tooltip("Stamina cost per second while moving")]
    public float moveStaminaCost = 10f;

    [Header("Base Stats (Max)")]
    public int maxHealth = 100;
    public float maxStamina = 100f;
    public int attackDamage = 20;
    public int armor = 5;

    [Header("Runtime Values (Read Only)")]
    public int currentHealth;
    public float currentStamina;

    private void Awake()
    {
        ResetTurnStats();
        currentHealth = maxHealth;
    }

    public void ResetTurnStats()
    {
        currentStamina = maxStamina;
        Debug.Log($"🔄 Turn Start: Stamina Refilled to {maxStamina}");
    }

    public bool HasStamina()
    {
        return currentStamina > 0;
    }

    public void UseStamina(float amount)
    {
        currentStamina -= amount;
        if (currentStamina < 0) currentStamina = 0;

        // 🟢 ADDED LOG HERE
        // ":F1" formats the number to 1 decimal place (e.g., "85.4") to avoid messy numbers
        Debug.Log($"⚡ Stamina: {currentStamina:F1} / {maxStamina}");
    }

    public void TakeDamage(int dmg)
    {
        int finalDamage = Mathf.Max(1, dmg - armor);
        currentHealth -= finalDamage;
        Debug.Log($"{gameObject.name} took {finalDamage} dmg. HP: {currentHealth}");
    }
}