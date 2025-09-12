using UnityEngine;
using System.Collections.Generic;

public class EnemySpawner : MonoBehaviour
{
    [Header("Referencias")]
    public MapGenerator map;          // arrastrá tu MapGenerator en el Inspector
    public GameObject enemyPrefab;    // arrastrá tu prefab de enemigo

    [Header("Spawn Settings")]
    public int enemyCount = 5;        // cuántos enemigos spawnear
    public float yOffset = 1f;        // altura al instanciar

    void Start()
    {
        SpawnEnemies();
    }

    void SpawnEnemies()
    {
        if (!map || !enemyPrefab) return;

        HashSet<Vector2Int> used = new();

        for (int i = 0; i < enemyCount; i++)
        {
            int x, z;
            int safety = 0;

            // elegir una celda distinta
            do
            {
                x = Random.Range(0, map.width);
                z = Random.Range(0, map.height);
                safety++;
            } while (used.Contains(new Vector2Int(x, z)) && safety < 50);

            used.Add(new Vector2Int(x, z));

            // calcular posición en esa celda
            Vector3 pos = new Vector3(
                x * map.cellSize + map.cellSize / 2f,
                yOffset,
                z * map.cellSize + map.cellSize / 2f
            );

            Instantiate(enemyPrefab, pos, Quaternion.identity, transform);
        }
    }
}
