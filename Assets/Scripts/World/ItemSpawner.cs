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

    [Header("Spell Rotation")]
    public bool rotateSpellItemsOnSpawn = true;
    public Vector3 spellSpawnEulerOffset = new Vector3(0f, 0f, 90f);

    public bool IsEmpty
    {
        get
        {
            EnsureSpawnPoint();
            return spawnPoint != null && spawnPoint.childCount == 0;
        }
    }

    private void Start()
    {
        EnsureSpawnPoint();

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

        SpawnPrefab(prefabToSpawn, GetSpawnRotation(prefabToSpawn));
    }

    public bool CanSpawnItem(ItemType itemType)
    {
        return FindPrefabForItem(itemType) != null;
    }

    public bool TrySpawnItem(ItemType itemType)
    {
        EnsureSpawnPoint();
        if (spawnPoint == null || spawnPoint.childCount > 0)
            return false;

        GameObject prefab = FindPrefabForItem(itemType);
        if (prefab == null)
            return false;

        SpawnPrefab(prefab, GetSpawnRotation(prefab));
        return true;
    }

    private void SpawnPrefab(GameObject prefab, Quaternion rotation)
    {
        if (prefab == null)
            return;

        EnsureSpawnPoint();
        if (spawnPoint == null)
            return;

        Instantiate(prefab, spawnPoint.position, rotation, spawnPoint);
    }

    private Quaternion GetSpawnRotation(GameObject prefab)
    {
        Quaternion baseRotation = spawnPoint != null ? spawnPoint.rotation : transform.rotation;

        if (!rotateSpellItemsOnSpawn || prefab == null)
            return baseRotation;

        ItemPickup pickup = prefab.GetComponent<ItemPickup>();
        if (pickup == null || !IsSpellItem(pickup.GetItemType()))
            return baseRotation;

        return baseRotation * Quaternion.Euler(spellSpawnEulerOffset);
    }

    private bool IsSpellItem(ItemType itemType)
    {
        return itemType == ItemType.SpellSlow
            || itemType == ItemType.SpellFreeze
            || itemType == ItemType.SpellClear;
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

    private GameObject FindPrefabForItem(ItemType itemType)
    {
        GameObject[] pool = cofreRaro ? rareLootPrefabs : normalLootPrefabs;
        if (pool == null || pool.Length == 0)
        {
            return null;
        }

        for (int i = 0; i < pool.Length; i++)
        {
            GameObject prefab = pool[i];
            if (prefab == null)
                continue;

            ItemPickup pickup = prefab.GetComponent<ItemPickup>();
            if (pickup != null && pickup.GetItemType() == itemType)
            {
                return prefab;
            }
        }

        return null;
    }

    private void EnsureSpawnPoint()
    {
        if (spawnPoint == null)
        {
            spawnPoint = FindSpawnPoint();
        }
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
