using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class SelectionRing : MonoBehaviour
{
    [Header("Apariencia")]
    public float radius = 0.7f;
    public float width  = 0.05f;
    public Color color  = new Color(1f, 0.85f, 0.1f, 0.9f);
    public int segments = 64;

    [Header("Colocación")]
    public Transform center;       
    public float yOffset = 0.02f;  

    [Header("Pulse (opcional)")]
    public bool pulse = true;
    public float pulseAmount = 0.06f;  
    public float pulseSpeed  = 4f;

    LineRenderer lr;
    float baseRadius;

    void Awake()
    {
        lr = GetComponent<LineRenderer>();
        if (!center) center = transform.parent;

        lr.useWorldSpace = true;
        lr.loop = true;
        lr.positionCount = Mathf.Max(segments, 3);
        lr.startWidth = lr.endWidth = width;

        
        if (!lr.sharedMaterial)
        {
            var mat = new Material(Shader.Find("Sprites/Default"));
            mat.renderQueue = 3000; 
            lr.material = mat;
        }
        lr.startColor = lr.endColor = color;

        baseRadius = radius;
        gameObject.SetActive(false); 
    }

    void Update()
    {
        if (!center) return;

        
        Vector3 c = center.position;
        c.y += yOffset;
        transform.position = c;

        
        float r = baseRadius;
        if (pulse)
            r += Mathf.Sin(Time.time * pulseSpeed) * pulseAmount;

        
        int n = Mathf.Max(segments, 3);
        float step = Mathf.PI * 2f / n;
        for (int i = 0; i < n; i++)
        {
            float a = i * step;
            Vector3 p = new Vector3(c.x + Mathf.Cos(a) * r, c.y, c.z + Mathf.Sin(a) * r);
            lr.SetPosition(i, p);
        }
    }
}
