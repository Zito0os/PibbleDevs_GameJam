using UnityEngine;

public class ItemPickup : MonoBehaviour
{
    [Header("Item")]
    [SerializeField] private ItemType itemType;
    [SerializeField] private int quantity = 1;

    private bool isPickedUp = false;

    private void OnEnable()
    {
        // Asegurar que tenga collider para raycast
        Collider col = GetComponent<Collider>();
        if (col == null)
        {
            col = gameObject.AddComponent<BoxCollider>();
        }

        // Dar tag para que Selected.cs lo detecte
        if (!gameObject.CompareTag("Item"))
        {
            gameObject.tag = "Item";
        }

        // Ponerlo en layer RayCastDetect si existe
        int layer = LayerMask.NameToLayer("RayCastDetect");
        if (layer != -1)
        {
            gameObject.layer = layer;
        }

        isPickedUp = false;
    }

    public void PickUp(PlayerMovement player)
    {
        if (isPickedUp)
            return;

        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("ItemPickup: el jugador no tiene Inventory");
            return;
        }

        bool added = inventory.AddItem(itemType, quantity);
        if (added)
        {
            isPickedUp = true;
            Debug.Log($"ItemPickup: {itemType} x{quantity} recogido.");
            ChestController chest = GetComponentInParent<ChestController>();
            if (chest != null)
            {
                chest.ItemTaken();
            }
            // Opcionalmente, destruir o desactivar el item
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"ItemPickup: no se pudo añadir {itemType} al inventario.");
        }
    }

    public ItemType GetItemType() => itemType;
    public int GetQuantity() => quantity;
}
