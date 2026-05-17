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
    public GameObject llaveEncontradaUI;
    public Image llaveEncontradaImage;
    public Image llaveEncontradaChildImage;

    public void Bind(Canvas canvas, InventoryUI inventory, StaminaBar stamina, RawImage miniMap, Camera miniMapCamera, GameObject interactPromptObject, GameObject llaveEncontradaObject)
    {
        hudCanvas = canvas;
        inventoryUI = inventory;
        staminaBar = stamina;
        minimapImage = miniMap;
        minimapCamera = miniMapCamera;
        interactPrompt = interactPromptObject;
        llaveEncontradaUI = llaveEncontradaObject;

        CacheLlaveEncontradaImages();
        SetLlaveEncontradaVisible(false);
    }

    private void CacheLlaveEncontradaImages()
    {
        llaveEncontradaImage = null;
        llaveEncontradaChildImage = null;

        if (llaveEncontradaUI == null)
            return;

        llaveEncontradaImage = llaveEncontradaUI.GetComponent<Image>();

        Transform child = llaveEncontradaUI.transform.Find("Llave");
        if (child != null)
        {
            llaveEncontradaChildImage = child.GetComponent<Image>();
        }
    }

    public void SetLlaveEncontradaVisible(bool visible)
    {
        if (llaveEncontradaImage != null)
            llaveEncontradaImage.enabled = visible;

        if (llaveEncontradaChildImage != null)
            llaveEncontradaChildImage.enabled = visible;
    }
}