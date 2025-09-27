using UnityEngine;

[RequireComponent(typeof(Rigidbody))]
public class EnemyRanged : MonoBehaviour
{
    [Header("Movimiento")]
    [Tooltip("Velocidad al perseguir")]
    public float speed = 3f;

    [Tooltip("Distancia a la que el enemigo se DETIENE para disparar")]
    public float stopDistance = 8f;

    [Tooltip("Margen para evitar 'bombeo' adelante/atrás")]
    public float stopTolerance = 0.25f;

    [Header("Ataque a distancia")]
    [Tooltip("Debe ser <= stopDistance para disparar al frenarse")]
    public float attackRange = 8f;

    [Tooltip("Daño que aplicará la flecha al Player")]
    public int damage = 10;

    [Tooltip("Tiempo entre disparos")]
    public float attackCooldown = 1.2f;
    private float lastAttackTime;

    [Header("Proyectil")]
    [Tooltip("Prefab de la flecha (con script Flecha.cs)")]
    public Flecha flechaPrefab;

    [Tooltip("Punto desde donde sale la flecha")]
    public Transform shootPoint;

    [Tooltip("Velocidad inicial de la flecha")]
    public float projectileSpeed = 18f;

    [Header("Línea de visión (opcional)")]
    public bool requireLineOfSight = true;

    [Tooltip("Capas que bloquean la visión (paredes, etc.)")]
    public LayerMask losMask = ~0; 
    
    private Transform player;
    private bool isChasing;
    private Rigidbody rb;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = true;      
        rb.detectCollisions = true;

        
        if (attackRange > stopDistance)
            attackRange = stopDistance;
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
            if (!requireLineOfSight || HasLineOfSight())
            {
                lastAttackTime = Time.time;
                Shoot();
            }
        }
    }

    private bool HasLineOfSight()
    {
        Vector3 origin = shootPoint ? shootPoint.position : transform.position + Vector3.up * 1.2f;
        Vector3 target = player.position + Vector3.up * 1.0f;
        Vector3 dir = (target - origin).normalized;
        float dist = Vector3.Distance(origin, target);

       
        if (Physics.Raycast(origin, dir, out RaycastHit hit, dist, losMask, QueryTriggerInteraction.Ignore))
        {
            if (!hit.collider.CompareTag("Player"))
                return false;
        }
        return true;
    }

    private void Shoot()
    {
        if (!flechaPrefab) return;

        
        Vector3 origin = shootPoint ? shootPoint.position : transform.position + Vector3.up * 1.2f;
        Vector3 target = player.position + Vector3.up * 1.0f;
        Vector3 dir = (target - origin).normalized;

       
        Flecha flecha = Instantiate(flechaPrefab, origin, Quaternion.LookRotation(dir));
        flecha.Init(dir * projectileSpeed, damage);
    }

    
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            player = other.transform;
            isChasing = true;
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            isChasing = false;
            player = null;
        }
    }

    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan; Gizmos.DrawWireSphere(transform.position, stopDistance);
        Gizmos.color = Color.red;  Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
