using UnityEngine;
using UnityEngine.UI;

public enum Element { Fire, Water }

[DisallowMultipleComponent]
public class ElementSwitcher : MonoBehaviour
{
    [Header("Estado actual")]
    public Element current = Element.Fire;

    [Header("Referencias visuales en el Player")]
    public Renderer targetRenderer;   
    public Light targetLight;         

    [Header("Colores")]
    public Color fireColor = new Color(1f, 0.3f, 0.1f); 
    public Color waterColor = Color.cyan;               

    [Header("UI de Elemento")]
    public Image elementIcon;    
    public Sprite fireSprite;    
    public Sprite waterSprite;   

    [Header("Flash de feedback")]
    public float flashDuration = 0.18f;
    public AnimationCurve flashCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private Material _matInstance;
    private Color _baseColor;
    private Coroutine _flashCo;

    void Awake()
    {
       
        if (!targetRenderer) targetRenderer = GetComponentInChildren<Renderer>();
        if (targetRenderer) _matInstance = targetRenderer.material;

        ApplyVisuals(); 
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Tab))
        {
            ToggleElement();
        }
    }

    public void ToggleElement()
    {
        current = (current == Element.Fire) ? Element.Water : Element.Fire;
        ApplyVisuals();

        if (_flashCo != null) StopCoroutine(_flashCo);
        _flashCo = StartCoroutine(FlashFeedback());

        Debug.Log("Elemento actual: " + current);
    }

    void ApplyVisuals()
    {
        
        if (_matInstance)
        {
            _baseColor = (current == Element.Fire) ? fireColor : waterColor;
            _matInstance.color = _baseColor;

            _matInstance.EnableKeyword("_EMISSION");
            _matInstance.SetColor("_EmissionColor", _baseColor * 0f);
        }

        if (targetLight)
        {
            targetLight.color = _baseColor;
        }

        
        if (elementIcon != null)
        {
            elementIcon.sprite = (current == Element.Fire) ? fireSprite : waterSprite;
        }
    }

    System.Collections.IEnumerator FlashFeedback()
    {
        float t = 0f;
        while (t < flashDuration)
        {
            t += Time.deltaTime;
            float k = flashCurve.Evaluate(t / flashDuration);

            if (_matInstance)
                _matInstance.SetColor("_EmissionColor", _baseColor * Mathf.Lerp(0f, 2f, k));

            yield return null;
        }

       
        if (_matInstance)
            _matInstance.SetColor("_EmissionColor", _baseColor * 0f);
    }
}