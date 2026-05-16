using UnityEngine;

public class ItemSpawner : MonoBehaviour
{
    [Header("Spawn Point")]
    public Transform spawnPoint;

    [Header("Loot Pools")]
    public GameObject[] normalLootPrefabs;
    public GameObject[] rareLootPrefabs;

    [Header("Overrides")]
    public GameObject presetPrefab;

    [Header("Cofre Settings")]
    public bool cofreRaro = false;
    [Range(0f, 1f)] public float spawnChanceNormal = 0.30f;
    [Range(0f, 1f)] public float spawnChanceRare = 0.40f;

    [Header("Debug")]
    public bool forceSpawn = false;

    private void Start()
    {
        if (spawnPoint == null)
        {
            spawnPoint = FindSpawnPoint();
        }

        if (spawnPoint == null)
        {
            Debug.LogWarning("ItemSpawner: no se encontro SpawnObjetos.", this);
            return;
        }

        if (spawnPoint.childCount > 0)
        {
            return;
        }

        float chance = cofreRaro ? spawnChanceRare : spawnChanceNormal;
        bool shouldSpawn = forceSpawn || Random.value <= chance;
        if (!shouldSpawn)
        {
            return;
        }

        GameObject prefabToSpawn = presetPrefab != null
            ? presetPrefab
            : GetRandomPrefab(cofreRaro ? rareLootPrefabs : normalLootPrefabs);

        if (prefabToSpawn == null)
        {
            Debug.LogWarning("ItemSpawner: no hay prefab valido para spawnear.", this);
            return;
        }

        Instantiate(prefabToSpawn, spawnPoint.position, spawnPoint.rotation, spawnPoint);
    }

    private GameObject GetRandomPrefab(GameObject[] pool)
    {
        if (pool == null || pool.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < pool.Length; i++)
        {
            int index = Random.Range(0, pool.Length);
            if (pool[index] != null)
            {
                return pool[index];
            }
        }

        return null;
    }

    private Transform FindSpawnPoint()
    {
        if (transform.name == "SpawnObjetos")
        {
            return transform;
        }

        Transform[] all = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            if (all[i].name == "SpawnObjetos")
            {
                return all[i];
            }
        }

        return null;
    }
}
