using UnityEngine;

[RequireComponent(typeof(ElementSwitcher))]
public class WaterShield : MonoBehaviour
{
    [Header("Referencias")]
    [SerializeField] private GameObject shieldSphere;
    [SerializeField] private PlayerSimpleHealth playerHealth;
    [SerializeField] private ManaSystem mana;

    [Header("Costos")]
    [SerializeField] private float manaCost = 20f;

    [Header("Timing")]
    [SerializeField] private float duration = 3f;
    [SerializeField] private float cooldown = 8f;

    // ---- GETTERS para la UI ----
    public bool IsActive => _active;
    public float ActiveTotal => duration;
    public float ActiveRemaining => _active ? Mathf.Max(0f, _activeEndTime - Time.time) : 0f;

    public float CooldownRemaining => Mathf.Max(0f, _cdTimer);
    public float CooldownTotal => cooldown;
    public float Cooldown01 => (cooldown <= 0f) ? 0f : Mathf.Clamp01(_cdTimer / cooldown);

    private ElementSwitcher _element;
    private float _cdTimer;
    private bool _active;
    private float _activeEndTime; 

    [SerializeField] private AnimationManager anim; // 👈 Referencia al animador


    void Awake()
    {
        _element = GetComponent<ElementSwitcher>();
        if (!playerHealth) playerHealth = GetComponent<PlayerSimpleHealth>();
        if (!mana) mana = GetComponent<ManaSystem>();
        if (!anim) anim = GetComponentInChildren<AnimationManager>(); // 👈 busca en hijos


        if (shieldSphere != null)
            shieldSphere.SetActive(false);
        else
            Debug.LogError("[WaterShield] No asignaste la esfera en el Inspector!");
    }

    void Update()
    {
        if (_cdTimer > 0f) _cdTimer -= Time.deltaTime;

        if (Input.GetKeyDown(KeyCode.Q))
        {
            if (_element.current != Element.Water)
            {
                Debug.Log("[WaterShield] No se activa: estás en " + _element.current);
            }
            else if (_active)
            {
                Debug.Log("[WaterShield] Ya está activo.");
            }
            else if (_cdTimer > 0f)
            {
                Debug.Log("[WaterShield] En cooldown: " + _cdTimer.ToString("0.0") + "s restantes");
            }
            else
            {
                if (mana == null || mana.TrySpend(manaCost))
                {
                    Activate();
                }
                else
                {
                    Debug.Log("[WaterShield] No hay suficiente maná (necesitas " + manaCost + ")");
                }
            }
        }

        
        if (_active && _element.current != Element.Water)
        {
            Debug.Log("[WaterShield] Elemento cambiado → Escudo desactivado");
            Deactivate();
        }
    }

    void Activate()
    {
        if (anim) anim.PlayWaterShield();
        if (shieldSphere) shieldSphere.SetActive(true);
        if (playerHealth) playerHealth.isInvulnerable = true;

        _active = true;
        _cdTimer = cooldown;

        _activeEndTime = Time.time + duration;


        CancelInvoke(nameof(Deactivate));
        Invoke(nameof(Deactivate), duration);

        Debug.Log("[WaterShield] ACTIVADO por " + duration + "s. (Coste: " + manaCost + ")");
    }

    void Deactivate()
    {
        if (shieldSphere) shieldSphere.SetActive(false);
        if (playerHealth) playerHealth.isInvulnerable = false;

        _active = false;
        _activeEndTime = 0f; // limpia

        CancelInvoke(nameof(Deactivate));
        Debug.Log("[WaterShield] DESACTIVADO");
    }
}    