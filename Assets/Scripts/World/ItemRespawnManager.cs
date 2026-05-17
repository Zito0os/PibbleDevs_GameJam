using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ItemRespawnManager : MonoBehaviour
{
    public static ItemRespawnManager Instance { get; private set; }

    [Header("Guaranteed Keys")]
    [SerializeField] private int requiredGoldKeysInRare = 1;
    [SerializeField] private int requiredSilverKeysInNormal = 2;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    private void Start()
    {
        StartCoroutine(EnsureGuaranteedKeysNextFrame());
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
        ItemSpawner[] spawners = FindObjectsByType<ItemSpawner>(FindObjectsSortMode.None);
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

    private IEnumerator EnsureGuaranteedKeysNextFrame()
    {
        yield return null;

        EnsureGuaranteedKeys();
    }

    private void EnsureGuaranteedKeys()
    {
        ItemSpawner[] spawners = FindObjectsByType<ItemSpawner>(FindObjectsSortMode.None);
        if (spawners == null || spawners.Length == 0)
            return;

        List<ItemSpawner> rareEmptyCandidates = new List<ItemSpawner>();
        List<ItemSpawner> rareReplaceCandidates = new List<ItemSpawner>();
        List<ItemSpawner> normalEmptyCandidates = new List<ItemSpawner>();
        List<ItemSpawner> normalReplaceCandidates = new List<ItemSpawner>();
        int goldKeysInRare = 0;
        int silverKeysInNormal = 0;

        for (int i = 0; i < spawners.Length; i++)
        {
            ItemSpawner spawner = spawners[i];
            if (spawner == null)
                continue;

            ItemType? existing = GetSpawnedItemType(spawner);

            if (spawner.cofreRaro)
            {
                if (existing == ItemType.KeyGold)
                    goldKeysInRare++;
                else if (spawner.CanSpawnItem(ItemType.KeyGold))
                {
                    if (spawner.IsEmpty)
                        rareEmptyCandidates.Add(spawner);
                    else
                        rareReplaceCandidates.Add(spawner);
                }
            }
            else
            {
                if (existing == ItemType.KeySilver)
                    silverKeysInNormal++;
                else if (spawner.CanSpawnItem(ItemType.KeySilver))
                {
                    if (spawner.IsEmpty)
                        normalEmptyCandidates.Add(spawner);
                    else
                        normalReplaceCandidates.Add(spawner);
                }
            }
        }

        TryForceSpawn(ItemType.KeyGold, requiredGoldKeysInRare, goldKeysInRare, rareEmptyCandidates, rareReplaceCandidates);
        TryForceSpawn(ItemType.KeySilver, requiredSilverKeysInNormal, silverKeysInNormal, normalEmptyCandidates, normalReplaceCandidates);
    }

    private ItemType? GetSpawnedItemType(ItemSpawner spawner)
    {
        if (spawner == null)
            return null;

        Transform spawnPoint = spawner.spawnPoint != null ? spawner.spawnPoint : spawner.transform.Find("SpawnObjetos");
        if (spawnPoint == null || spawnPoint.childCount == 0)
            return null;

        ItemPickup pickup = spawnPoint.GetComponentInChildren<ItemPickup>(true);
        if (pickup == null)
            return null;

        return pickup.GetItemType();
    }

    private void TryForceSpawn(ItemType itemType, int required, int current, List<ItemSpawner> emptyCandidates, List<ItemSpawner> replaceCandidates)
    {
        int missing = Mathf.Max(required - current, 0);
        if (missing == 0)
            return;

        for (int i = 0; i < missing; i++)
        {
            ItemSpawner spawner = null;
            bool useReplace = false;

            if (emptyCandidates.Count > 0)
            {
                int index = Random.Range(0, emptyCandidates.Count);
                spawner = emptyCandidates[index];
                emptyCandidates.RemoveAt(index);
            }
            else if (replaceCandidates.Count > 0)
            {
                int index = Random.Range(0, replaceCandidates.Count);
                spawner = replaceCandidates[index];
                replaceCandidates.RemoveAt(index);
                useReplace = true;
            }
            else
            {
                Debug.LogWarning($"ItemRespawnManager: no hay cofres disponibles para asegurar {itemType}.");
                break;
            }

            if (spawner == null)
                continue;

            if (useReplace)
                spawner.ForceSpawnItem(itemType);
            else
                spawner.TrySpawnItem(itemType);
        }
    }
}
