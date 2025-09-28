using UnityEngine;
using System.Collections.Generic;

public class MapGenerator : MonoBehaviour
{
    [Header("Grid Settings")]
    public int width = 10;          
    public int height = 10;         
    public float cellSize = 10f;    

    [Header("Prefabs")]
    public List<GameObject> prefabs;  

    [Header("Random Seed")]
    public int seed = 0;
    public bool useRandomSeed = true;

    
    private int[,] gridPrefabIndex;

    void Start()
    {
        if (useRandomSeed) seed = Random.Range(0, 99999);
        Random.InitState(seed);

        gridPrefabIndex = new int[width, height];
        GenerateMap();
    }

    void GenerateMap()
    {
        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0, z * cellSize);
                int index = GetRandomPrefabIndex(x, z);

                // Guardamos qu� prefab sali� en esta celda
                gridPrefabIndex[x, z] = index;

                GameObject obj = Instantiate(prefabs[index], pos, Quaternion.identity);
                obj.transform.parent = transform;
            }
        }
    }

    int GetRandomPrefabIndex(int x, int z)
    {
        int index;
        int tries = 0;

        do
        {
            index = Random.Range(0, prefabs.Count);
            tries++;

            
            if (x > 0 && gridPrefabIndex[x - 1, z] == index) continue;

            
            if (z > 0 && gridPrefabIndex[x, z - 1] == index) continue;

            
            return index;

        } while (tries < 10);

        
        return index;
    }
}
