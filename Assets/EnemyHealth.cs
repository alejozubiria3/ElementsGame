using UnityEngine;
using System.Collections;

public class EnemyHealth : MonoBehaviour
{
    [Header("Vida")]
    public float maxHealth = 40f;
    public float currentHealth;
    public bool isDead { get; private set; }

    [Header("Feedback Visual")]
    [Tooltip("Si lo dejas vacío, se buscan todos los Renderers en hijos automáticamente.")]
    public Renderer[] renderersToColor;
    public Color hitColor = Color.red;                 
    public Color burnColor = new Color(1f, 0.4f, 0f);  
    public float flashTime = 0.12f;                    
    
    MaterialPropertyBlock _mpb;
    
    static readonly int _BaseColor = Shader.PropertyToID("_BaseColor");
    static readonly int _Color = Shader.PropertyToID("_Color");
    static readonly int _EmissionColor = Shader.PropertyToID("_EmissionColor");

    bool _isBurning;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (renderersToColor == null || renderersToColor.Length == 0)
            renderersToColor = GetComponentsInChildren<Renderer>(true);

        _mpb = new MaterialPropertyBlock();
        ClearTint(); 

    public void TakeDamage(float amount, bool isDot = false)
    {
        if (isDead) return;

        currentHealth = Mathf.Max(0f, currentHealth - amount);

        if (isDot)
        {
            
            ApplyBurnVisual(true);
        }
        else
        {
            
            if (!_isBurning) 
                StartCoroutine(FlashHit());
        }

        if (currentHealth <= 0f)
            Die();
    }

    IEnumerator FlashHit()
    {
        SetTint(hitColor, emission: 0.0f); 
        yield return new WaitForSeconds(flashTime);
        
        if (!_isBurning) ClearTint();
        else SetTint(burnColor, emission: 0.0f);
    }

    public void ApplyBurnVisual(bool on)
    {
        _isBurning = on;
        if (on) SetTint(burnColor, emission: 0.0f);
        else    ClearTint();
    }

    void SetTint(Color c, float emission = 0.0f)
    {
        if (renderersToColor == null) return;
        foreach (var r in renderersToColor)
        {
            if (!r) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(_BaseColor, c); 
            _mpb.SetColor(_Color, c);     
            if (emission > 0f)
                _mpb.SetColor(_EmissionColor, c * emission);
            r.SetPropertyBlock(_mpb);
        }
    }

    public void ClearTint()
    {
        if (renderersToColor == null) return;
        foreach (var r in renderersToColor)
        {
            if (!r) continue;
            r.SetPropertyBlock(null); 
        }
    }

    void Die()
    {
        if (isDead) return;
        isDead = true;
        Destroy(gameObject);
    }
}
