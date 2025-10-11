using UnityEngine;

[ExecuteAlways]
public class HealthBarBillboard : MonoBehaviour
{
    [Header("Refs")]
    public Transform anchor;          
    public Camera targetCamera;       

    [Header("Offset / Escala")]
    public Vector3 worldOffset = new Vector3(0f, 2.0f, 0f); 
    public float fixedWorldScale = 1f; 

    RectTransform _rt;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        if (!targetCamera) targetCamera = Camera.main;
        if (!anchor && transform.parent) anchor = transform.parent;
    }

    void LateUpdate()
    {
        if (!anchor) return;
        if (!targetCamera) targetCamera = Camera.main;

        
        transform.position = anchor.position + worldOffset;

        
        Vector3 toCam = transform.position - targetCamera.transform.position;
        toCam.y = 0f; 
        if (toCam.sqrMagnitude > 0.0001f)
            transform.rotation = Quaternion.LookRotation(toCam);

        
        if (_rt)
            _rt.localScale = Vector3.one * fixedWorldScale;
    }
}
