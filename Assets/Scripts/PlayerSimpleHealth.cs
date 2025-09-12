using UnityEngine;

public class PlayerSimpleHealth : MonoBehaviour
{
    [Header("Vida")]
    public int maxHealth = 100;
    [Tooltip("Solo lectura en runtime")]
    public int currentHealth;

    public bool isDead { get; private set; }

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;
        Debug.Log("[PlayerSimpleHealth] Awake -> HP: " + currentHealth);
    }

    public void TakeDamage(int amount)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0, currentHealth - amount);
        Debug.Log("[PlayerSimpleHealth] TakeDamage " + amount + " -> HP: " + currentHealth);

        if (currentHealth == 0)
            Die();
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
        Die();
    }
}
