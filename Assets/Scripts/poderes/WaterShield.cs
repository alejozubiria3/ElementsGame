using UnityEngine;

[RequireComponent(typeof(ElementSwitcher))]
public class WaterShield : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject shieldSphere;           
    [SerializeField] private PlayerSimpleHealth playerHealth;   

    [Header("Timing")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private float cooldown = 8f;

    private ElementSwitcher _element;
    private float _cd;
    private bool _active;

    void Awake()
    {
        _element = GetComponent<ElementSwitcher>();
        if (!playerHealth) playerHealth = GetComponent<PlayerSimpleHealth>();

        if (shieldSphere != null)
        {
            shieldSphere.SetActive(false);
        }
        else
        {
            Debug.LogError("[WaterShield] No asignaste la esfera en el Inspector!");
        }
    }

    void Update()
    {
        if (_cd > 0f) _cd -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            Debug.Log("[WaterShield] Q presionada. Elemento actual: " + _element.current);

            if (_element.current == Element.Water && !_active && _cd <= 0f)
            {
                Activate();
            }
            else if (_element.current != Element.Water)
            {
                Debug.Log("[WaterShield] No se activa: estás en " + _element.current);
            }
        }

        
        if (_active && _element.current != Element.Water)
        {
            Debug.Log("[WaterShield] Elemento cambiado a " + _element.current + " -> Escudo roto");
            Deactivate();
        }
    }

    void Activate()
    {
        if (shieldSphere != null)
        {
            shieldSphere.SetActive(true);
        }

        if (playerHealth != null)
        {
            playerHealth.isInvulnerable = true;
        }

        _active = true;
        _cd = cooldown;

        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), duration);

        Debug.Log("[WaterShield] ACTIVADO (duración: " + duration + "s)");
    }

    void Deactivate()
    {
        if (shieldSphere != null)
        {
            shieldSphere.SetActive(false);
        }

        if (playerHealth != null)
        {
            playerHealth.isInvulnerable = false;
        }

        _active = false;
        CancelInvoke(nameof(Deactivate));

        Debug.Log("[WaterShield] DESACTIVADO");
    }
}
