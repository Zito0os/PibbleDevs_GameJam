using UnityEngine;
using System.Collections;

public class DisparoHechizo : MonoBehaviour
{
    public Transform HechizoSpawnPoint;
    public GameObject HechizoPrefab;
    [Header("Spell Prefabs")]
    public GameObject spellSlowPrefab;
    public GameObject spellFreezePrefab;
    public GameObject spellClearPrefab;
    public float HechizoSpeed = 10f;
    public float delayDisparo = 0.15f;
    private Inventory inventory;
    private MultijugadorPlayerContext playerInputContext;
    private PlayerMovement ownerPlayerMovement;
    private Coroutine shootRoutine;

    private void Start()
    {
        ownerPlayerMovement = GetComponentInParent<PlayerMovement>();
        if (ownerPlayerMovement == null)
            ownerPlayerMovement = GetComponent<PlayerMovement>();

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

        if (ownerPlayerMovement != null)
            ownerPlayerMovement.PlaySpellCastAnimation();

        if (shootRoutine != null)
            StopCoroutine(shootRoutine);

        ItemType itemTypeToShoot = inventory.activeItemType;
        shootRoutine = StartCoroutine(ShootSpellAfterDelay(itemTypeToShoot));
    }

    private IEnumerator ShootSpellAfterDelay(ItemType itemType)
    {
        if (delayDisparo > 0f)
            yield return new WaitForSeconds(delayDisparo);

        if (inventory == null)
        {
            shootRoutine = null;
            yield break;
        }

        if (!inventory.UseSelectedItem())
        {
            shootRoutine = null;
            yield break;
        }

        GameObject prefab = ResolveSpellPrefab(itemType);
        if (prefab == null)
        {
            shootRoutine = null;
            yield break;
        }

        Transform spawn = HechizoSpawnPoint != null ? HechizoSpawnPoint : transform;
        var hechizo = Instantiate(prefab, spawn.position, spawn.rotation);
        SoundManager.PlaySound(SoundType.AttackPrincipal);
        Rigidbody rb = hechizo.GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.linearVelocity = spawn.forward * HechizoSpeed;
        }

        SpellProjectile projectile = hechizo.GetComponent<SpellProjectile>();
        if (projectile != null)
        {
            projectile.SetOwner(transform);
            projectile.ConfigureFromItem(itemType);
        }

        shootRoutine = null;
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

