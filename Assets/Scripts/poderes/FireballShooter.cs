using UnityEngine;

[RequireComponent(typeof(ElementSwitcher), typeof(TargetSelector))]
public class FireballShooter : MonoBehaviour
{
    [Header("Refs")]
    [SerializeField] private GameObject fireballPrefab;
    [SerializeField] private ManaSystem mana;   

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
    }

    void Update()
    {
        if (_cd > 0f) _cd -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q) && _cd <= 0f && _element.current == Element.Fire)
        {
            var target = _selector.CurrentTarget;
            if (target == null)
            {
                Debug.Log("[Fireball] No hay objetivo seleccionado.");
                return;
            }

            // gastar maná ANTES de disparar
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
        Vector3 origin = transform.position + Vector3.up * 1.2f;
        Vector3 dir = (target.position - origin);
        dir.y = 0f;
        dir.Normalize();

        var go = Instantiate(fireballPrefab, origin + dir * 0.8f, Quaternion.LookRotation(dir));
        if (go.TryGetComponent<FireballProjectile>(out var proj))
            proj.Launch(dir, speed, lifetime, this.gameObject);
    }
}  