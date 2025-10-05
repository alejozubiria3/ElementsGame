using UnityEngine;

[RequireComponent(typeof(Collider)), RequireComponent(typeof(Rigidbody))]
public class BasicProjectile : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField] float damage = 6f;  

    [Header("Colisión robusta")]
    [SerializeField] LayerMask hitMask = ~0;     
    [SerializeField] float sweepRadius = 0.16f;  

    [Header("Visual (opcional)")]
    [SerializeField] Renderer colorRenderer;

    float _damage;
    GameObject _instigator;
    float _speed;
    float _lifetime;

    Rigidbody _rb;
    Collider _col;
    int _detectionLayer;
    Vector3 _lastPos;

    void Awake()
    {
        _rb  = GetComponent<Rigidbody>();
        _col = GetComponent<Collider>();
        if (!colorRenderer) colorRenderer = GetComponentInChildren<Renderer>();

        
        _rb.useGravity = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

       
        _col.isTrigger = true;

        _detectionLayer = LayerMask.NameToLayer("Detection");
    }

    public void Launch(Vector3 dir, float speed, float lifetime, float dmg, GameObject instigator)
    {
        _instigator = instigator;
        _speed      = speed;
        _lifetime   = lifetime;
        _damage     = dmg > 0f ? dmg : damage;

       
        var instCols = _instigator.GetComponentsInChildren<Collider>(true);
        foreach (var c in instCols) Physics.IgnoreCollision(_col, c, true);

        _rb.linearVelocity = dir.normalized * _speed;
        _lastPos = transform.position;

        Destroy(gameObject, _lifetime);
    }

    public void SetColor(Color color)
    {
        if (colorRenderer) colorRenderer.material.color = color;
    }

    void FixedUpdate()
    {
        
        Vector3 cur   = transform.position;
        Vector3 delta = cur - _lastPos;
        float dist    = delta.magnitude;

        if (dist > 0f)
        {
            Vector3 dir = delta / dist;

            
            if (Physics.SphereCast(_lastPos, sweepRadius, dir, out var hit, dist, hitMask, QueryTriggerInteraction.Ignore))
            {
                HandleHit(hit.collider);
                _lastPos = cur;
                return;
            }

            
            if (Physics.Raycast(_lastPos, dir, out hit, dist, hitMask, QueryTriggerInteraction.Ignore))
            {
                HandleHit(hit.collider);
                _lastPos = cur;
                return;
            }
        }

        _lastPos = cur;
    }

    
    void OnTriggerEnter(Collider other) => HandleHit(other);

    void HandleHit(Collider other)
    {
        
        if (_instigator && other.transform.root == _instigator.transform) return;

        
        if (other.isTrigger) return;
        if (_detectionLayer != -1 && other.gameObject.layer == _detectionLayer) return;

        
        var eh = other.GetComponentInParent<EnemyHealth>();
        if (eh != null && !eh.isDead)
        {
            eh.TakeDamage(_damage, false);
            Destroy(gameObject);
            return;
        }

        
        if (((1 << other.gameObject.layer) & hitMask) != 0)
            Destroy(gameObject);
    }
}
