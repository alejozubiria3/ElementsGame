using UnityEngine;

[RequireComponent(typeof(ElementSwitcher), typeof(TargetSelector))]
public class FireballShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private ManaSystem mana;
    [Tooltip("Punto de spawn (vacío en la mano/pecho). Si no se asigna, usa la posición del Player.")]
    [SerializeField] private Transform spawnPoint;
    [SerializeField] private AnimationManager anim;

    [Header("Costos")]
    [SerializeField] private float manaCost = 15f;

    [Header("Disparo")]
    [SerializeField] private float speed = 24f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float cooldown = 2f;
    [Tooltip("Si TRUE, aplana la dirección en Y (útil solo en juegos 2D/top-down). Déjalo FALSE para 3D real.")]
    [SerializeField] private bool lockYDirection = false;

    
    public float CooldownRemaining => Mathf.Max(0f, _cd);
    public float CooldownTotal => cooldown;
    public float Cooldown01 => (cooldown <= 0f) ? 0f : Mathf.Clamp01(_cd / cooldown);

    private ElementSwitcher _element;
    private TargetSelector _selector;
    private float _cd;

    void Awake()
    {
        _element = GetComponent<ElementSwitcher>();
        _selector = GetComponent<TargetSelector>();
        if (!mana) mana = GetComponent<ManaSystem>();
        if (!anim) anim = GetComponentInChildren<AnimationManager>();

        if (!spawnPoint)
            Debug.LogWarning("[FireballShooter] No asignaste spawnPoint; se usará la posición del Player.");
    }

    void Update()
    {
        if (_cd > 0f) _cd -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && _element.current == Element.Fire)
        {
            if (_cd > 0f)
            {
                Debug.Log($"[Fireball] En cooldown: {CooldownRemaining:0.0}s");
                return;
            }

            var target = _selector.CurrentTarget;
            if (target == null)
            {
                Debug.Log("[Fireball] Necesitás tener un enemigo seleccionado.");
                return;
            }

            if (mana != null && !mana.TrySpend(manaCost))
            {
                Debug.Log($"[Fireball] Maná insuficiente. Requiere {manaCost}");
                return;
            }

            if (anim) anim.PlayFireAttack();

            ShootAt(target.transform);
            _cd = cooldown;
        }
    }

    void ShootAt(Transform target)
    {
        
        Vector3 origin = spawnPoint ? spawnPoint.position : (transform.position + Vector3.up * 1.2f);

       
        Vector3 aimPoint = GetAimPoint(target);

        
        Vector3 dir = (aimPoint - origin).normalized;
        if (lockYDirection) { dir.y = 0f; dir.Normalize(); }

        
        var go = Instantiate(fireballPrefab, origin, Quaternion.LookRotation(dir, Vector3.up));
        if (go.TryGetComponent<FireballProjectile>(out var proj))
            proj.Launch(dir, speed, lifetime, this.gameObject);
    }

    Vector3 GetAimPoint(Transform target)
    {
        
        Collider[] cols = target.GetComponentsInChildren<Collider>(true);
        Collider best = null;
        float bestDistSq = float.PositiveInfinity;

        foreach (var c in cols)
        {
            if (!c || c.isTrigger) continue; 
            float d2 = (c.bounds.center - transform.position).sqrMagnitude;
            if (d2 < bestDistSq) { best = c; bestDistSq = d2; }
        }

        if (best != null)
            return best.bounds.center;

        
        if (target.TryGetComponent<Renderer>(out var r))
            return r.bounds.center + Vector3.down * 0.5f;

        var rend = target.GetComponentInChildren<Renderer>();
        if (rend) return rend.bounds.center + Vector3.down * 0.5f;

        
        return target.position + Vector3.up * 1.0f;
    }
}
