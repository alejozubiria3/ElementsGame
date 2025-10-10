using UnityEngine;

public class ForceAggroOnHitAny : MonoBehaviour
{
    [Header("Referencias (opcionales)")]
    public EnemyMelee melee;
    public EnemyRanged ranged;

    [Header("Qué cuenta como proyectil")]
    [Tooltip("Tags que activan el agro (ej: Bullet, Flecha, Orbe)")]
    public string[] projectileTags = new string[] { "Bullet", "Flecha", "Orbe" };
    [Tooltip("Layers que activan el agro (dejar en 0 si no usás)")]
    public LayerMask projectileLayers = 0;

    [Header("Quién es el Player")]
    public string playerTag = "Player";

    [Header("Debug")]
    public bool debugLogs = false;

    void Awake()
    {
        if (!melee)  melee  = GetComponentInParent<EnemyMelee>();
        if (!ranged) ranged = GetComponentInParent<EnemyRanged>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (IsProjectile(other.gameObject))
            TryForce();
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision != null && IsProjectile(collision.collider.gameObject))
            TryForce();
    }

    void OnParticleCollision(GameObject other)
    {
        if (IsProjectile(other))
            TryForce();
    }

    private void TryForce()
    {
        Transform playerTr = null;
        var p = GameObject.FindGameObjectWithTag(playerTag);
        if (p) playerTr = p.transform;

        if (melee)  melee.ForceAggro(playerTr);
        if (ranged) ranged.ForceAggro(playerTr);

        if (debugLogs)
            Debug.Log("[ForceAggroOnHitAny] Impacto detectado → agro forzado en " +
                      (melee ? "Melee " : "") + (ranged ? "Ranged" : ""));
    }

    private bool IsProjectile(GameObject go)
    {
        if (!go) return false;

        // Por Tag
        if (projectileTags != null)
        {
            foreach (var t in projectileTags)
                if (!string.IsNullOrEmpty(t) && go.CompareTag(t))
                    return true;
        }

        // Por Layer
        if (projectileLayers.value != 0)
        {
            int mask = 1 << go.layer;
            if ((projectileLayers.value & mask) != 0)
                return true;
        }

        return false;
    }
}
