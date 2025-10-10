using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyRanged : MonoBehaviour
{
    [Header("Movimiento")]
    public float speed = 3f;
    public float stopDistance = 8f;
    public float stopTolerance = 0.25f;

    [Header("Ataque")]
    public float attackRange = 8f; 
    public int damage = 10;
    public float attackCooldown = 1.2f;
    float lastAttackTime;

    [Header("Proyectil")]
    public Flecha flechaPrefab;
    public Transform shootPoint;
    public float projectileSpeed = 18f;

    [Header("Detección")]
    public string playerTag = "Player";
    public float detectionRadius = 14f;   
    public bool requireLineOfSight = true;
    public LayerMask losMask = ~0;

    [Header("Vida / Agro")]
    public int maxHealth = 30;
    int currentHealth;
    [Tooltip("<= 0 = infinito")]
    public float aggroHoldSeconds = 8f;

    [Header("Re-adquisición / Fallback")]
    public float reacquireInterval = 0.4f;
    public float proximityScanRadius = 60f;
    public LayerMask playerLayer = ~0;

    [Header("Colisión con Player")]
    [Tooltip("Opcional: asigná a mano tus colliders de CUERPO (no-trigger). Si lo dejás vacío, se detectan solos.")]
    public Collider[] bodyCollidersManual;

    [Header("Debug")]
    public bool debugLogs = false;

    // Runtime
    Transform player;
    bool isChasing;
    bool forcedAggro;
    float aggroUntilTime;
    float _nextReacquireTime;
    Rigidbody rb;
    Collider[] myBodyCols; 

    void Awake()
    {
        currentHealth = maxHealth;

        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;          
        rb.detectCollisions = true;

        if (attackRange > stopDistance) attackRange = stopDistance;

        if (bodyCollidersManual != null && bodyCollidersManual.Length > 0)
        {
            myBodyCols = bodyCollidersManual;
        }
        else
        {
            var all = GetComponentsInChildren<Collider>(true);
            int bodyCount = 0;
            foreach (var c in all) if (c && !c.isTrigger) bodyCount++;
            myBodyCols = new Collider[bodyCount];
            int i = 0;
            foreach (var c in all) if (c && !c.isTrigger) myBodyCols[i++] = c;
        }

        if (debugLogs) Debug.Log($"[EnemyRanged] Awake: isKin={rb.isKinematic}, bodyCols={myBodyCols?.Length}");

        var pgo = GameObject.FindGameObjectWithTag(playerTag);
        if (pgo) player = pgo.transform;
    }

    void Update()
    {
        if (!isChasing) AcquireByRadius();

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
            transform.position += toPlayer.normalized * speed * Time.deltaTime;
        }

        if (dist <= attackRange && Time.time >= lastAttackTime + attackCooldown)
        {
            if (!requireLineOfSight || HasLineOfSight())
            {
                lastAttackTime = Time.time;
                Shoot();
            }
        }
    }

    void AcquireByRadius()
    {
        if (!player)
        {
            var pgo = GameObject.FindGameObjectWithTag(playerTag);
            if (pgo) player = pgo.transform;
        }
        if (!player) return;

        Vector3 flat = player.position - transform.position; flat.y = 0f;
        if (flat.sqrMagnitude <= detectionRadius * detectionRadius)
        {
            if (!requireLineOfSight || HasLineOfSight())
                StartChaseWithIgnore(player);
        }
    }

    bool HasLineOfSight()
    {
        if (!player) return false;
        Vector3 origin = shootPoint ? shootPoint.position : transform.position + Vector3.up * 1.2f;
        Vector3 target = player.position + Vector3.up * 1.0f;
        Vector3 dir = target - origin;
        float dist = dir.magnitude;

        if (Physics.Raycast(origin, dir.normalized, out RaycastHit hit, dist, losMask, QueryTriggerInteraction.Ignore))
            return hit.collider.CompareTag(playerTag);
        return true;
    }
void Shoot()
{
    if (!flechaPrefab || !player) return;

    Vector3 origin = shootPoint ? shootPoint.position : transform.position + Vector3.up * 1.2f;
    Vector3 target = player.position + Vector3.up * 1.0f;
    Vector3 dir    = (target - origin).normalized;

    const float spawnOffset = 0.35f;
    Vector3 spawnPos = origin + dir * spawnOffset;

    var flecha = Instantiate(flechaPrefab, spawnPos, Quaternion.LookRotation(dir));
    flecha.Init(dir * projectileSpeed, damage, this.transform);
}
    public void StartChase(Collider playerCol)
    {
        StartChaseWithIgnore(playerCol ? playerCol.transform : null);
    }

    public void StopChase(Collider playerCol)
    {
        EnsureIgnoreCollisionWithPlayer(false);
        if (!forcedAggro)
        {
            isChasing = false;
            player = null;
        }
    }

    void StartChaseWithIgnore(Transform p)
    {
        if (!p) return;
        player = p;
        isChasing = true;
        EnsureIgnoreCollisionWithPlayer(true);
        if (debugLogs) Debug.Log("[EnemyRanged] StartChase");
    }

    void EnsureIgnoreCollisionWithPlayer(bool ignore)
    {
        if (!player || myBodyCols == null || myBodyCols.Length == 0) return;

        var playerCols = player.GetComponentsInChildren<Collider>(true);
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
        if (currentHealth <= 0) Die();
    }

    public void TakeDamageFrom(int amount, Transform attacker)
    {
        currentHealth -= amount;
        ForceAggro(attacker);
        if (currentHealth <= 0) Die();
    }

    public void ForceAggro(Transform attackerOrNull)
    {
        if (attackerOrNull) player = attackerOrNull;
        if (!player)
        {
            var pgo = GameObject.FindGameObjectWithTag(playerTag);
            if (pgo) player = pgo.transform;
        }

        isChasing = true;
        forcedAggro = true;
        aggroUntilTime = (aggroHoldSeconds > 0f) ? Time.time + aggroHoldSeconds : Mathf.Infinity;
        EnsureIgnoreCollisionWithPlayer(true);
    }

    void TryResolvePlayerReference(bool aggressive = false)
    {
        if (!player)
        {
            var pGO = GameObject.FindGameObjectWithTag(playerTag);
            if (pGO) player = pGO.transform;
        }
        if (!player && aggressive)
        {
            var hits = Physics.OverlapSphere(transform.position, proximityScanRadius, playerLayer, QueryTriggerInteraction.Ignore);
            foreach (var h in hits)
            {
                if (h.CompareTag(playerTag) || h.GetComponentInParent<PlayerSimpleHealth>() != null)
                {
                    player = h.transform;
                    isChasing = true;
                    EnsureIgnoreCollisionWithPlayer(true);
                    break;
                }
            }
        }
    }

    void Die()
    {
        if (debugLogs) Debug.Log("[EnemyRanged] Die()");
        Destroy(gameObject);
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.red;  Gizmos.DrawWireSphere(transform.position, attackRange);
        Gizmos.color = Color.yellow; Gizmos.DrawWireSphere(transform.position, detectionRadius);
    }
}
