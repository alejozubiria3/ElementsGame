using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    private float _damage;
    private GameObject _instigator;
    private float _speed;
    private float _lifetime;
    private Rigidbody _rb;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        if (_rb) { _rb.useGravity = false; _rb.collisionDetectionMode = CollisionDetectionMode.Continuous; }
    }

    public void Launch(Vector3 dir, float speed, float lifetime, float damage, GameObject instigator)
    {
        _damage = damage;
        _instigator = instigator;
        _speed = speed;
        _lifetime = lifetime;

        if (_rb) _rb.linearVelocity = dir * _speed;
        Destroy(gameObject, _lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody && other.attachedRigidbody.gameObject == _instigator) return;

        if (other.TryGetComponent<EnemyHealth>(out var eh))
            eh.TakeDamage(_damage);

        Destroy(gameObject);
    }
}   