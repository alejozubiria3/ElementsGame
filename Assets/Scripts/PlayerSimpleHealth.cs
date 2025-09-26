using UnityEngine;
using System;

public class PlayerSimpleHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;
    [Tooltip("Solo lectura en runtime")]
    public int currentHealth;

    [Header("Estado")]
    public bool isInvulnerable = false;
    public bool isDead { get; private set; }

    public event Action<int, int> OnHealthChanged; 

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
        Debug.Log("[PlayerSimpleHealth] Awake -> HP: " + currentHealth);
        OnHealthChanged?.Invoke(currentHealth, maxHealth); 
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        if (isInvulnerable)
        {
            Debug.Log("[PlayerSimpleHealth] Daño bloqueado por WaterShield.");
            return;
        }

        currentHealth = Mathf.Max(0, currentHealth - amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);

        Debug.Log("[PlayerSimpleHealth] TakeDamage " + amount + " -> HP: " + currentHealth);

        if (currentHealth == 0)
            Die();
    }

    public void Heal(int amount)
    {
        if (isDead) return;
        currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Debug.Log("[PlayerSimpleHealth] DIE() llamado. Destruyendo Player...");

        var cc = GetComponent<CharacterController>();
        if (cc) cc.enabled = false;
        var rb = GetComponent<Rigidbody>();
        if (rb) { rb.linearVelocity = Vector3.zero; rb.isKinematic = true; }

        Destroy(gameObject, 0.05f);
    }

    [ContextMenu("TEST: Kill Player")]
    void TestKill()
    {
        currentHealth = 0;
        OnHealthChanged?.Invoke(currentHealth, maxHealth);
        Die();
    }
}  