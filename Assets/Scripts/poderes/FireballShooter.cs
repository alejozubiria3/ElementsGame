using UnityEngine;

[RequireComponent(typeof(ElementSwitcher), typeof(TargetSelector))]
public class FireballShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private ManaSystem mana;
    [Tooltip("Punto de spawn de la bola de fuego (vacío en la mano, frente del player, etc.)")]
    [SerializeField] private Transform spawnPoint;

    [Header("Costos")]
    [SerializeField] private float manaCost = 15f;

    [Header("Disparo")]
    [SerializeField] private float speed = 24f;
    [SerializeField] private float lifetime = 4f;
    [SerializeField] private float cooldown = 0.35f;

    private ElementSwitcher _element;
    private TargetSelector _selector;
    private float _cd;

    void Awake()
    {
        _element = GetComponent<ElementSwitcher>();
        _selector = GetComponent<TargetSelector>();
        if (!mana) mana = GetComponent<ManaSystem>();

        if (!spawnPoint)
            Debug.LogWarning("[FireballShooter] No asignaste spawnPoint, se usará la posición del Player.");
    }

    void Update()
    {
        if (_cd > 0f) _cd -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && _cd <= 0f && _element.current == Element.Fire)
        {
            var target = _selector.CurrentTarget;

            
            if (target == null)
            {
                Debug.Log("[Fireball] Necesitás tener un enemigo seleccionado para disparar.");
                return;
            }

            
            if (mana != null && !mana.TrySpend(manaCost))
            {
                Debug.Log("[Fireball] Maná insuficiente. Requiere " + manaCost);
                return;
            }

            ShootAt(target.transform);
            _cd = cooldown;
        }
    }

    void ShootAt(Transform target)
    {
        
        Vector3 origin = spawnPoint ? spawnPoint.position : transform.position + Vector3.up * 1.2f;

        
        Vector3 dir = (target.position - origin).normalized;

        var go = Instantiate(fireballPrefab, origin, Quaternion.LookRotation(dir, Vector3.up));

        if (go.TryGetComponent<FireballProjectile>(out var proj))
            proj.Launch(dir, speed, lifetime, this.gameObject);
    }
}
