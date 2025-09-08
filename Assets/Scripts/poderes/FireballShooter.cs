using UnityEngine;

[RequireComponent(typeof(ElementSwitcher))]
public class FireballShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject fireballPrefab;

    [Header("Shoot Params")]
    [SerializeField] private float speed = 24f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float cooldown = 0.35f;

    [Header("Aiming")]
    [SerializeField] private LayerMask groundMask = ~0; 
    [SerializeField] private float aimPlaneY = 0f;     
    [SerializeField] private bool lockYDirection = true;

    private ElementSwitcher _element;
    private float _cd;
    private Camera _cam;

    void Awake()
    {
        _element = GetComponent<ElementSwitcher>();
        _cam = Camera.main;
        if (_cam == null) Debug.LogWarning("No hay MainCamera con tag 'MainCamera'.");
    }

    void Update()
    {
        if (_cd > 0f) _cd -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && _cd <= 0f && _element.current == Element.Fire)
        {
            if (TryGetAimPoint(out var target))
            {
                ShootTowards(target);
                _cd = cooldown;
            }
        }
    }

    bool TryGetAimPoint(out Vector3 point)
    {
        point = Vector3.zero;
        if (_cam == null) return false;

        Ray ray = _cam.ScreenPointToRay(Input.mousePosition);

        
        if (Physics.Raycast(ray, out var hit, 500f, groundMask))
        {
            point = hit.point;
            return true;
        }

        
        Plane plane = new Plane(Vector3.up, new Vector3(0f, aimPlaneY, 0f));
        if (plane.Raycast(ray, out float enter))
        {
            point = ray.GetPoint(enter);
            return true;
        }

        return false;
    }

    void ShootTowards(Vector3 target)
    {
        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 dir = (target - origin).normalized;
        if (lockYDirection) { dir.y = 0f; dir.Normalize(); }

        Vector3 spawnPos = origin + dir * 0.8f;

        var go = Instantiate(fireballPrefab, spawnPos, Quaternion.LookRotation(dir));
        if (go.TryGetComponent<FireballProjectile>(out var proj))
        {
            proj.Launch(dir, speed, lifetime, this.gameObject);
        }
    }
}