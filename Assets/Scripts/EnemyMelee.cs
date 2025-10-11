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

    [Header("Agro al recibir daño")]
    [Tooltip("<=0 = infinito")]
    public float aggroHoldSeconds = 8f;

    [Header("Resolución de Player")]
    [SerializeField] private Transform player;           
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float reacquireInterval = 0.4f; 
    [SerializeField] private float proximityScanRadius = 60f; 
    [SerializeField] private LayerMask playerLayer = ~0;      

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private bool isChasing = false;
    private bool forcedAggro = false;
    private float aggroUntilTime = 0f;

    private float _nextReacquireTime = 0f;
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
        foreach (var c in all) { if (!c) continue; if (c.isTrigger) trigCount++; else bodyCount++; }
        myBodyCols    = new Collider[bodyCount];
        myTriggerCols = new Collider[trigCount];
        int bi = 0, ti = 0;
        foreach (var c in all)
        {
            if (!c) continue;
            if (c.isTrigger) myTriggerCols[ti++] = c;
            else             myBodyCols[bi++]    = c;
        }

        if (!player) TryResolvePlayerReference();
    }

    void Update()
    {
        
        if (forcedAggro && aggroHoldSeconds > 0f && Time.time >= aggroUntilTime)
            forcedAggro = false;

        
        if ((isChasing || forcedAggro) && !player && Time.time >= _nextReacquireTime)
        {
            _nextReacquireTime = Time.time + reacquireInterval;
            TryResolvePlayerReference(true); 
        }

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
            if (ph != null && !ph.isDead) ph.TakeDamage(damage);
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            player = other.transform;
            isChasing = true;
            ToggleIgnoreBodyCollisionWithPlayer(other, true);
            if (debugLogs) Debug.Log("[EnemyMelee] OnTriggerEnter: player=" + player.name);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            ToggleIgnoreBodyCollisionWithPlayer(other, false);
            if (!forcedAggro)
            {
                isChasing = false;
                player = null;
                if (debugLogs) Debug.Log("[EnemyMelee] OnTriggerExit: suelto (sin agro).");
            }
            else if (debugLogs) Debug.Log("[EnemyMelee] OnTriggerExit: mantengo persecución (agro).");
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
        ForceAggro(null);  

        if (debugLogs) Debug.Log($"[EnemyMelee] TakeDamage({amount}) → {currentHealth}/{maxHealth}, forcedAggro={forcedAggro}, player={(player?player.name:"NULL")}");
        if (currentHealth <= 0) Die();
    }

    public void TakeDamageFrom(int amount, Transform attacker)
    {
        currentHealth -= amount;
        ForceAggro(attacker);
        if (debugLogs) Debug.Log($"[EnemyMelee] TakeDamageFrom({amount}, attacker={attacker?.name}) → {currentHealth}/{maxHealth}");
        if (currentHealth <= 0) Die();
    }

    
    public void ForceAggro(Transform attackerOrNull)
    {
        if (attackerOrNull) player = attackerOrNull;
        if (!player) TryResolvePlayerReference(true); 

        isChasing = true;                 
        forcedAggro = true;
        aggroUntilTime = (aggroHoldSeconds > 0f) ? Time.time + aggroHoldSeconds : Mathf.Infinity;

        if (debugLogs) Debug.Log($"[EnemyMelee] ForceAggro → chasing={isChasing}, player={(player?player.name:"NULL")}, hold={aggroHoldSeconds}");
    }

    
    private void TryResolvePlayerReference(bool aggressive = false)
    {
        
        if (!player && !string.IsNullOrEmpty(playerTag))
        {
            var pGO = GameObject.FindGameObjectWithTag(playerTag);
            if (pGO) { player = pGO.transform; if (debugLogs) Debug.Log("[EnemyMelee] Player por tag: " + player.name); }
        }
        
        if (!player)
        {
            var ph = FindObjectOfType<PlayerSimpleHealth>();
            if (ph) { player = ph.transform; if (debugLogs) Debug.Log("[EnemyMelee] Player por PlayerSimpleHealth: " + player.name); }
        }
        
        if (!player && aggressive)
        {
            var hits = Physics.OverlapSphere(transform.position, proximityScanRadius, playerLayer);
            foreach (var h in hits)
            {
                if (h.CompareTag(playerTag) || h.GetComponentInParent<PlayerSimpleHealth>() != null)
                {
                    player = h.transform;
                    if (debugLogs) Debug.Log("[EnemyMelee] Player por scan de proximidad: " + player.name);
                    break;
                }
            }
        }
    }

    private void Die()
    {
        if (debugLogs) Debug.Log("[EnemyMelee] Die()");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.red;  Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
