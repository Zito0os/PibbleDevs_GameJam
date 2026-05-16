using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int maxSlots = 5;  // máximo de tipos distintos de items
    [SerializeField] public int activeSlotIndex = 0;
    [SerializeField] public bool hasActiveItem = false;
    [SerializeField] public ItemType activeItemType;
    [SerializeField] public string activeItemName = "None";
    
    private List<ItemStack> itemStacks = new List<ItemStack>();

    public event Action OnInventoryChanged;
    public event Action OnInventoryFull;

    public IReadOnlyList<ItemStack> ItemStacks => itemStacks;
    public int MaxSlots => maxSlots;
    public bool IsFull => itemStacks.Count >= maxSlots;
    public ItemStack ActiveItem => GetActiveItemStack();

    public bool CanAddItem(ItemType itemType, int quantity = 1)
    {
        if (quantity <= 0)
            return false;

        ItemStack existingStack = itemStacks.FirstOrDefault(s => s.itemType == itemType);
        if (existingStack != null)
            return true;

        return itemStacks.Count < maxSlots;
    }

    // Añade un item. Si ya existe ese tipo, aumenta la cantidad
    public bool AddItem(ItemType itemType, int quantity = 1)
    {
        if (quantity <= 0)
            return false;

        // Buscar si ya existe ese tipo de item
        ItemStack existingStack = itemStacks.FirstOrDefault(s => s.itemType == itemType);
        
        if (existingStack != null)
        {
            // Aumentar cantidad del stack existente
            existingStack.quantity += quantity;
            Debug.Log($"Inventory: {itemType} x{quantity} añadido. Total: {existingStack}");
            UpdateActiveSelectionState();
            OnInventoryChanged?.Invoke();
            return true;
        }

        // Si no existe y hay espacio, crear nuevo stack
        if (itemStacks.Count >= maxSlots)
        {
            Debug.LogWarning($"Inventory: no hay espacio para más tipos de items. Max slots: {maxSlots}");
            OnInventoryFull?.Invoke();
            return false;
        }

        ItemStack newStack = new ItemStack(itemType, quantity);
        itemStacks.Add(newStack);
        Debug.Log($"Inventory: {itemType} x{quantity} añadido. Total stacks: {itemStacks.Count}/{maxSlots}");
        UpdateActiveSelectionState();
        OnInventoryChanged?.Invoke();
        return true;
    }

    // Quitar cantidad de un item
    public bool RemoveItem(ItemType itemType, int quantity = 1)
    {
        if (quantity <= 0)
            return false;

        ItemStack stack = itemStacks.FirstOrDefault(s => s.itemType == itemType);
        if (stack == null)
        {
            Debug.LogWarning($"Inventory: no hay {itemType}");
            return false;
        }

        if (stack.quantity < quantity)
        {
            Debug.LogWarning($"Inventory: no hay suficiente {itemType}. Disponible: {stack.quantity}, solicitado: {quantity}");
            return false;
        }

        stack.quantity -= quantity;
        Debug.Log($"Inventory: {itemType} x{quantity} removido. Quedan: {stack.quantity}");

        if (stack.quantity <= 0)
        {
            itemStacks.Remove(stack);
            Debug.Log($"Inventory: {itemType} removido completamente.");
        }

        NormalizeActiveSlotIndex();
        UpdateActiveSelectionState();
        OnInventoryChanged?.Invoke();
        return true;
    }

    public bool UseFirstItem()
    {
        return UseItemAt(0);
    }

    public bool UseSelectedItem()
    {
        return UseItemAt(activeSlotIndex);
    }

    public void SetActiveSlot(int slotIndex)
    {
        activeSlotIndex = Mathf.Clamp(slotIndex, 0, maxSlots - 1);
        UpdateActiveSelectionState();
        OnInventoryChanged?.Invoke();
    }

    public bool UseItemAt(int index)
    {
        if (index < 0 || index >= itemStacks.Count)
            return false;

        ItemStack stack = itemStacks[index];
        if (stack == null)
            return false;

        stack.quantity -= 1;
        Debug.Log($"Inventory: usando {stack.itemType}. Quedan: {Mathf.Max(stack.quantity, 0)}");

        if (stack.quantity <= 0)
        {
            itemStacks.RemoveAt(index);
            Debug.Log($"Inventory: {stack.itemType} eliminado del slot {index + 1}.");
        }

        NormalizeActiveSlotIndex();
        UpdateActiveSelectionState();
        OnInventoryChanged?.Invoke();
        return true;
    }

    private ItemStack GetActiveItemStack()
    {
        if (activeSlotIndex < 0 || activeSlotIndex >= itemStacks.Count)
            return null;

        return itemStacks[activeSlotIndex];
    }

    private void NormalizeActiveSlotIndex()
    {
        if (itemStacks.Count == 0)
        {
            activeSlotIndex = 0;
            return;
        }

        activeSlotIndex = Mathf.Clamp(activeSlotIndex, 0, Mathf.Min(itemStacks.Count - 1, maxSlots - 1));
    }

    private void UpdateActiveSelectionState()
    {
        ItemStack stack = GetActiveItemStack();
        hasActiveItem = stack != null;
        activeItemType = stack != null ? stack.itemType : ItemType.None;
        activeItemName = stack != null ? stack.itemType.ToString() : "None";
    }

    // Verificar si tenemos cantidad mínima de un item
    public bool HasItem(ItemType itemType, int minQuantity = 1)
    {
        ItemStack stack = itemStacks.FirstOrDefault(s => s.itemType == itemType);
        return stack != null && stack.quantity >= minQuantity;
    }

    // Obtener cantidad de un item
    public int GetQuantity(ItemType itemType)
    {
        ItemStack stack = itemStacks.FirstOrDefault(s => s.itemType == itemType);
        return stack != null ? stack.quantity : 0;
    }

    // Vaciar inventario
    public void Clear()
    {
        itemStacks.Clear();
        Debug.Log("Inventory: vaciado completamente.");
        activeSlotIndex = 0;
        UpdateActiveSelectionState();
        OnInventoryChanged?.Invoke();
    }

    // Obtener string de debug para mostrar inventario
    public string GetDebugInfo()
    {
        if (itemStacks.Count == 0)
            return "Inventory: vacío";

        string info = "Inventory:\n";
        foreach (var stack in itemStacks)
        {
            info += $"  {stack}\n";
        }
        return info;
    }
}
