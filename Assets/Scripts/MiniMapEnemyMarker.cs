using UnityEngine;

[DisallowMultipleComponent]
public class MiniMapEnemyMarker : MonoBehaviour
{
    [Header("Refs")]
    public Transform target;            
    public RectTransform markerUI;     
    public RectTransform minimap;       
    public Camera minimapCamera;        
    public Transform center;            

    [Header("Visibilidad por distancia")]
    public float visionRadius = 50f;   

    [Header("Borde (opcional)")]
    public bool clampToEdge = true;
    public float edgePadding = 6f;

    [Header("Limpieza (opcional)")]
    public bool autoDestroyWhenTargetGone = true; 
    private EnemyHealth _health;

    void Reset()
    {
        markerUI = GetComponent<RectTransform>();
    }

    void Awake()
    {
        if (!markerUI) markerUI = GetComponent<RectTransform>();
        if (target) _health = target.GetComponent<EnemyHealth>();
    }

    void Update()
    {
        
        if (autoDestroyWhenTargetGone && (target == null || (_health && _health.isDead)))
        {
            if (markerUI) Destroy(markerUI.gameObject);
            Destroy(this);
            return;
        }

        if (!target || !markerUI || !minimap || !minimapCamera || !center) return;

      
        Vector3 d = target.position - center.position; d.y = 0f;
        bool inRange = d.sqrMagnitude <= visionRadius * visionRadius;

        if (markerUI.gameObject.activeSelf != inRange)
            markerUI.gameObject.SetActive(inRange);

        if (!inRange) return; 

        
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
