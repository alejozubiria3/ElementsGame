using UnityEngine;

[DisallowMultipleComponent]
public class Targetable : MonoBehaviour
{
    [Header("Renderers a resaltar (si lo dejás vacío, busca todos en hijos)")]
    public Renderer[] renderers;

    [Header("Feedback visual")]
    public Color tintColor = Color.yellow;
    public Color emissionColor = new Color(1f, 0.6f, 0.1f);
    [Range(0f, 4f)] public float emissionIntensity = 2.2f;
    public bool pulseWhenSelected = true;
    [Range(0f, 0.25f)] public float pulseScale = 0.07f;   // 7% de pulso
    [Range(0.5f, 10f)] public float pulseSpeed = 4f;

    [Header("Anillo/ícono opcional (hijo)")]
    public Transform selectionVisual;     // ej: un Quad con textura de círculo
    public bool selectionBillboard = true;

    // IDs comunes (Built-in / URP / HDRP)
    static readonly int _ColorID         = Shader.PropertyToID("_Color");
    static readonly int _BaseColorID     = Shader.PropertyToID("_BaseColor");
    static readonly int _EmissionColorID = Shader.PropertyToID("_EmissionColor");

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

        // inicial: sin selección
        ApplyTint(false, 0f);
        if (selectionVisual) selectionVisual.gameObject.SetActive(false);
    }

    void Update()
    {
        // Pulso
        if (_selected && pulseWhenSelected)
        {
            float s = 1f + Mathf.Sin(Time.time * pulseSpeed) * pulseScale;
            transform.localScale = _baseScale * s;
        }

        // Hacer billboard del anillo/ícono
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
        transform.localScale = _baseScale;       // reseteo por si venía pulsando
        ApplyTint(on, on ? emissionIntensity : 0f);

        if (selectionVisual) selectionVisual.gameObject.SetActive(on);
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
                // seteo ambos nombres por compatibilidad de shader
                _mpb.SetColor(_ColorID, tintColor);
                _mpb.SetColor(_BaseColorID, tintColor);
                _mpb.SetColor(_EmissionColorID, emissionColor * emiss);
            }
            else
            {
                _mpb.Clear(); // vuelve a colores del material
            }

            r.SetPropertyBlock(_mpb);
        }
    }
}
