using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class FireballProjectile : MonoBehaviour
{
    [Header("Daño")]
    [SerializeField] private float damage = 20f;

    [Header("Visual/Debug")]
    [SerializeField] private Color fireColor = new Color(1f, 0.3f, 0.1f);

    private Rigidbody _rb;
    private Collider _myCol;
    private GameObject _instigator;

    
    private int _detectionLayer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _myCol = GetComponent<Collider>();

        _rb.useGravity = false;
        _rb.interpolation = RigidbodyInterpolation.Interpolate;
        _rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

        _myCol.isTrigger = true;

        var rend = GetComponentInChildren<Renderer>();
        if (rend) rend.material.color = fireColor;

        
        _detectionLayer = LayerMask.NameToLayer("Detection");
    }

    /// <summary>Lanza la fireball</summary>
    public void Launch(Vector3 dir, float speed, float lifetime, GameObject instigator)
    {
        _instigator = instigator;

        
        var cols = _instigator.GetComponentsInChildren<Collider>(true);
        foreach (var c in cols)
            Physics.IgnoreCollision(_myCol, c, true);

        
        _rb.linearVelocity = dir.normalized * speed;

        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        
        if (_instigator && other.transform.root == _instigator.transform) return;

        
        if (other.isTrigger) return;

        
        if (_detectionLayer != -1 && other.gameObject.layer == _detectionLayer) return;

        
        var enemy = other.GetComponentInParent<EnemyHealth>();
        if (enemy != null && !enemy.isDead)
        {
            enemy.TakeDamage(damage, false);

            
            var burn = enemy.GetComponent<BurnEffect>();
            if (burn == null) burn = enemy.gameObject.AddComponent<BurnEffect>();
            burn.tickDamage = 5f;
            burn.tickInterval = 1f;
            burn.duration = 3f;
            burn.StartBurn();

            Destroy(gameObject);
            return;
        }

        
        Destroy(gameObject);
    }
}
