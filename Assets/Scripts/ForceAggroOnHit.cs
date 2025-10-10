using UnityEngine;

public class ForceAggroOnHit : MonoBehaviour
{
    [Header("Referencia")]
    public EnemyMelee enemy;                    

    [Header("Quién dispara el agro")]
    [Tooltip("Tags que cuentan como proyectil (ej: \"Bullet\", \"Arrow\", etc.)")]
    public string[] projectileTags = new string[] { "Bullet" };

    [Tooltip("Layers que cuentan como proyectil (dejar en Nothing si no usás)")]
    public LayerMask projectileLayers = 0;      

    [Header("Player para el agro")]
    public string playerTag = "Player";         

    [Header("Debug")]
    public bool debugLogs = true;

    void Awake()
    {
        if (!enemy) enemy = GetComponentInParent<EnemyMelee>();
        if (!enemy && debugLogs) Debug.LogWarning("[ForceAggroOnHit] No se encontró EnemyMelee en padres.");
    }

    
    private void OnTriggerEnter(Collider other)
    {
        if (IsProjectile(other.gameObject))
            Force();
    }

    
    private void OnCollisionEnter(Collision collision)
    {
        if (collision != null && IsProjectile(collision.collider.gameObject))
            Force();
    }

    
    private void OnParticleCollision(GameObject other)
    {
        if (IsProjectile(other))
            Force();
    }

    private bool IsProjectile(GameObject go)
    {
        if (!go) return false;

        
        if (projectileTags != null)
        {
            for (int i = 0; i < projectileTags.Length; i++)
            {
                var t = projectileTags[i];
                if (!string.IsNullOrEmpty(t) && go.CompareTag(t))
                    return true;
            }
        }

        
        if (projectileLayers.value != 0)
        {
            int layerMask = 1 << go.layer;
            if ((projectileLayers.value & layerMask) != 0)
                return true;
        }

        return false;
    }

    private void Force()
    {
        if (!enemy) return;

        Transform playerTr = null;
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) playerTr = p.transform;

        enemy.ForceAggro(playerTr);  
        if (debugLogs) Debug.Log("[ForceAggroOnHit] Impacto detectado → agro forzado.");
    }
}
