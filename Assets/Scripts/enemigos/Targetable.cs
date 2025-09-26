using UnityEngine;

[DisallowMultipleComponent]
public class Targetable : MonoBehaviour
{
    [Header("Feedback")]
    public Renderer targetRenderer;
    public Color selectedColor = Color.yellow;
    private Material _mat;
    private Color _base;

    void Awake()
    {
        if (!targetRenderer) targetRenderer = GetComponentInChildren<Renderer>();
        if (targetRenderer)
        {
            _mat = targetRenderer.material;
            _base = _mat.color;
        }
    }

    public void SetSelected(bool on)
    {
        if (_mat) _mat.color = on ? selectedColor : _base;
    }
} 
