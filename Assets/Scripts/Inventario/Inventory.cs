using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class Inventory : MonoBehaviour
{
    [SerializeField] private int maxSlots = 5;  // máximo de tipos distintos de items
    
    private List<ItemStack> itemStacks = new List<ItemStack>();

    public IReadOnlyList<ItemStack> ItemStacks => itemStacks;
    public int MaxSlots => maxSlots;

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
            return true;
        }

        // Si no existe y hay espacio, crear nuevo stack
        if (itemStacks.Count >= maxSlots)
        {
            Debug.LogWarning($"Inventory: no hay espacio para más tipos de items. Max slots: {maxSlots}");
            return false;
        }

        ItemStack newStack = new ItemStack(itemType, quantity);
        itemStacks.Add(newStack);
        Debug.Log($"Inventory: {itemType} x{quantity} añadido. Total stacks: {itemStacks.Count}/{maxSlots}");
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

        return true;
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
