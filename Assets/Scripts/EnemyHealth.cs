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

    
    private MaterialPropertyBlock _mpb;
    private static readonly int BaseColorID     = Shader.PropertyToID("_BaseColor"); 
    private static readonly int ColorID         = Shader.PropertyToID("_Color");     
    private static readonly int EmissionColorID = Shader.PropertyToID("_EmissionColor");

    private bool _isBurning;

    void Awake()
    {
        currentHealth = maxHealth;
        isDead = false;

        if (renderersToColor == null || renderersToColor.Length == 0)
            renderersToColor = GetComponentsInChildren<Renderer>(true);

        _mpb = new MaterialPropertyBlock();
        ClearTint();
    }

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

    private IEnumerator FlashHit()
    {
        SetTint(hitColor, 0f);
        yield return new WaitForSeconds(flashTime);
        if (!_isBurning) ClearTint();
        else SetTint(burnColor, 0f);
    }

    public void ApplyBurnVisual(bool on)
    {
        _isBurning = on;
        if (on) SetTint(burnColor, 0f);
        else    ClearTint();
    }

    private void SetTint(Color c, float emissionFactor)
    {
        if (renderersToColor == null) return;
        foreach (var r in renderersToColor)
        {
            if (!r) continue;
            r.GetPropertyBlock(_mpb);
            _mpb.SetColor(BaseColorID, c); 
            _mpb.SetColor(ColorID, c);     
            if (emissionFactor > 0f)
                _mpb.SetColor(EmissionColorID, c * emissionFactor);
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

    private void Die()
    {
        if (isDead) return;
        isDead = true;
        Destroy(gameObject);
    }
}
