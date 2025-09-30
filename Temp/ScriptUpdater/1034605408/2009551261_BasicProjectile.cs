using UnityEngine;

public class BasicProjectile : MonoBehaviour
{
    private float _damage;
    private GameObject _instigator;
    private float _speed;
    private float _lifetime;
    private Rigidbody _rb;
    private Renderer _renderer;

    void Awake()
    {
        _rb = GetComponent<Rigidbody>();
        _renderer = GetComponent<Renderer>();

        if (_rb)
        {
            _rb.useGravity = false;
            _rb.collisionDetectionMode = CollisionDetectionMode.Continuous;
        }
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

    public void SetColor(Color color)
    {
        if (_renderer != null)
            _renderer.material.color = color;
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.attachedRigidbody && other.attachedRigidbody.gameObject == _instigator) return;

        if (other.TryGetComponent<EnemyHealth>(out var eh))
            eh.TakeDamage(_damage);

        Destroy(gameObject);
    }
}   