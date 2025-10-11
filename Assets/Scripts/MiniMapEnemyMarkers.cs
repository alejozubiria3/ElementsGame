using System.Collections.Generic;
using UnityEngine;

public class MiniMapEnemyMarkers : MonoBehaviour
{
    [Header("Referencias del minimapa")]
    public RectTransform minimap;
    public Camera minimapCamera;

    [Header("Marker")]
    public RectTransform enemyMarkerPrefab;
    public Transform markersParent; 

    [Header("Comportamiento")]
    public bool clampToEdge = true;
    public float edgePadding = 6f;
    public bool showOnlyWhenVisible = false;

    
    public static MiniMapEnemyMarkers Instance { get; private set; }

    private readonly Dictionary<Transform, RectTransform> markers = new Dictionary<Transform, RectTransform>();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Debug.LogWarning("[MiniMapEnemyMarkers] Ya existe otra instancia. Deshabilitando esta.");
            enabled = false; 
            return;
        }
        Instance = this;
        if (!markersParent) markersParent = minimap;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public void RegisterEnemy(Transform enemy)
    {
        if (!enemy || markers.ContainsKey(enemy)) return;
        if (!minimap || !minimapCamera || !enemyMarkerPrefab)
        {
            Debug.LogError("[MiniMapEnemyMarkers] Faltan referencias (minimap/camera/prefab).");
            return;
        }

        var ui = Instantiate(enemyMarkerPrefab, markersParent ? markersParent : minimap);
        ui.gameObject.SetActive(true);
        ui.gameObject.tag = "Untagged"; 
        markers.Add(enemy, ui);
    }

    public void UnregisterEnemy(Transform enemy)
    {
        if (!enemy) return;
        if (markers.TryGetValue(enemy, out var ui))
        {
            if (ui) Destroy(ui.gameObject);
            markers.Remove(enemy);
        }
    }

    void LateUpdate()
    {
        if (!minimap || !minimapCamera) return;

        Rect r = minimap.rect;
        float w = r.width, h = r.height;
        float halfW = w * 0.5f - edgePadding;
        float halfH = h * 0.5f - edgePadding;

        foreach (var kv in markers)
        {
            var target = kv.Key;
            var markerUI = kv.Value;
            if (!target || !markerUI) continue;

            Vector3 vp = minimapCamera.WorldToViewportPoint(target.position);
            Vector2 anchored = new Vector2((vp.x - 0.5f) * w, (vp.y - 0.5f) * h);

            bool visible = vp.z > 0f && vp.x >= 0f && vp.x <= 1f && vp.y >= 0f && vp.y <= 1f;

            if (clampToEdge)
            {
                anchored.x = Mathf.Clamp(anchored.x, -halfW, halfW);   
                anchored.y = Mathf.Clamp(anchored.y, -halfH, halfH);   
            }

            if (showOnlyWhenVisible)
            {
                if (markerUI.gameObject.activeSelf != visible)
                    markerUI.gameObject.SetActive(visible);
            }
            else
            {
                if (!markerUI.gameObject.activeSelf)
                    markerUI.gameObject.SetActive(true);
            }

            markerUI.anchoredPosition = anchored;
        }
    }
}
