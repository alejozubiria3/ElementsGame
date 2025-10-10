using UnityEngine;

/// Adjuntá este script a un collider del enemigo (puede ser el mismo
/// que usás para recibir daño o un "hurtbox" extra). Funciona con
/// EnemyMelee y EnemyRanged indistintamente.
///
/// Recomendado: un collider "grande" (isTrigger = true) en el root o un hijo.
public class ForceAggroOnHitAny : MonoBehaviour
{
    [Header("Referencias (auto)")]
    public EnemyMelee enemyMelee;
    public EnemyRanged enemyRanged;

    [Header("Qué cuenta como proyectil")]
    [Tooltip("Tags que cuentan como proyectil (\"Bullet\", \"Fireball\", \"Arrow\", etc.)")]
    public string[] projectileTags = new string[] { "Bullet", "Fireball", "Arrow" };

    [Tooltip("Layers que cuentan como proyectil (dejar en Nothing si no usás)")]
    public LayerMask projectileLayers = 0;

    [Header("Player para el agro")]
    public string playerTag = "Player";   // debe coincidir con tu Tag del jugador

    [Header("Debug")]
    public bool debugLogs = false;

    void Awake()
    {
        if (!enemyMelee)  enemyMelee  = GetComponentInParent<EnemyMelee>();
        if (!enemyRanged) enemyRanged = GetComponentInParent<EnemyRanged>();

        if (!enemyMelee && !enemyRanged && debugLogs)
            Debug.LogWarning("[ForceAggroOnHitAny] No encontré EnemyMelee ni EnemyRanged en padres.", this);
    }

    // --- Disparadores de impacto (cubrimos todas las variantes) ---
    void OnTriggerEnter(Collider other)            { TryForce(other); }
    void OnCollisionEnter(Collision c)             { if (c != null && c.collider) TryForce(c.collider); }
    void OnParticleCollision(GameObject otherGo)   { TryForce(otherGo ? otherGo.GetComponent<Collider>() : null); }

    void TryForce(Collider other)
    {
        if (!other) return;
        if (!IsProjectile(other.gameObject)) return;

        // Si podemos, pasamos como "atacante" la raíz del proyectil
        Transform attacker = other.transform.root;
        Force(attacker);
    }

    bool IsProjectile(GameObject go)
    {
        if (!go) return false;

        // Tags
        if (projectileTags != null)
        {
            for (int i = 0; i < projectileTags.Length; i++)
            {
                var t = projectileTags[i];
                if (!string.IsNullOrEmpty(t) && go.CompareTag(t))
                    return true;
            }
        }

        // Layers
        if (projectileLayers.value != 0)
        {
            int mask = 1 << go.layer;
            if ((projectileLayers.value & mask) != 0)
                return true;
        }

        return false;
    }

    void Force(Transform attackerOrProjectile)
    {
        // Intentamos conseguir el Transform del Player:
        Transform playerTr = null;

        // 1) por Tag
        var pGO = GameObject.FindGameObjectWithTag(playerTag);
        if (pGO) playerTr = pGO.transform;

        // 2) si no, por un componente típico del player (opcional)
        if (!playerTr)
        {
            var ph = FindObjectOfType<PlayerSimpleHealth>();
            if (ph) playerTr = ph.transform;
        }

        // 3) fallback: si no encontramos player, usamos el atacante recibido (sirve para orientar)
        if (!playerTr) playerTr = attackerOrProjectile;

        // Llamamos al agro del enemigo que exista
        if (enemyMelee)
            enemyMelee.ForceAggro(playerTr);

        if (enemyRanged)
            enemyRanged.ForceAggro(playerTr);

        if (debugLogs)
            Debug.Log($"[ForceAggroOnHitAny] Impacto → agro forzado. enemy= {(enemyMelee? "Melee": enemyRanged? "Ranged":"Ninguno")}, target={(playerTr?playerTr.name:"NULL")}", this);
    }
}
