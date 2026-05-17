using UnityEngine;
using TMPro;

public class DoorWheelMinigame : MonoBehaviour
{
    private static DoorWheelMinigame instance;

    public static bool IsRunning => instance != null && instance.isRunning;

    [Header("Scene References")]
    [SerializeField] private GameObject wheelPanel;
    [SerializeField] private RectTransform spinningWheel;
    [SerializeField] private RectTransform metaZone;
    [SerializeField] private RectTransform needle;
    [SerializeField] private Canvas canvas;

    [Header("Auto Find")]
    [SerializeField] private string mainCanvasName = MultijugadorUIConstants.MainCanvasRootName;
    [SerializeField] private string wheelPanelName = "Rueda";
    [SerializeField] private string spinningWheelName = "Rueda";
    [SerializeField] private string metaName = "meta";
    [SerializeField] private string needleName = "Aguja";

    [Header("Gameplay")]
    [SerializeField] private int maxAttempts = 3;
    [SerializeField] private float spinSpeed = 240f;
    [SerializeField] private float restartMessageDuration = 0.35f;
    [SerializeField] private bool hideCursorWhileActive = false;
    [SerializeField] private float metaHitPadding = 24f;
    [SerializeField] private bool debugDoorLogs = false;

    private InventoryUI inventoryUI;
    private Door_Controller currentDoor;
    private PlayerMovement currentPlayer;
    private MultijugadorPlayerContext currentInputContext;
    private ItemType requiredKey;
    private bool isRunning;
    private bool isSpinning;
    private int attemptsLeft;
    private float currentAngle;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockState;
    private GameObject interactPromptObject = null;
    private bool? cachedInteractPromptActive = null;

    private void Awake()
    {
        if (instance == null)
            instance = this;

        BindReferences();
        SetPanelVisible(false);
    }

    private void OnDestroy()
    {
        if (instance == this)
            instance = null;
    }

    private void Update()
    {
        if (!isRunning)
            return;

        if (isSpinning && spinningWheel != null)
        {
            currentAngle -= spinSpeed * Time.deltaTime;
            spinningWheel.localRotation = Quaternion.Euler(0f, 0f, currentAngle);
        }

        bool stopPressed = currentInputContext != null ? currentInputContext.JumpPressedThisFrame : Input.GetKeyDown(KeyCode.Space);

        if (stopPressed)
        {
            StopAndEvaluate();
        }
    }

    public bool StartChallenge(Door_Controller door, PlayerMovement player, ItemType keyType)
    {
        if (door == null || player == null)
            return false;

        if (isRunning)
            return false;

        currentDoor = door;
        currentPlayer = player;
        currentInputContext = player != null ? player.GetComponent<MultijugadorPlayerContext>() : null;
        requiredKey = keyType;
        attemptsLeft = Mathf.Max(1, maxAttempts);

        ResetRuntimeReferences();

        BindReferences();

        if (!HasValidReferences())
        {
            Debug.LogWarning("DoorWheelMinigame: faltan referencias UI para iniciar la ruleta.");
            ShowMessage("Ruleta no configurada");
            currentDoor = null;
            currentPlayer = null;
            requiredKey = ItemType.None;
            return false;
        }

        isRunning = true;
        isSpinning = true;
        currentAngle = spinningWheel != null ? spinningWheel.localEulerAngles.z : 0f;

        CacheCursorState();
        SetPanelVisible(true);
        SetInputBlocked(true);

        ShowMessage("Pulsa SPACE para detener la rueda");
        return true;
    }

    private void StopAndEvaluate()
    {
        if (!isRunning || !isSpinning)
            return;

        isSpinning = false;

        if (IsNeedleInsideMeta())
        {
            CompleteSuccess();
            return;
        }

        attemptsLeft--;

        if (attemptsLeft <= 0)
        {
            CompleteFailure();
            return;
        }

        ShowMessage($"Fallaste. Intentos restantes: {attemptsLeft}");
        Invoke(nameof(RestartSpin), restartMessageDuration);
    }

    private void RestartSpin()
    {
        if (!isRunning)
            return;

        isSpinning = true;
        ShowMessage($"Intenta de nuevo. Intentos restantes: {attemptsLeft}");
    }

    private bool IsNeedleInsideMeta()
    {
        if (needle == null || metaZone == null)
            return false;

        Camera uiCamera = GetUICamera();
        Vector2 needleScreenPosition = RectTransformUtility.WorldToScreenPoint(uiCamera, needle.position);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(metaZone, needleScreenPosition, uiCamera, out Vector2 localPoint))
            return false;

        Rect metaRect = metaZone.rect;
        float halfWidth = metaRect.width * 0.5f + metaHitPadding;
        float halfHeight = metaRect.height * 0.5f + metaHitPadding;

        bool inside = Mathf.Abs(localPoint.x) <= halfWidth && Mathf.Abs(localPoint.y) <= halfHeight;

        if (debugDoorLogs)
        {
            Debug.Log("[Wheel] Check -> needleLocal=" + localPoint + " | metaHalfSize=" + new Vector2(halfWidth, halfHeight) + " | inside=" + inside);
        }

        return inside;
    }

    private void CompleteSuccess()
    {
        Door_Controller door = currentDoor;
        PlayerMovement player = currentPlayer;
        ItemType keyType = requiredKey;

        ShowMessage("Meta lograda");
        EndChallenge();
        if (door != null)
        {
            door.ResolveWheelSuccess(player, keyType);
        }
    }

    private void CompleteFailure()
    {
        Door_Controller door = currentDoor;
        PlayerMovement player = currentPlayer;
        ItemType keyType = requiredKey;

        ShowMessage("La llave se rompió");
        EndChallenge();
        if (door != null)
        {
            door.ResolveWheelFailure(player, keyType);
        }
    }

    private void EndChallenge()
    {
        CancelInvoke(nameof(RestartSpin));
        isRunning = false;
        isSpinning = false;
        SetPanelVisible(false);
        SetInputBlocked(false);

        currentDoor = null;
        currentPlayer = null;
        currentInputContext = null;
        requiredKey = ItemType.None;
    }

    private void SetPanelVisible(bool visible)
    {
        if (wheelPanel != null)
            wheelPanel.SetActive(visible);
    }

    private void SetInputBlocked(bool blocked)
    {
        if (currentPlayer != null)
        {
            PlayerMovement playerMovement = currentPlayer;
            if (playerMovement != null)
                playerMovement.enabled = !blocked;

            DisparoHechizo spellShooter = currentPlayer.GetComponentInChildren<DisparoHechizo>(true);
            if (spellShooter == null)
                spellShooter = currentPlayer.GetComponent<DisparoHechizo>();

            if (spellShooter != null)
                spellShooter.enabled = !blocked;

            Cameralook cameraLook = currentPlayer.GetComponentInChildren<Cameralook>(true);
            if (cameraLook == null)
                cameraLook = currentPlayer.GetComponent<Cameralook>();

            if (cameraLook != null)
                cameraLook.enabled = !blocked;
        }

        Selected selected = currentPlayer != null ? currentPlayer.GetComponentInChildren<Selected>(true) : null;
        if (selected == null && currentPlayer != null)
            selected = currentPlayer.GetComponent<Selected>();

        if (selected != null)
            selected.enabled = !blocked;

        MultijugadorPlayerHUD hud = currentPlayer != null ? currentPlayer.GetComponent<MultijugadorPlayerHUD>() : null;
        interactPromptObject = hud != null ? hud.interactPrompt : null;

        if (blocked)
        {
            if (interactPromptObject != null)
            {
                if (!cachedInteractPromptActive.HasValue)
                    cachedInteractPromptActive = interactPromptObject.activeSelf;

                interactPromptObject.SetActive(false);
            }
        }
        else
        {
            if (interactPromptObject != null && cachedInteractPromptActive.HasValue)
            {
                interactPromptObject.SetActive(cachedInteractPromptActive.Value);
                cachedInteractPromptActive = null;
            }
        }

        if (hideCursorWhileActive)
        {
            if (blocked)
            {
                previousCursorVisible = Cursor.visible;
                previousCursorLockState = Cursor.lockState;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                Cursor.visible = previousCursorVisible;
                Cursor.lockState = previousCursorLockState;
            }
        }
    }

    private void CacheCursorState()
    {
        previousCursorVisible = Cursor.visible;
        previousCursorLockState = Cursor.lockState;
    }

    private void ShowMessage(string message)
    {
        if (currentPlayer != null)
        {
            MultijugadorPlayerHUD hud = currentPlayer.GetComponent<MultijugadorPlayerHUD>();
            if (hud != null && hud.inventoryUI != null)
            {
                hud.inventoryUI.SetDoorMessage(message);
                return;
            }
        }

        if (inventoryUI == null)
        {
            inventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        if (inventoryUI != null)
        {
            inventoryUI.SetDoorMessage(message);
        }
        else
        {
            Debug.Log(message);
        }
    }

    private Camera GetUICamera()
    {
        if (canvas == null)
        {
            return null;
        }

        return canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : canvas.worldCamera;
    }

    private void BindReferences()
    {
        Canvas playerCanvas = null;

        if (currentPlayer != null)
        {
            MultijugadorPlayerHUD hud = currentPlayer.GetComponent<MultijugadorPlayerHUD>();
            if (hud != null && hud.hudCanvas != null)
            {
                canvas = hud.hudCanvas;
                playerCanvas = hud.hudCanvas;
                if (hud.inventoryUI != null)
                    inventoryUI = hud.inventoryUI;
            }
        }

        if (canvas == null)
        {
            GameObject mainCanvas = GameObject.Find(mainCanvasName);
            if (mainCanvas != null)
                canvas = mainCanvas.GetComponent<Canvas>();
        }

        if (wheelPanel == null)
        {
            if (playerCanvas != null)
            {
                Transform localRoot = FindTransformInChildren(playerCanvas.transform, wheelPanelName);
                if (localRoot != null)
                    wheelPanel = localRoot.gameObject;
            }

            if (wheelPanel == null)
            {
                Transform root = FindTransformByName(mainCanvasName, wheelPanelName);
                if (root != null)
                    wheelPanel = root.gameObject;
            }
        }

        if (wheelPanel == null && canvas != null)
        {
            Transform found = FindTransformInChildren(canvas.transform, wheelPanelName);
            if (found != null)
                wheelPanel = found.gameObject;
        }

        if (wheelPanel != null)
        {
            if (spinningWheel == null)
                spinningWheel = FindRectTransformInChildren(wheelPanel.transform, spinningWheelName, wheelPanel.transform);

            if (metaZone == null)
                metaZone = FindRectTransformInChildren(wheelPanel.transform, metaName, null);

            if (needle == null)
                needle = FindRectTransformInChildren(wheelPanel.transform, needleName, null);
        }

        if (spinningWheel == null && wheelPanel != null)
        {
            spinningWheel = wheelPanel.GetComponent<RectTransform>();
        }

        if (metaZone == null && spinningWheel != null)
        {
            metaZone = FindRectTransformInChildren(spinningWheel, metaName, null);
        }

        if (needle == null && wheelPanel != null)
        {
            needle = FindRectTransformInChildren(wheelPanel.transform, needleName, null);
        }
    }

    private bool HasValidReferences()
    {
        return wheelPanel != null
            && spinningWheel != null
            && metaZone != null
            && needle != null;
    }

    private void ResetRuntimeReferences()
    {
        canvas = null;
        wheelPanel = null;
        spinningWheel = null;
        metaZone = null;
        needle = null;
        inventoryUI = null;
    }

    private Transform FindTransformByName(string rootName, string targetName)
    {
        GameObject rootObject = GameObject.Find(rootName);
        if (rootObject == null)
            return null;

        return FindTransformInChildren(rootObject.transform, targetName);
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

    private RectTransform FindRectTransformInChildren(Transform root, string targetName, Transform exclude)
    {
        if (root == null)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child == exclude)
                continue;

            if (child.name == targetName)
                return child as RectTransform;
        }

        return null;
    }
}