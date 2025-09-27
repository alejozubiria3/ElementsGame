using UnityEngine;

[RequireComponent(typeof(Rigidbody), typeof(Collider))]
public class Flecha : MonoBehaviour
{
    [Header("Ajustes")]
    public float lifeTime = 6f;       // Tiempo antes de autodestruirse si no pega
    public bool destroyOnAnyHit = true;

    private Rigidbody rb;
    private int damage;

    void Awake()
    {
        rb = GetComponent<Rigidbody>();
        rb.isKinematic = false;                // para que se mueva con físicas
        rb.useGravity = false;                 // que no caiga
        rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        rb.interpolation = RigidbodyInterpolation.Interpolate;

        // Asegurarnos que el collider NO sea trigger
        Collider col = GetComponent<Collider>();
        col.isTrigger = false;
    }

    /// <summary>
    /// Inicializa la flecha con velocidad y daño.
    /// </summary>
    public void Init(Vector3 initialVelocity, int damageAmount)
    {
        damage = damageAmount;
        rb.linearVelocity = initialVelocity;
        Destroy(gameObject, lifeTime);
    }

    void OnCollisionEnter(Collision collision)
    {
        // Si choca con el Player → daño
        if (collision.collider.CompareTag("Player"))
        {
            var ph = collision.collider.GetComponent<PlayerSimpleHealth>() 
                     ?? collision.collider.GetComponentInParent<PlayerSimpleHealth>();
            if (ph != null && !ph.isDead)
            {
                ph.TakeDamage(damage);
            }
        }

        if (destroyOnAnyHit)
        {
            Destroy(gameObject);
        }
    }
}
