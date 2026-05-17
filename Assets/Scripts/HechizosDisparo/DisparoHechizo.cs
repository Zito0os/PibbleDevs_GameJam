using UnityEngine;

public class DisparoHechizo : MonoBehaviour
{
    public Transform HechizoSpawnPoint;
    public GameObject HechizoPrefab;
    [Header("Spell Prefabs")]
    public GameObject spellSlowPrefab;
    public GameObject spellFreezePrefab;
    public GameObject spellClearPrefab;
    public float HechizoSpeed = 10f;
    private Inventory inventory;
    private MultijugadorPlayerContext playerInputContext;

    private void Start()
    {
        inventory = GetComponentInParent<Inventory>();
        if (inventory == null)
            inventory = GetComponent<Inventory>();

        RefreshInputContext();
    }

    public void SetInputContext(MultijugadorPlayerContext inputContext)
    {
        playerInputContext = inputContext;
    }

    private void RefreshInputContext()
    {
        if (playerInputContext != null)
            return;

        playerInputContext = GetComponentInParent<MultijugadorPlayerContext>();
        if (playerInputContext == null)
            playerInputContext = GetComponent<MultijugadorPlayerContext>();
    }

    private void Update()
    {
        RefreshInputContext();

        bool attackPressed = playerInputContext != null ? playerInputContext.AttackPressedThisFrame : false;
        if (!attackPressed)
            return;

        if (inventory == null || !EsHechizoInventario(inventory.activeItemType))
            return;

        //Disparar con el botón de ataque del jugador
        GameObject prefab = ResolveSpellPrefab(inventory.activeItemType);
        if (prefab == null)
            return;

        if (!inventory.UseSelectedItem())
            return;

        Transform spawn = HechizoSpawnPoint != null ? HechizoSpawnPoint : transform;
        var hechizo = Instantiate(prefab, spawn.position, spawn.rotation);
        Rigidbody rb = hechizo.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = spawn.forward * HechizoSpeed;
        }

        SpellProjectile projectile = hechizo.GetComponent<SpellProjectile>();
        if (projectile != null)
        {
            projectile.SetOwner(transform);
            projectile.ConfigureFromItem(inventory.activeItemType);
        }
    }

    private bool EsHechizoInventario(ItemType itemType)
    {
        return itemType == ItemType.SpellSlow
            || itemType == ItemType.SpellFreeze
            || itemType == ItemType.SpellClear;
    }

    private GameObject ResolveSpellPrefab(ItemType itemType)
    {
        switch (itemType)
        {
            case ItemType.SpellSlow:
                return spellSlowPrefab != null ? spellSlowPrefab : HechizoPrefab;
            case ItemType.SpellFreeze:
                return spellFreezePrefab != null ? spellFreezePrefab : HechizoPrefab;
            case ItemType.SpellClear:
                return spellClearPrefab != null ? spellClearPrefab : HechizoPrefab;
            default:
                return null;
        }
    }
}

