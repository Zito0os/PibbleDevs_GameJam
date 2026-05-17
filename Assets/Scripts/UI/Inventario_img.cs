using UnityEngine;
using UnityEngine.UI;

public class Inventario_img : MonoBehaviour
{
    [Header("Item Sprites")]
    public Sprite Key_gold;
    public Sprite Key_silver;
    public Sprite Freeze;
    public Sprite Lento;
    public Sprite Destroy;

    [Header("Inventory Slots")]
    public GameObject Slot_1;
    public GameObject Slot_2;
    public GameObject Slot_3;
    public GameObject Slot_4;
    public GameObject Slot_5;

    private Inventory inventory;
    private Image[] slotImages;

    private void Awake()
    {
        slotImages = new Image[5]
        {
            GetSlotImage(Slot_1),
            GetSlotImage(Slot_2),
            GetSlotImage(Slot_3),
            GetSlotImage(Slot_4),
            GetSlotImage(Slot_5)
        };

        RefreshInventoryReference();
    }

    private void OnEnable()
    {
        RefreshInventoryReference();
        SubscribeToInventory();
        RefreshSlots();
    }

    private void Start()
    {
        RefreshSlots();
    }

    private void OnDisable()
    {
        UnsubscribeFromInventory();
    }

    private void OnDestroy()
    {
        UnsubscribeFromInventory();
    }

    private void RefreshInventoryReference()
    {
        if (inventory != null)
            return;

        PlayerMovement playerMovement = GetComponentInParent<PlayerMovement>();
        if (playerMovement == null)
            playerMovement = FindFirstObjectByType<PlayerMovement>();

        if (playerMovement != null)
            inventory = playerMovement.GetComponent<Inventory>();
    }

    private void SubscribeToInventory()
    {
        if (inventory == null)
            return;

        inventory.OnInventoryChanged -= RefreshSlots;
        inventory.OnInventoryChanged += RefreshSlots;
    }

    private void UnsubscribeFromInventory()
    {
        if (inventory == null)
            return;

        inventory.OnInventoryChanged -= RefreshSlots;
    }

    private void RefreshSlots()
    {
        if (inventory == null)
        {
            ClearAllSlots();
            return;
        }

        ItemStack[] stacks = new ItemStack[5];
        int stackCount = inventory.ItemStacks.Count;

        for (int i = 0; i < stacks.Length; i++)
        {
            if (i < stackCount)
                stacks[i] = inventory.ItemStacks[i];
        }

        UpdateSlotImage(slotImages[0], stacks[0]);
        UpdateSlotImage(slotImages[1], stacks[1]);
        UpdateSlotImage(slotImages[2], stacks[2]);
        UpdateSlotImage(slotImages[3], stacks[3]);
        UpdateSlotImage(slotImages[4], stacks[4]);
    }

    private void ClearAllSlots()
    {
        for (int i = 0; i < slotImages.Length; i++)
        {
            if (slotImages[i] == null)
                continue;

            slotImages[i].sprite = null;
            slotImages[i].enabled = false;
        }
    }

    private void UpdateSlotImage(Image slotImage, ItemStack stack)
    {
        if (slotImage == null)
            return;

        Sprite sourceSprite = ResolveSourceSprite(stack != null ? stack.itemType : ItemType.None);

        if (sourceSprite == null || stack == null || stack.quantity <= 0)
        {
            slotImage.sprite = null;
            slotImage.enabled = false;
            return;
        }

        slotImage.sprite = sourceSprite;
        slotImage.enabled = true;
        slotImage.preserveAspect = true;
    }

    private Sprite ResolveSourceSprite(ItemType itemType)
    {
        return itemType switch
        {
            ItemType.KeyGold => Key_gold,
            ItemType.KeySilver => Key_silver,
            ItemType.SpellFreeze => Freeze,
            ItemType.SpellSlow => Lento,
            ItemType.SpellClear => Destroy,
            _ => null
        };
    }

    private Image GetSlotImage(GameObject slotObject)
    {
        if (slotObject == null)
            return null;

        return slotObject.GetComponent<Image>();
    }

    // Bind this UI element to a specific Inventory instance (per-player)
    public void BindToInventory(Inventory inv)
    {
        // Unsubscribe from previous inventory (if any)
        UnsubscribeFromInventory();

        inventory = inv;

        // Subscribe and refresh visuals for the new inventory
        SubscribeToInventory();
        RefreshSlots();
    }


}
