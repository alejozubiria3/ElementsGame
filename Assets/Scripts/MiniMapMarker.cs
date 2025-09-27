using UnityEngine;

public class MiniMapMarker : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;                 // enemigo (la instancia)
    public RectTransform markerUI;           // este RectTransform (del puntito)
    public RectTransform minimap;            // panel del minimapa (RectTransform padre)
    public Camera minimapCamera;             // cámara del minimapa
    public Transform center;                 // Player

    [Header("Auto-wire (por si algo está None)")]
    [SerializeField] string minimapCameraName = "CamaraMiniMapa"; // cámbialo si tu cámara se llama distinto

    [Header("Rango (único criterio)")]
    public float visionRadius = 50f;

    [Header("Borde (opcional)")]
    public bool clampToEdge = true;
    public float edgePadding = 6f;

    [Header("Auto-remoción")]
    public bool autoDestroyWhenTargetGone = true;
    private EnemyHealth _health;

    void Awake()
    {
        AutoWire();
        if (target) _health = target.GetComponent<EnemyHealth>();
    }

    void AutoWire()
    {
        if (!markerUI) markerUI = GetComponent<RectTransform>();
        if (!minimap)
        {
            // si el puntito está colgado bajo el panel, su padre ES el panel
            if (transform.parent is RectTransform rtParent) minimap = rtParent;
        }
        if (!minimapCamera)
        {
            var go = GameObject.Find(minimapCameraName);
            if (go) minimapCamera = go.GetComponent<Camera>();
            if (!minimapCamera) minimapCamera = Camera.main; // último recurso
        }
    }

    void Update()
    {
        // por si algo se pierde en runtime
        if (!markerUI || !minimap || !minimapCamera) AutoWire();

        // destruir SOLO si murió o se destruyó
        if (autoDestroyWhenTargetGone && (target == null || (_health && _health.isDead)))
        {
            if (markerUI) Destroy(markerUI.gameObject);
            Destroy(this);
            return;
        }

        if (!target || !markerUI || !minimap || !minimapCamera || !center) return;

        // ON/OFF solo por distancia (esto hace que vuelva a prender al entrar)
        Vector3 d = target.position - center.position; d.y = 0f;
        bool inRange = d.sqrMagnitude <= visionRadius * visionRadius;

        if (markerUI.gameObject.activeSelf != inRange)
            markerUI.gameObject.SetActive(inRange);

        if (!inRange) return;

        // Posicionamiento
        Vector3 vp = minimapCamera.WorldToViewportPoint(target.position);
        Rect r = minimap.rect;
        float w = r.width, h = r.height;
        Vector2 anchored = new Vector2((vp.x - 0.5f) * w, (vp.y - 0.5f) * h);

        if (clampToEdge)
        {
            float hw = w * 0.5f - edgePadding;
            float hh = h * 0.5f - edgePadding;
            anchored.x = Mathf.Clamp(anchored.x, -hw, hw);
            anchored.y = Mathf.Clamp(anchored.y, -hh, hh);
        }

        markerUI.anchoredPosition = anchored;
    }
}
