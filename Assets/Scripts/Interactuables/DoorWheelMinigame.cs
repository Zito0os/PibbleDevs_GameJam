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
    [SerializeField] private string mainCanvasName = "MAIN_CANVA";
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
    private ItemType requiredKey;
    private bool isRunning;
    private bool isSpinning;
    private int attemptsLeft;
    private float currentAngle;
    private bool previousCursorVisible;
    private CursorLockMode previousCursorLockState;
    // cached reference to the TMP that shows "Interactuar"
    private TextMeshProUGUI interactuarTMP = null;
    private bool? cachedInteractuarActive = null;

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

        if (Input.GetKeyDown(KeyCode.Space))
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

        BindReferences();

        currentDoor = door;
        currentPlayer = player;
        requiredKey = keyType;
        attemptsLeft = Mathf.Max(1, maxAttempts);

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

        // hide or restore the "Interactuar" TextMeshPro inside Interactuable-Widget
        if (blocked)
        {
            if (interactuarTMP == null)
            {
                GameObject widget = GameObject.Find("Interactuable-Widget");
                if (widget != null)
                {
                    TextMeshProUGUI[] tms = widget.GetComponentsInChildren<TextMeshProUGUI>(true);
                    foreach (var t in tms)
                    {
                        if (t != null && t.name == "Interactuar")
                        {
                            interactuarTMP = t;
                            break;
                        }
                    }
                }
            }

            if (interactuarTMP != null)
            {
                if (!cachedInteractuarActive.HasValue)
                    cachedInteractuarActive = interactuarTMP.gameObject.activeSelf;

                interactuarTMP.gameObject.SetActive(false);
            }
        }
        else
        {
            if (interactuarTMP != null && cachedInteractuarActive.HasValue)
            {
                interactuarTMP.gameObject.SetActive(cachedInteractuarActive.Value);
                cachedInteractuarActive = null;
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
        if (canvas == null)
        {
            GameObject mainCanvas = GameObject.Find(mainCanvasName);
            if (mainCanvas != null)
                canvas = mainCanvas.GetComponent<Canvas>();
        }

        if (wheelPanel == null)
        {
            Transform root = FindTransformByName(mainCanvasName, wheelPanelName);
            if (root != null)
                wheelPanel = root.gameObject;
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