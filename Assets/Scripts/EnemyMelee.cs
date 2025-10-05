using UnityEngine;

public class EnemyMelee : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 3f;
    public float stopDistance = 3f;
    public float stopTolerance = 0.15f;

    [Header("Ataque")]
    [Tooltip("Debe ser <= stopDistance para pegar al frenarse")]
    public float attackRange = 3f;
    public int damage = 10;
    public float attackCooldown = 1f;
    private float lastAttackTime;

    [Header("Vida Enemigo")]
    public int maxHealth = 30;
    private int currentHealth;

   
    private Transform player;
    private bool isChasing = false;
    private Rigidbody rb;

   
    private Collider[] myBodyCols;    
    private Collider[] myTriggerCols; 

    void Awake()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();
        if (rb) { rb.isKinematic = true; rb.detectCollisions = true; }

        
        var all = GetComponentsInChildren<Collider>(true);
        int bodyCount = 0, trigCount = 0;
        foreach (var c in all)
        {
            if (!c) continue;
            if (c.isTrigger) trigCount++; else bodyCount++;
        }
        myBodyCols   = new Collider[bodyCount];
        myTriggerCols= new Collider[trigCount];

        int bi = 0, ti = 0;
        foreach (var c in all)
        {
            if (!c) continue;
            if (c.isTrigger) myTriggerCols[ti++] = c;
            else             myBodyCols[bi++]    = c;
        }
    }

    void Update()
    {
        if (!isChasing || !player) return;

        Vector3 toPlayer = player.position - transform.position;
        toPlayer.y = 0f;
        float dist = toPlayer.magnitude;

       
        if (dist > 0.001f)
        {
            Quaternion look = Quaternion.LookRotation(toPlayer);
            transform.rotation = Quaternion.Slerp(transform.rotation, look, 12f * Time.deltaTime);
        }

        
        if (dist > stopDistance + stopTolerance)
        {
            Vector3 dir = toPlayer.normalized;
            transform.position += dir * speed * Time.deltaTime;
        }

        
        if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            lastAttackTime = Time.time;
            var ph = player.GetComponent<PlayerSimpleHealth>() ?? player.GetComponentInParent<PlayerSimpleHealth>();
            if (ph != null && !ph.isDead)
                ph.TakeDamage(damage);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            isChasing = true;

            
            ToggleIgnoreBodyCollisionWithPlayer(other, true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            
            ToggleIgnoreBodyCollisionWithPlayer(other, false);

            isChasing = false;
            player = null;
        }
    }

    
    void ToggleIgnoreBodyCollisionWithPlayer(Collider playerAnyCol, bool ignore)
    {
        if (myBodyCols == null || myBodyCols.Length == 0) return;

        
        var playerCols = playerAnyCol.GetComponentsInParent<Collider>(true);
        foreach (var ec in myBodyCols)
        {
            if (!ec) continue;
            foreach (var pc in playerCols)
            {
                if (!pc || pc.isTrigger) continue; 
                Physics.IgnoreCollision(ec, pc, ignore);
            }
        }
    }

    public void TakeDamage(int amount)
    {
        currentHealth -= amount;
        if (currentHealth <= 0) Die();
    }

    private void Die()
    {
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.red;  Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
