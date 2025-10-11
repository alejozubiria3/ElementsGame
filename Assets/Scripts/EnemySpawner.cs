using UnityEngine;
using System.Linq;
using System.Collections.Generic;
using UnityEngine.AI; 

public class EnemySpawner : MonoBehaviour
{
    [Header("Referencias")]
    public MapGenerator map;                 
    public GameObject meleePrefab;
    public GameObject rangedPrefab;

    [Header("Spawn Settings")]
    public int enemyCount = 10;

    [Tooltip("Separación final del suelo para evitar interpenetración")]
    public float extraY = 0.05f;

    [Header("Ajuste al suelo")]
    [SerializeField] LayerMask groundMask = ~0;  
    [SerializeField] float snapUp = 50f;         
    [SerializeField] float snapDown = 200f;      
    [SerializeField] bool tryNavMesh = true;     

    [Header("Spawnpoints (opcional)")]
    public Transform[] spawnPoints;              
    public bool autoUseChildSpawnPoints = true;  
    public float spawnRadius = 3f;               
    public int maxPerPoint = 3;                  

    [Header("Tipo de enemigo")]
    [Range(0f, 1f)] public float rangedChance = 0.5f;

    [Header("Minimapa (para markers de enemigos)")]
    public RectTransform minimapPanel;
    public RectTransform enemyMarkerPrefab;
    public Camera minimapCamera;
    public Transform minimapCenter;
    public float markerVisionRadius = 50f;
    public bool markerClampToEdge = true;
    public float markerEdgePadding = 6f;

    void Start()
    {
        
        if ((spawnPoints == null || spawnPoints.Length == 0) && autoUseChildSpawnPoints)
            spawnPoints = GetComponentsInChildren<Transform>(true).Where(t => t != transform).ToArray();

        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        if (meleePrefab == null && rangedPrefab == null)
        {
            Debug.LogWarning("[EnemySpawner] No hay prefabs asignados.");
            return;
        }

        if (spawnPoints != null && spawnPoints.Length > 0)
            SpawnUsingPoints_Safe();
        else
            SpawnUsingGrid();
    }

    
    void SpawnUsingPoints_Safe()
    {
        int capacity = Mathf.Max(0, spawnPoints.Length * Mathf.Max(0, maxPerPoint));
        int toSpawn = Mathf.Min(enemyCount, capacity);
        if (toSpawn < enemyCount)
            Debug.LogWarning($"[EnemySpawner] Capacidad insuficiente: pedidos {enemyCount}, capacidad {capacity}. Se spawnearán {toSpawn}.");

        int spawned = 0;

        while (spawned < toSpawn)
        {
            for (int i = 0; i < spawnPoints.Length && spawned < toSpawn; i++)
            {
                int slots = Mathf.Min(maxPerPoint, toSpawn - spawned);
                for (int j = 0; j < slots && spawned < toSpawn; j++)
                {
                    Vector3 c = spawnPoints[i].position;
                    Vector2 rnd = Random.insideUnitCircle * spawnRadius;
                    Vector3 guess = new Vector3(c.x + rnd.x, c.y, c.z + rnd.y);

                    if (!TrySnapToGround(ref guess)) continue;

                    var enemy = InstantiateRandomEnemy(guess, Quaternion.Euler(0, Random.Range(0f, 360f), 0f));
                    if (enemy) CreateMinimapMarkerFor(enemy.transform);
                    spawned++;
                }
            }
        }
    }

    
    void SpawnUsingGrid()
    {
        if (!map)
        {
            Debug.LogWarning("[EnemySpawner] No hay MapGenerator ni spawnpoints definidos.");
            return;
        }

        HashSet<Vector2Int> used = new();
        int maxCells = map.width * map.height;
        int toSpawn = Mathf.Min(enemyCount, maxCells);

        for (int i = 0; i < toSpawn; i++)
        {
            int x, z, safety = 0;
            Vector2Int cell;
            do
            {
                x = Random.Range(0, map.width);
                z = Random.Range(0, map.height);
                cell = new Vector2Int(x, z);
                safety++;
                if (safety > 100) break;
            } while (used.Contains(cell));

            used.Add(cell);

            Vector3 guess = new Vector3(
                x * map.cellSize + map.cellSize * 0.5f,
                0f, 
                z * map.cellSize + map.cellSize * 0.5f
            );

            if (!TrySnapToGround(ref guess)) continue;

            var enemy = InstantiateRandomEnemy(guess, Quaternion.identity);
            if (enemy) CreateMinimapMarkerFor(enemy.transform);
        }
    }

    
    GameObject InstantiateRandomEnemy(Vector3 position, Quaternion rotation)
    {
        GameObject prefab = null;
        if (meleePrefab && rangedPrefab)
            prefab = (Random.value < rangedChance) ? rangedPrefab : meleePrefab;
        else if (meleePrefab)  prefab = meleePrefab;
        else if (rangedPrefab) prefab = rangedPrefab;

        if (!prefab) return null;
        return Instantiate(prefab, position, rotation, transform);
    }

    void CreateMinimapMarkerFor(Transform enemy)
    {
        if (!minimapPanel || !enemyMarkerPrefab || !minimapCamera || !minimapCenter || !enemy) return;

        var marker = Instantiate(enemyMarkerPrefab, minimapPanel);
        marker.sizeDelta = new Vector2(10, 10);

        var mm = marker.GetComponent<MiniMapEnemyMarker>();
        if (!mm) mm = marker.gameObject.AddComponent<MiniMapEnemyMarker>();

        mm.target        = enemy;
        mm.markerUI      = marker;
        mm.minimap       = minimapPanel;
        mm.minimapCamera = minimapCamera;
        mm.center        = minimapCenter;
        mm.visionRadius  = markerVisionRadius;
        mm.clampToEdge   = markerClampToEdge;
        mm.edgePadding   = markerEdgePadding;
        mm.autoDestroyWhenTargetGone = true;
    }

  
    bool TrySnapToGround(ref Vector3 pos)
    {
        
        Vector3 start = pos + Vector3.up * snapUp;
        float maxDist = snapUp + snapDown;

        if (Physics.Raycast(start, Vector3.down, out RaycastHit hit, maxDist, groundMask, QueryTriggerInteraction.Ignore))
        {
            
            if (Vector3.Angle(hit.normal, Vector3.up) > 55f)
                return false;

            pos = hit.point + Vector3.up * extraY;

            
            if (tryNavMesh && NavMesh.SamplePosition(pos, out NavMeshHit nHit, 1.0f, NavMesh.AllAreas))
                pos = nHit.position + Vector3.up * extraY;

            return true;
        }

        
        if (tryNavMesh && NavMesh.SamplePosition(pos, out NavMeshHit nOnly, snapDown, NavMesh.AllAreas))
        {
            pos = nOnly.position + Vector3.up * extraY;
            return true;
        }

        Debug.LogWarning($"[EnemySpawner] No se encontró suelo para {pos}. Revisa groundMask/escena.");
        return false;
    }
}
