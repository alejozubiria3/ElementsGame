using UnityEngine;

public class Flecha : MonoBehaviour
{
    [Header("Vida útil")]
    public float lifetime = 6f;

    [Header("Máscara de impacto (solo para modo no-homing o si ignoreObstacles=false)")]
    public LayerMask hitMask = ~0;

    // Runtime
    Rigidbody _rb;
    Collider  _col;

    Transform _shooter;
    Transform _target;          
    int _damage;
    bool _launched;

    // homing
    bool  _homing;
    bool  _ignoreObstacles;
    float _speed;
    float _rotateSpeedRad;      
    float _hitRadius;           
    float _targetHeightOffset = 1.0f;

    void Awake()
    {
        _rb  = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        if (_rb) _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
    }

    public void Init(Vector3 velocity, int damage, Transform shooter = null)
    {
        _shooter = shooter;
        _damage  = damage;
        _homing  = false;

        SetupIgnoreShooter();

        if (_rb)
        {
            _rb.isKinematic  = false;
            _rb.useGravity   = false;
            _rb.linearVelocity     = velocity;
        }

        _launched = true;
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    public void InitHoming(Transform target, float speed, int damage, Transform shooter = null,
                           float rotateSpeedDegPerSec = 1080f, float hitRadius = 0.35f,
                           bool ignoreObstacles = true, float targetHeightOffset = 1.0f)
    {
        _target             = target;
        _speed              = speed;
        _damage             = damage;
        _shooter            = shooter;
        _homing             = true;
        _rotateSpeedRad     = rotateSpeedDegPerSec * Mathf.Deg2Rad;
        _hitRadius          = hitRadius;
        _ignoreObstacles    = ignoreObstacles;
        _targetHeightOffset = targetHeightOffset;

        SetupIgnoreShooter();

        if (_rb)
        {
            _rb.isKinematic = false;
            _rb.useGravity  = false;
            _rb.linearVelocity    = transform.forward * _speed;
        }

        if (_col && _ignoreObstacles) _col.isTrigger = true;

        _launched = true;
        if (lifetime > 0f) Destroy(gameObject, lifetime);
    }

    void SetupIgnoreShooter()
    {
        if (_col && _shooter)
        {
            var shooterCols = _shooter.GetComponentsInChildren<Collider>(true);
            foreach (var sc in shooterCols)
                if (sc) Physics.IgnoreCollision(_col, sc, true);
        }
    }

    void FixedUpdate()
    {
        if (!_launched) return;

        if (_homing)
        {
            if (_target == null) { Destroy(gameObject); return; }

            Vector3 aimPos = _target.position + Vector3.up * _targetHeightOffset;
            Vector3 desired = (aimPos - transform.position);
            float  dist = desired.magnitude;
            if (dist <= _hitRadius)
            {
                HitPlayer(_target);
                return;
            }

            desired.Normalize();

            Vector3 newDir = Vector3.RotateTowards(transform.forward, desired,
                               _rotateSpeedRad * Time.fixedDeltaTime, 999f);
            transform.rotation = Quaternion.LookRotation(newDir);

            if (_rb) _rb.linearVelocity = newDir * _speed;
            else     transform.position += newDir * _speed * Time.fixedDeltaTime;
        }
    }

    void OnTriggerEnter(Collider other)  { TryHit(other); }
    void OnCollisionEnter(Collision c)   { if (c != null && c.collider) TryHit(c.collider); }

    void TryHit(Collider other)
    {
        if (!_launched || !other) return;

        if (_shooter && other.transform.IsChildOf(_shooter)) return;

        
        if (_homing && _ignoreObstacles)
        {
            if (_target && other.transform.IsChildOf(_target))
                { HitPlayer(_target); }
            return; 
        }

        if (hitMask != ~0)
        {
            int m = 1 << other.gameObject.layer;
            if ((hitMask.value & m) == 0) return;
        }

        
        var ph = other.GetComponentInParent<PlayerSimpleHealth>();
        if (ph) { ph.TakeDamage(_damage); Destroy(gameObject); return; }

        if (!other.isTrigger) Destroy(gameObject);
    }

    void HitPlayer(Transform t)
    {
        var ph = t ? t.GetComponentInParent<PlayerSimpleHealth>() : null;
        if (ph) ph.TakeDamage(_damage);
        Destroy(gameObject);
    }
}
