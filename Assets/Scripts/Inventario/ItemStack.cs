using UnityEngine;

// Represents a stack of items with a type and quantity
[System.Serializable]
public class ItemStack
{
    public ItemType itemType;
    public int quantity;

    public ItemStack(ItemType type, int qty = 1)
    {
        itemType = type;
        quantity = Mathf.Max(1, qty);
    }

    public ItemStack(ItemType type) : this(type, 1) { }

    public override string ToString()
    {
        return $"{itemType} x{quantity}";
    }
}
