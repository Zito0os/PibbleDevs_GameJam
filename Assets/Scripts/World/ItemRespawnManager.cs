using UnityEngine;
using System.Collections.Generic;

public class ItemRespawnManager : MonoBehaviour
{
    public static ItemRespawnManager Instance { get; private set; }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public bool RespawnItem(ItemType itemType, int quantity = 1)
    {
        if (quantity <= 0)
            return false;

        bool spawnedAny = false;
        for (int i = 0; i < quantity; i++)
        {
            bool spawned = RespawnSingle(itemType);
            spawnedAny = spawnedAny || spawned;
        }

        return spawnedAny;
    }

    private bool RespawnSingle(ItemType itemType)
    {
        ItemSpawner[] spawners = FindObjectsOfType<ItemSpawner>(true);
        if (spawners == null || spawners.Length == 0)
        {
            Debug.LogWarning("ItemRespawnManager: no se encontraron ItemSpawner en la escena.");
            return false;
        }

        List<ItemSpawner> candidates = new List<ItemSpawner>();
        for (int i = 0; i < spawners.Length; i++)
        {
            ItemSpawner spawner = spawners[i];
            if (spawner == null)
                continue;

            if (!spawner.IsEmpty)
                continue;

            if (!spawner.CanSpawnItem(itemType))
                continue;

            candidates.Add(spawner);
        }

        if (candidates.Count == 0)
        {
            Debug.LogWarning($"ItemRespawnManager: no hay cofres disponibles para {itemType}.");
            return false;
        }

        int index = Random.Range(0, candidates.Count);
        return candidates[index].TrySpawnItem(itemType);
    }
}
