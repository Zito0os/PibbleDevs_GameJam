using TMPro;
using UnityEngine;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MultijugadorPlayerHUD : MonoBehaviour
{
    public Canvas hudCanvas;
    public InventoryUI inventoryUI;
    public StaminaBar staminaBar;
    public RawImage minimapImage;
    public Camera minimapCamera;
    public GameObject interactPrompt;

    public void Bind(Canvas canvas, InventoryUI inventory, StaminaBar stamina, RawImage miniMap, Camera miniMapCamera, GameObject interactPromptObject)
    {
        hudCanvas = canvas;
        inventoryUI = inventory;
        staminaBar = stamina;
        minimapImage = miniMap;
        minimapCamera = miniMapCamera;
        interactPrompt = interactPromptObject;
    }
}