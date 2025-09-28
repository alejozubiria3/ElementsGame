using UnityEngine;

public class MiniMapRegister : MonoBehaviour
{
    [SerializeField] private string requiredTag = "Enemy";

    void OnEnable()
    {
        if (CompareTag(requiredTag) && MiniMapEnemyMarkers.Instance)
            MiniMapEnemyMarkers.Instance.RegisterEnemy(transform);
    }

    void OnDisable()
    {
        if (CompareTag(requiredTag) && MiniMapEnemyMarkers.Instance)
            MiniMapEnemyMarkers.Instance.UnregisterEnemy(transform);
    }
}
