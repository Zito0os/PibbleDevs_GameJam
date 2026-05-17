using UnityEngine;

public class DisparoHechizo : MonoBehaviour
{
    public Transform HechizoSpawnPoint;
    public GameObject HechizoPrefab;
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
        if (HechizoPrefab != null && HechizoSpawnPoint != null)
        {
            var hechizo = Instantiate(HechizoPrefab, HechizoSpawnPoint.position, HechizoSpawnPoint.rotation);
            hechizo.GetComponent<Rigidbody>().linearVelocity = HechizoSpawnPoint.forward * HechizoSpeed;

        }
    }

    private bool EsHechizoInventario(ItemType itemType)
    {
        return itemType == ItemType.SpellSlow
            || itemType == ItemType.SpellFreeze
            || itemType == ItemType.SpellClear;
    }
}

