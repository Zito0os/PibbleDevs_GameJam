using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class MultijugadorManager : MonoBehaviour
{
    private class JoinedPlayer
    {
        public GameObject playerObject;
        public MultijugadorPlayerContext context;
        public MultijugadorPlayerContext.ControlMode mode;
        public Gamepad gamepad;
    }

    [Header("Prefab & Input")]
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private InputActionAsset inputActions;

    [Header("Spawn")]
    [SerializeField] private Transform playerOneSpawnPoint;
    [SerializeField] private Transform playerTwoSpawnPoint;
    [SerializeField] private Vector3 spawnOffset = new Vector3(2f, 0f, 0f);

    [Header("Split Screen")]
    [SerializeField] private bool splitHorizontally = false;

    [Header("HUD")]
    [SerializeField] private GameObject mainCanvasTemplate;

    [Header("Debug")]
    [SerializeField] private bool debugLogs = true;

    private readonly List<JoinedPlayer> joinedPlayers = new List<JoinedPlayer>(2);
    private bool matchStarted = false;
    private GameObject reservedScenePlayer;
    private bool reservedScenePlayerClaimed = false;
    private GameObject playerTwoHudInstance;

    private void Awake()
    {
        if (playerPrefab == null)
        {
            Debug.LogWarning("MultijugadorManager: asigna el prefab Player.prefab en el inspector.");
        }

        if (mainCanvasTemplate == null)
        {
            mainCanvasTemplate = GameObject.Find(MultijugadorUIConstants.MainCanvasRootName);
        }

        PlayerMovement scenePlayer = FindFirstObjectByType<PlayerMovement>();
        if (scenePlayer != null)
        {
            reservedScenePlayer = scenePlayer.gameObject;
            SetPlayerGameplayEnabled(reservedScenePlayer, false);
        }
    }

    private void Update()
    {
        if (matchStarted)
            return;

        if (joinedPlayers.Count < 2)
        {
            TryJoinKeyboardMouse();
            TryJoinGamepad();
        }

        if (joinedPlayers.Count >= 2)
            StartMatch();
    }

    private void TryJoinKeyboardMouse()
    {
        if (HasKeyboardMousePlayer())
            return;

        bool keyboardPressed = Keyboard.current != null && Keyboard.current.anyKey.wasPressedThisFrame;
        bool mousePressed = Mouse.current != null && (
            Mouse.current.leftButton.wasPressedThisFrame ||
            Mouse.current.rightButton.wasPressedThisFrame ||
            Mouse.current.middleButton.wasPressedThisFrame ||
            Mouse.current.forwardButton.wasPressedThisFrame ||
            Mouse.current.backButton.wasPressedThisFrame ||
            Mouse.current.scroll.ReadValue().sqrMagnitude > 0.01f);

        if (!keyboardPressed && !mousePressed)
            return;

        SpawnPlayer(MultijugadorPlayerContext.ControlMode.KeyboardMouse, null);
    }

    private void TryJoinGamepad()
    {
        foreach (Gamepad gamepad in Gamepad.all)
        {
            if (IsGamepadJoined(gamepad))
                continue;

            if (!HasGamepadActivity(gamepad))
                continue;

            SpawnPlayer(MultijugadorPlayerContext.ControlMode.Gamepad, gamepad);
            break;
        }
    }

    private bool HasGamepadActivity(Gamepad gamepad)
    {
        if (gamepad == null)
            return false;

        return gamepad.buttonSouth.wasPressedThisFrame
            || gamepad.buttonNorth.wasPressedThisFrame
            || gamepad.buttonEast.wasPressedThisFrame
            || gamepad.buttonWest.wasPressedThisFrame
            || gamepad.startButton.wasPressedThisFrame
            || gamepad.selectButton.wasPressedThisFrame
            || gamepad.leftShoulder.wasPressedThisFrame
            || gamepad.rightShoulder.wasPressedThisFrame
            || gamepad.leftStick.ReadValue().sqrMagnitude > 0.3f
            || gamepad.rightStick.ReadValue().sqrMagnitude > 0.3f
            || gamepad.dpad.ReadValue().sqrMagnitude > 0.3f;
    }

    private bool IsGamepadJoined(Gamepad gamepad)
    {
        foreach (JoinedPlayer joinedPlayer in joinedPlayers)
        {
            if (joinedPlayer.gamepad == gamepad)
                return true;
        }

        return false;
    }

    private bool HasKeyboardMousePlayer()
    {
        foreach (JoinedPlayer joinedPlayer in joinedPlayers)
        {
            if (joinedPlayer.mode == MultijugadorPlayerContext.ControlMode.KeyboardMouse)
                return true;
        }

        return false;
    }

    private void SpawnPlayer(MultijugadorPlayerContext.ControlMode mode, Gamepad gamepad)
    {
        if (playerPrefab == null || joinedPlayers.Count >= 2)
            return;

        Vector3 spawnPosition = GetSpawnPosition(joinedPlayers.Count);
        Quaternion spawnRotation = Quaternion.identity;

        GameObject playerObject;

        if (reservedScenePlayer != null && !reservedScenePlayerClaimed)
        {
            playerObject = reservedScenePlayer;
            reservedScenePlayerClaimed = true;
            playerObject.transform.SetPositionAndRotation(spawnPosition, spawnRotation);
        }
        else
        {
            playerObject = Instantiate(playerPrefab, spawnPosition, spawnRotation);
        }

        MultijugadorPlayerContext context = playerObject.GetComponent<MultijugadorPlayerContext>();
        if (context == null)
            context = playerObject.AddComponent<MultijugadorPlayerContext>();

        if (mode == MultijugadorPlayerContext.ControlMode.KeyboardMouse)
            context.InitializeKeyboardMouse(inputActions);
        else
            context.InitializeGamepad(inputActions, gamepad);

        PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.SetInputContext(context);

        Cameralook cameraLook = playerObject.GetComponentInChildren<Cameralook>(true);
        if (cameraLook != null)
            cameraLook.SetInputContext(context);

        DisparoHechizo spellShooter = playerObject.GetComponentInChildren<DisparoHechizo>(true);
        if (spellShooter != null)
            spellShooter.SetInputContext(context);

        Selected selected = playerObject.GetComponentInChildren<Selected>(true);
        if (selected != null)
            selected.SetInputContext(context);

        SetPlayerGameplayEnabled(playerObject, false);

        joinedPlayers.Add(new JoinedPlayer
        {
            playerObject = playerObject,
            context = context,
            mode = mode,
            gamepad = gamepad
        });

        if (debugLogs)
        {
            string deviceName = mode == MultijugadorPlayerContext.ControlMode.KeyboardMouse ? "Keyboard&Mouse" : gamepad != null ? gamepad.displayName : "Gamepad";
            Debug.Log("MultijugadorManager: jugador unido -> " + deviceName);
        }
    }

    private Vector3 GetSpawnPosition(int index)
    {
        if (index == 0 && playerOneSpawnPoint != null)
            return playerOneSpawnPoint.position;

        if (index == 1 && playerTwoSpawnPoint != null)
            return playerTwoSpawnPoint.position;

        if (reservedScenePlayer != null && index == 0)
            return reservedScenePlayer.transform.position;

        if (reservedScenePlayer != null && index > 0)
            return reservedScenePlayer.transform.position + (spawnOffset * index);

        return transform.position + (spawnOffset * index);
    }

    private void StartMatch()
    {
        if (matchStarted)
            return;

        matchStarted = true;

        for (int i = 0; i < joinedPlayers.Count; i++)
            SetPlayerGameplayEnabled(joinedPlayers[i].playerObject, true);

        ConfigureSplitScreen();
        ConfigureAudioListeners();
        ConfigurePlayerHud(0, joinedPlayers[0].playerObject, mainCanvasTemplate, true);

        if (joinedPlayers.Count > 1)
        {
            GameObject hudTemplate = mainCanvasTemplate;
            if (hudTemplate != null)
            {
                playerTwoHudInstance = Instantiate(hudTemplate, hudTemplate.transform.position, hudTemplate.transform.rotation, hudTemplate.transform.parent);
                playerTwoHudInstance.name = MultijugadorUIConstants.SecondCanvasRootName;
                ConfigurePlayerHud(1, joinedPlayers[1].playerObject, playerTwoHudInstance, false);
            }
        }

        if (debugLogs)
            Debug.Log("MultijugadorManager: partida iniciada con " + joinedPlayers.Count + " jugadores.");
    }

    private void ConfigurePlayerHud(int playerIndex, GameObject playerObject, GameObject hudRoot, bool isPrimaryHud)
    {
        if (playerObject == null || hudRoot == null)
            return;

        PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
        Camera mainCamera = GetPlayerMainCamera(playerObject);
        Camera minimapCamera = GetPlayerMinimapCamera(playerObject);

        Canvas hudCanvas = hudRoot.GetComponent<Canvas>();
        if (hudCanvas != null)
        {
            hudCanvas.renderMode = RenderMode.ScreenSpaceCamera;
            hudCanvas.worldCamera = mainCamera;
            hudCanvas.planeDistance = 1f;
            hudCanvas.sortingOrder = playerIndex;
        }

        InventoryUI inventoryUI = hudRoot.GetComponentInChildren<InventoryUI>(true);
        if (inventoryUI != null)
        {
            inventoryUI.ConfigureForPlayer(playerMovement, hudRoot.transform);
        }

        StaminaBar staminaBar = hudRoot.GetComponentInChildren<StaminaBar>(true);
        if (staminaBar != null)
        {
            staminaBar.ConfigureForPlayer(playerMovement);
            if (playerMovement != null)
                playerMovement.SetStaminaBar(staminaBar);
        }

        RawImage miniMapImage = FindChildRawImage(hudRoot.transform, "MiniMapa");
        GameObject interactPrompt = FindChildGameObject(hudRoot.transform, "Interactuar");
        ConfigureMinimap(minimapCamera, miniMapImage, hudRoot.name);

        MultijugadorPlayerHUD hudLink = playerObject.GetComponent<MultijugadorPlayerHUD>();
        if (hudLink == null)
            hudLink = playerObject.AddComponent<MultijugadorPlayerHUD>();

        hudLink.Bind(hudCanvas, inventoryUI, staminaBar, miniMapImage, minimapCamera, interactPrompt);

        if (isPrimaryHud)
        {
            hudRoot.name = MultijugadorUIConstants.MainCanvasRootName;
        }
    }

    private void ConfigureMinimap(Camera minimapCamera, RawImage miniMapImage, string hudRootName)
    {
        if (minimapCamera == null)
            return;

        RenderTexture runtimeTexture = CreateMinimapTexture(hudRootName, minimapCamera);
        minimapCamera.targetTexture = runtimeTexture;

        if (miniMapImage != null)
        {
            miniMapImage.texture = runtimeTexture;
        }

        minimapCamera.Render();
    }

    private RenderTexture CreateMinimapTexture(string hudRootName, Camera minimapCamera)
    {
        int width = 256;
        int height = 256;
        int depth = 16;
        RenderTextureFormat format = RenderTextureFormat.ARGB32;

        if (minimapCamera != null)
        {
            width = Mathf.Max(256, minimapCamera.pixelWidth > 0 ? minimapCamera.pixelWidth : width);
            height = Mathf.Max(256, minimapCamera.pixelHeight > 0 ? minimapCamera.pixelHeight : height);
        }

        RenderTexture runtimeTexture = new RenderTexture(width, height, depth, format);
        runtimeTexture.name = hudRootName + "_MiniMapaRT";
        runtimeTexture.Create();
        return runtimeTexture;
    }

    private RawImage FindChildRawImage(Transform root, string targetName)
    {
        if (root == null)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == targetName)
                return child.GetComponent<RawImage>();
        }

        return null;
    }

    private GameObject FindChildGameObject(Transform root, string targetName)
    {
        if (root == null)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        foreach (Transform child in children)
        {
            if (child.name == targetName)
                return child.gameObject;
        }

        return null;
    }

    private void SetPlayerGameplayEnabled(GameObject playerObject, bool enabled)
    {
        if (playerObject == null)
            return;

        PlayerMovement playerMovement = playerObject.GetComponent<PlayerMovement>();
        if (playerMovement != null)
            playerMovement.enabled = enabled;

        Cameralook cameraLook = playerObject.GetComponentInChildren<Cameralook>(true);
        if (cameraLook != null)
            cameraLook.enabled = enabled;

        Selected selected = playerObject.GetComponentInChildren<Selected>(true);
        if (selected != null)
            selected.enabled = enabled;

        DisparoHechizo spellShooter = playerObject.GetComponentInChildren<DisparoHechizo>(true);
        if (spellShooter != null)
            spellShooter.enabled = enabled;

        MultijugadorPlayerContext context = playerObject.GetComponent<MultijugadorPlayerContext>();
        if (context != null)
            context.enabled = true;

        Camera mainCamera = GetPlayerMainCamera(playerObject);
        if (mainCamera != null)
            mainCamera.enabled = enabled;

        Camera minimapCamera = GetPlayerMinimapCamera(playerObject);
        if (minimapCamera != null)
            minimapCamera.enabled = enabled;
    }

    private void ConfigureSplitScreen()
    {
        if (joinedPlayers.Count < 2)
            return;

        Rect firstRect;
        Rect secondRect;

        if (splitHorizontally)
        {
            firstRect = new Rect(0f, 0.5f, 1f, 0.5f);
            secondRect = new Rect(0f, 0f, 1f, 0.5f);
        }
        else
        {
            firstRect = new Rect(0f, 0f, 0.5f, 1f);
            secondRect = new Rect(0.5f, 0f, 0.5f, 1f);
        }

        Camera firstCamera = GetPlayerMainCamera(joinedPlayers[0].playerObject);
        Camera secondCamera = GetPlayerMainCamera(joinedPlayers[1].playerObject);

        if (firstCamera != null)
            firstCamera.rect = firstRect;

        if (secondCamera != null)
            secondCamera.rect = secondRect;
    }

    private void ConfigureAudioListeners()
    {
        if (joinedPlayers.Count == 0)
            return;

        for (int i = 0; i < joinedPlayers.Count; i++)
        {
            SetAudioListenersForPlayer(joinedPlayers[i].playerObject, false);
        }

        SetAudioListenersForPlayer(joinedPlayers[0].playerObject, true);
    }

    private void SetAudioListenersForPlayer(GameObject playerObject, bool enabled)
    {
        if (playerObject == null)
            return;

        AudioListener[] listeners = playerObject.GetComponentsInChildren<AudioListener>(true);
        foreach (AudioListener listener in listeners)
        {
            if (listener != null)
                listener.enabled = enabled && listener.gameObject.name == "Player_Camera";
        }
    }

    private Camera GetPlayerMainCamera(GameObject playerObject)
    {
        if (playerObject == null)
            return null;

        Transform cameraTransform = playerObject.transform.Find("Player_Camera");
        if (cameraTransform == null)
        {
            Cameralook look = playerObject.GetComponentInChildren<Cameralook>(true);
            if (look != null)
                cameraTransform = look.transform;
        }

        return cameraTransform != null ? cameraTransform.GetComponent<Camera>() : null;
    }

    private Camera GetPlayerMinimapCamera(GameObject playerObject)
    {
        if (playerObject == null)
            return null;

        Transform minimapTransform = playerObject.transform.Find("Camera_Minimapa");
        return minimapTransform != null ? minimapTransform.GetComponent<Camera>() : null;
    }
}