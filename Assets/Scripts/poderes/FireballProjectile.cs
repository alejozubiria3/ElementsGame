using UnityEngine;

[RequireComponent(typeof(Collider))]
[RequireComponent(typeof(Rigidbody))]
public class FireballProjectile : MonoBehaviour
{
    [SerializeField] private Color fireColor = new Color(1f, 0.3f, 0.1f);

    private Rigidbody _rb;
    private Collider _myCol;
    private GameObject _instigator;

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
    }

    public void Launch(Vector3 dir, float speed, float lifetime, GameObject instigator)
    {
        _instigator = instigator;

        
        foreach (var c in _instigator.GetComponentsInChildren<Collider>())
            Physics.IgnoreCollision(_myCol, c, true);

        _rb.linearVelocity = dir * speed;
        Destroy(gameObject, lifetime);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.transform.root == _instigator.transform) return; // por si acaso
        Destroy(gameObject);
    }
}