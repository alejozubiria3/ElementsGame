using UnityEngine;

public class Flecha : MonoBehaviour
{
    [Header("Setup")]
    public float lifetime = 6f;
    public LayerMask hitMask = ~0;

    Rigidbody _rb;
    Collider _myCol;
    int _damage;
    Transform _shooter;
    bool _launched;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _myCol = GetComponent<Collider>();
        if (_rb) _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public void Init(Vector3 velocity, int damage, Transform shooter = null)
    {
        _damage  = damage;
        _shooter = shooter;

        // ⬇️ Ignorar colisión con TODOS los colliders del tirador
        if (_myCol && _shooter)
        {
            var shooterCols = _shooter.GetComponentsInChildren<Collider>(true);
            foreach (var sc in shooterCols)
            {
                if (!sc) continue;
                Physics.IgnoreCollision(_myCol, sc, true);
            }
        }

        if (_rb)
        {
            _rb.isKinematic = false;
            _rb.linearVelocity = velocity;
        }

        _launched = true;
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)  { TryHit(other); }
    void OnCollisionEnter(Collision c)   { if (c != null && c.collider) TryHit(c.collider); }

    void TryHit(Collider other)
    {
        if (!_launched || !other) return;

        // no pegarle al que disparó ni a sus hijos
        if (_shooter && other.transform.IsChildOf(_shooter)) return;

        // filtrar por máscara si la usás
        if (hitMask != ~0)
        {
            int m = 1 << other.gameObject.layer;
            if ((hitMask.value & m) == 0) return;
        }

        // Player
        var ph = other.GetComponentInParent<PlayerSimpleHealth>();
        if (ph) { ph.TakeDamage(_damage); Destroy(gameObject); return; }

        // Enemigos (por si reutilizás la flecha para el jugador)
        var em = other.GetComponentInParent<EnemyMelee>();
        if (em) { em.TakeDamageFrom(_damage, _shooter ? _shooter : transform); Destroy(gameObject); return; }

        var er = other.GetComponentInParent<EnemyRanged>();
        if (er) { er.TakeDamageFrom(_damage, _shooter ? _shooter : transform); Destroy(gameObject); return; }

        // cualquier otra cosa sólida
        if (!other.isTrigger) Destroy(gameObject);
    }
}
