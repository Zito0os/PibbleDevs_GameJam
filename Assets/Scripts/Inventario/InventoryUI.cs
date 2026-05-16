using UnityEngine;
using UnityEngine.UI;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    //Hacerlo compatible con TextMeshPro si es necesario    
    [SerializeField] private Text inventoryText;

    private Inventory inventory;

    private void Start()
    {
        if (playerMovement == null)
        {
            // Intentar encontrar al jugador
            //playerMovement = FindObjectOfType<PlayerMovement>();
        }

        if (playerMovement != null)
        {
            inventory = playerMovement.GetComponent<Inventory>();
        }

        if (inventory == null)
        {
            Debug.LogWarning("InventoryUI: no se encontró Inventory");
        }

        if (inventoryText == null)
        {
            Debug.LogWarning("InventoryUI: Text component no asignado. Buscando en hijos...");
            inventoryText = GetComponentInChildren<Text>();
        }
    }

    private void Update()
    {
        if (inventory != null && inventoryText != null)
        {
            inventoryText.text = inventory.GetDebugInfo();
        }
    }

    // Por si necesitas actualizar manualmente desde otro script
    public void UpdateUI()
    {
        if (inventory != null && inventoryText != null)
        {
            inventoryText.text = inventory.GetDebugInfo();
        }
    }
}
