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

    
    [Header("Ground Setup")]
    public string groundLayerName = "Ground";
    public bool addMeshColliderIfMissing = false;

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
        int groundLayer = LayerMask.NameToLayer(groundLayerName);

        for (int x = 0; x < width; x++)
        {
            for (int z = 0; z < height; z++)
            {
                Vector3 pos = new Vector3(x * cellSize, 0f, z * cellSize);
                int index = GetRandomPrefabIndex(x, z);
                gridPrefabIndex[x, z] = index;

                GameObject obj = Instantiate(prefabs[index], pos, Quaternion.identity, transform);

                
                if (groundLayer != -1) SetLayerRecursively(obj, groundLayer);
                if (addMeshColliderIfMissing) EnsureCollider(obj);
            }
        }
    }

    int GetRandomPrefabIndex(int x, int z)
    {
        int index; int tries = 0;
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

    
    void SetLayerRecursively(GameObject go, int layer)
    {
        go.layer = layer;
        foreach (Transform t in go.transform)
            SetLayerRecursively(t.gameObject, layer);
    }

    void EnsureCollider(GameObject go)
    {
        
        if (go.GetComponentInChildren<Collider>(true) != null) return;

        var mf = go.GetComponentInChildren<MeshFilter>(true);
        if (mf && !go.GetComponent<MeshCollider>())
        {
            var mc = go.AddComponent<MeshCollider>();
            mc.sharedMesh = mf.sharedMesh; 
            mc.convex = false;
        }
    }
}
