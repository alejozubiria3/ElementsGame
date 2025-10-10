using UnityEngine;

/// Poné este script en el OBJETO con el CapsuleCollider (IsTrigger = true).
/// Asigná "owner" al EnemyRanged del root.
public class EnemyDetectionTrigger : MonoBehaviour
{
    public EnemyRanged owner;
    public string playerTag = "Player";

    void Awake()
    {
        if (!owner) owner = GetComponentInParent<EnemyRanged>();
    }

    void OnTriggerEnter(Collider other)
    {
        if (!owner) return;
        if (other.CompareTag(playerTag))
            owner.StartChase(other);
    }

    void OnTriggerExit(Collider other)
    {
        if (!owner) return;
        if (other.CompareTag(playerTag))
            owner.StopChase(other);
    }
}
