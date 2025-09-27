using UnityEngine;

public class MiniMapPlayerMarker : MonoBehaviour
{
    public Transform target;            
    public RectTransform markerUI;      
    public RectTransform minimap;      
    public Camera minimapCamera;        

    public bool clampToEdge = true;
    public float edgePadding = 6f;

    void Reset()
    {
        markerUI = GetComponent<RectTransform>();
    }

    void Update()
    {
        if (!target || !markerUI || !minimap || !minimapCamera) return;

        
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
