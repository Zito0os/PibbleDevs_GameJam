using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections;

public class InventoryUI : MonoBehaviour
{
    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private Transform inventoryRoot;
    [SerializeField] private TMP_Text[] slotTexts = new TMP_Text[5];
    [SerializeField] private Transform[] slotRoots = new Transform[5];
    public Vector3 selectedSlotScale = new Vector3(1.2f, 1.2f, 1f);
    [SerializeField] private TMP_Text inventoryMessageText;
    [SerializeField] private TMP_Text puertaMessageText;
    [SerializeField] private float inventoryMessageDuration = 1.5f;
    [SerializeField] private float puertaMessageDuration = 1.5f;

    private Inventory inventory;
    private Coroutine clearMessageRoutine;
    private Coroutine clearDoorMessageRoutine;
    private Vector3[] baseSlotScales = new Vector3[5];
    private bool isBoundToInventory = false;

    private void Awake()
    {
        if (playerMovement == null)
        {
            playerMovement = FindFirstObjectByType<PlayerMovement>();
        }

        if (playerMovement != null)
        {
            inventory = playerMovement.GetComponent<Inventory>();
        }

        if (inventoryRoot == null)
        {
            GameObject foundRoot = GameObject.Find("Inventario");
            inventoryRoot = foundRoot != null ? foundRoot.transform : transform;
        }

        BindSlotTexts();
        BindSlotRoots();
        BindMessageText();
        BindDoorMessageText();
    }

    private void Start()
    {
        SubscribeToInventory();

        RefreshUI();
    }

    private void OnDestroy()
    {
        UnsubscribeFromInventory();
    }

    public void ConfigureForPlayer(PlayerMovement targetPlayer, Transform rootOverride = null)
    {
        UnsubscribeFromInventory();

        playerMovement = targetPlayer;
        inventory = playerMovement != null ? playerMovement.GetComponent<Inventory>() : null;

        if (rootOverride != null)
        {
            inventoryRoot = rootOverride;
        }
        else if (inventoryRoot == null)
        {
            inventoryRoot = transform;
        }

        slotTexts = new TMP_Text[5];
        slotRoots = new Transform[5];
        baseSlotScales = new Vector3[5];
        inventoryMessageText = null;
        puertaMessageText = null;

        BindSlotTexts();
        BindSlotRoots();
        BindMessageText();
        BindDoorMessageText();

        SubscribeToInventory();
        // Ensure any Inventario_img UI under this inventory root is bound to the correct Inventory
        if (inventory != null && inventoryRoot != null)
        {
            Inventario_img[] invImgs = inventoryRoot.GetComponentsInChildren<Inventario_img>(true);
            foreach (Inventario_img img in invImgs)
            {
                if (img != null)
                    img.BindToInventory(inventory);
            }
        }

        RefreshUI();
    }

    private void SubscribeToInventory()
    {
        if (inventory == null)
        {
            Debug.LogWarning("InventoryUI: no se encontró Inventory");
            return;
        }

        if (isBoundToInventory)
            return;

        inventory.OnInventoryChanged += RefreshUI;
        inventory.OnInventoryFull += ShowInventoryFullMessage;
        isBoundToInventory = true;
    }

    private void UnsubscribeFromInventory()
    {
        if (inventory != null && isBoundToInventory)
        {
            inventory.OnInventoryChanged -= RefreshUI;
            inventory.OnInventoryFull -= ShowInventoryFullMessage;
        }

        isBoundToInventory = false;
    }

    private void BindSlotTexts()
    {
        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (slotTexts[i] != null)
                continue;

            string slotName = $"Slot_{i + 1}_text";
            slotTexts[i] = FindTextInChildren(inventoryRoot, slotName);
        }
    }

    private void BindSlotRoots()
    {
        for (int i = 0; i < slotRoots.Length; i++)
        {
            if (slotRoots[i] != null)
            {
                if (baseSlotScales[i] == Vector3.zero)
                    baseSlotScales[i] = slotRoots[i].localScale;

                continue;
            }

            string slotName = $"Slot_{i + 1}";
            slotRoots[i] = FindTransformInChildren(inventoryRoot, slotName);

            if (slotRoots[i] != null && baseSlotScales[i] == Vector3.zero)
            {
                baseSlotScales[i] = slotRoots[i].localScale;
            }
        }
    }

    private void BindMessageText()
    {
        if (inventoryMessageText != null)
            return;

        inventoryMessageText = FindTextByAnyName(inventoryRoot,
            "Inventario_text",
            "inventario_txt",
            "InventoryMessage_text",
            "Status_text");
    }

    private void BindDoorMessageText()
    {
        if (puertaMessageText != null)
            return;

        puertaMessageText = FindTextByAnyName(inventoryRoot,
            "Puerta_txt",
            "puerta_txt");
    }

    private TMP_Text FindTextByAnyName(Transform root, params string[] targetNames)
    {
        if (root == null || targetNames == null)
            return null;

        foreach (string targetName in targetNames)
        {
            TMP_Text foundText = FindTextInChildren(root, targetName);
            if (foundText != null)
                return foundText;
        }

        return null;
    }

    private TMP_Text FindTextInChildren(Transform root, string targetName)
    {
        if (root == null)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == targetName)
                return child.GetComponent<TMP_Text>();
        }

        return null;
    }

    private Transform FindTransformInChildren(Transform root, string targetName)
    {
        if (root == null)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == targetName)
                return child;
        }

        return null;
    }

    private void RefreshUI()
    {
        if (inventory == null)
            return;

        for (int i = 0; i < slotTexts.Length; i++)
        {
            if (slotTexts[i] == null)
                continue;

            if (i < inventory.ItemStacks.Count)
            {
                ItemStack stack = inventory.ItemStacks[i];
                slotTexts[i].text = $"{stack.itemType} x{stack.quantity}";
                slotTexts[i].color = i == inventory.activeSlotIndex ? Color.red : Color.black;
            }
            else
            {
                slotTexts[i].text = string.Empty;
                slotTexts[i].color = Color.black;
            }
        }

        UpdateSlotScales();

        if (inventoryMessageText != null && inventory.ItemStacks.Count < inventory.MaxSlots)
        {
            inventoryMessageText.text = string.Empty;
        }
    }

    private void UpdateSlotScales()
    {
        for (int i = 0; i < slotRoots.Length; i++)
        {
            if (slotRoots[i] == null)
                continue;

            if (baseSlotScales[i] == Vector3.zero)
            {
                baseSlotScales[i] = slotRoots[i].localScale;
            }

            Vector3 baseScale = baseSlotScales[i];
            slotRoots[i].localScale = inventory.activeSlotIndex >= 0 && i == inventory.activeSlotIndex
                ? Vector3.Scale(baseScale, selectedSlotScale)
                : baseScale;
        }
    }

    private void ShowInventoryFullMessage()
    {
        if (inventoryMessageText == null)
        {
            Debug.LogWarning("InventoryUI: inventario lleno, pero no se encontró un TMP_Text para mostrar el mensaje.");
            return;
        }

        if (clearMessageRoutine != null)
        {
            StopCoroutine(clearMessageRoutine);
        }

        inventoryMessageText.text = "Inventario lleno";
        clearMessageRoutine = StartCoroutine(ClearInventoryMessage());
    }

    public void SetDoorMessage(string message)
    {
        if (puertaMessageText == null)
        {
            Debug.LogWarning("InventoryUI: no se encontró Puerta_txt para mostrar mensajes de puerta.");
            return;
        }

        if (clearDoorMessageRoutine != null)
        {
            StopCoroutine(clearDoorMessageRoutine);
        }

        puertaMessageText.text = message;
        clearDoorMessageRoutine = StartCoroutine(ClearDoorMessage());
    }

    private IEnumerator ClearDoorMessage()
    {
        yield return new WaitForSeconds(puertaMessageDuration);
        if (puertaMessageText != null)
            puertaMessageText.text = string.Empty;
    }

    private IEnumerator ClearInventoryMessage()
    {
        yield return new WaitForSeconds(inventoryMessageDuration);
        if (inventoryMessageText != null)
            inventoryMessageText.text = string.Empty;
    }

    public void UpdateUI()
    {
        RefreshUI();
    }
}
