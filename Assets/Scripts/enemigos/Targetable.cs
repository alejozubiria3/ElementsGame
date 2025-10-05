using UnityEngine;

[DisallowMultipleComponent]
public class Targetable : MonoBehaviour
{
    [Header("Renderers a resaltar (si lo dejás vacío, busca todos en hijos)")]
    public Renderer[] renderers;

    [Header("Feedback visual")]
    public Color tintColor = Color.yellow;
    public Color emissionColor = new Color(1f, 0.6f, 0.1f);
    [Range(0f, 6f)] public float emissionIntensity = 3f;
    public bool pulseWhenSelected = true;
    [Range(0f, 0.3f)] public float pulseScale = 0.08f;
    [Range(0.5f, 10f)] public float pulseSpeed = 4f;

    [Header("Plan B: activar un hijo (anillo/ícono) al seleccionar")]
    public Transform selectionVisual;      
    public bool selectionBillboard = true;

    [Header("Compatibilidad / Debug")]
    public bool forceMaterialOverride = true; 
    public bool debugLog = false;

    
    static readonly int ColorID          = Shader.PropertyToID("_Color");
    static readonly int BaseColorID      = Shader.PropertyToID("_BaseColor");
    static readonly int EmissionColorID  = Shader.PropertyToID("_EmissionColor");      
    static readonly int EmissiveHDRPID   = Shader.PropertyToID("_EmissiveColor");      
    static readonly int EmissiveLDRHDRP  = Shader.PropertyToID("_EmissiveColorLDR");   

    MaterialPropertyBlock _mpb;
    bool _selected;
    Vector3 _baseScale;
    Camera _cam;

    void Awake()
    {
        if (renderers == null || renderers.Length == 0)
            renderers = GetComponentsInChildren<Renderer>(true);

        _mpb = new MaterialPropertyBlock();
        _baseScale = transform.localScale;
        _cam = Camera.main;

        ApplyTint(false, 0f);
        if (selectionVisual) selectionVisual.gameObject.SetActive(false);
    }

    void Update()
    {
        if (_selected && pulseWhenSelected)
        {
            float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = _baseScale * s;
        }

        if (_selected && selectionVisual && selectionBillboard && _cam)
        {
            Vector3 fwd = (selectionVisual.position - _cam.transform.position).normalized;
            selectionVisual.rotation = Quaternion.LookRotation(fwd, Vector3.up);
        }
    }

    void OnDisable()
    {
        transform.localScale = _baseScale;
        ApplyTint(false, 0f);
        if (selectionVisual) selectionVisual.gameObject.SetActive(false);
        _selected = false;
    }

    public void SetSelected(bool on)
    {
        _selected = on;
        transform.localScale = _baseScale;
        ApplyTint(on, on ? emissionIntensity : 0f);
        if (selectionVisual) selectionVisual.gameObject.SetActive(on);
        if (debugLog) Debug.Log($"[Targetable] {(on ? "SELECTED" : "DESELECTED")} {name}");
    }

    void ApplyTint(bool on, float emiss)
    {
        if (renderers == null) return;

        foreach (var r in renderers)
        {
            if (!r) continue;

            
            r.GetPropertyBlock(_mpb);
            if (on)
            {
                _mpb.SetColor(ColorID,     tintColor);
                _mpb.SetColor(BaseColorID, tintColor);
                _mpb.SetColor(EmissionColorID, emissionColor * emiss);
                _mpb.SetColor(EmissiveHDRPID,   emissionColor * emiss);
                _mpb.SetColor(EmissiveLDRHDRP,  emissionColor * emiss);
            }
            else
            {
                _mpb.Clear();
            }
            r.SetPropertyBlock(_mpb);

            
            if (forceMaterialOverride)
            {
                var m = r.material; 
                if (on)
                {
                    if (m.HasProperty(ColorID))     m.SetColor(ColorID, tintColor);
                    if (m.HasProperty(BaseColorID)) m.SetColor(BaseColorID, tintColor);

                    bool hasEmissionProp = false;
                    if (m.HasProperty(EmissionColorID)) { m.SetColor(EmissionColorID, emissionColor * emiss); hasEmissionProp = true; }
                    if (m.HasProperty(EmissiveHDRPID))  { m.SetColor(EmissiveHDRPID,  emissionColor * emiss); hasEmissionProp = true; }
                    if (m.HasProperty(EmissiveLDRHDRP)) { m.SetColor(EmissiveLDRHDRP, emissionColor * emiss); hasEmissionProp = true; }

                    if (hasEmissionProp)
                    {
                        m.EnableKeyword("_EMISSION"); 
                        m.globalIlluminationFlags = MaterialGlobalIlluminationFlags.RealtimeEmissive;
                    }
                }
                else
                {
                    if (m.IsKeywordEnabled("_EMISSION")) m.DisableKeyword("_EMISSION");
                    
                    
                }
            }
        }
    }
}
