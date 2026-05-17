using UnityEngine;
using UnityEngine.InputSystem;

public class Ver_teclas : MonoBehaviour
{
    private enum ToggleSource
    {
        None,
        Keyboard,
        Gamepad
    }

    [Header("UI References")]
    [SerializeField] private GameObject panelTeclas;
    [SerializeField] private GameObject teclado;
    [SerializeField] private GameObject gamePad;

    [Header("Behavior")]
    [SerializeField] private bool ocultarVisualesAlIniciar = true;

    private MultijugadorPlayerContext inputContext;
    private bool visualesVisibles;
    private ToggleSource lastToggleSource = ToggleSource.Keyboard;

    private void Awake()
    {
        RefreshInputContext();
        BindReferences();

        SetVisualesVisible(!ocultarVisualesAlIniciar);
    }

    private void Update()
    {
        RefreshInputContext();

        if (panelTeclas == null || teclado == null || gamePad == null)
            BindReferences();

        if (TryReadTogglePressed(out ToggleSource source))
        {
            if (source != ToggleSource.None)
                lastToggleSource = source;

            SetVisualesVisible(!visualesVisibles);
        }

        if (visualesVisibles)
        {
            RefreshDeviceVisual();
        }
    }

    private void RefreshInputContext()
    {
        if (inputContext != null)
            return;
        inputContext = GetComponentInParent<MultijugadorPlayerContext>();
        if (inputContext == null)
            inputContext = GetComponent<MultijugadorPlayerContext>();

        // Do not fall back to global keyboard/gamepad polling. Prefer explicit binding from the
        // MultijugadorManager so each HUD instance responds only to its player's inputs.
        if (inputContext == null)
            inputContext = FindContextFromLocalHud();
    }

    private void BindReferences()
    {
        // Prefer searching in this object's local canvas so each player controls only their HUD.
        Transform searchRoot = transform;
        Canvas localCanvas = GetComponentInParent<Canvas>(true);
        if (localCanvas != null)
            searchRoot = localCanvas.transform;

        if (panelTeclas == null)
            panelTeclas = FindChildByName(searchRoot, "Teclas");

        Transform panelTransform = panelTeclas != null ? panelTeclas.transform : null;

        if (teclado == null && panelTransform != null)
        {
            Transform found = FindChildByName(panelTransform, "Teclado")?.transform;
            if (found != null)
                teclado = found.gameObject;
        }

        if (gamePad == null && panelTransform != null)
        {
            Transform found = FindChildByName(panelTransform, "GamePad")?.transform;
            if (found != null)
                gamePad = found.gameObject;
        }
    }

    private GameObject FindChildByName(Transform root, string targetName)
    {
        if (root == null)
            return null;

        Transform[] children = root.GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < children.Length; i++)
        {
            Transform child = children[i];
            if (child != null && child.name == targetName)
                return child.gameObject;
        }

        return null;
    }

    private bool TryReadTogglePressed(out ToggleSource source)
    {
        source = ToggleSource.None;
        // Require an explicit per-player input context to decide who toggled the panel.
        if (inputContext == null)
            return false;

        if (inputContext.HelpTogglePressedThisFrame)
        {
            source = inputContext.IsGamepad ? ToggleSource.Gamepad : ToggleSource.Keyboard;
            return true;
        }

        return false;
    }

    // Called by MultijugadorManager when configuring the HUD so the Ver_teclas instance is
    // explicitly bound to the correct player's input context. This ensures only the invoking
    // player's HUD reacts to help toggles.
    public void Bind(MultijugadorPlayerContext context)
    {
        inputContext = context;
    }

    private void SetVisualesVisible(bool visible)
    {
        visualesVisibles = visible;
        RefreshDeviceVisual();
    }

    private void RefreshDeviceVisual()
    {
        if (!visualesVisibles)
        {
            if (teclado != null)
                teclado.SetActive(false);

            if (gamePad != null)
                gamePad.SetActive(false);

            return;
        }

        bool useKeyboard = lastToggleSource != ToggleSource.Gamepad;

        if (teclado != null)
            teclado.SetActive(useKeyboard);

        if (gamePad != null)
            gamePad.SetActive(!useKeyboard);
    }

    private MultijugadorPlayerContext FindContextFromLocalHud()
    {
        Canvas localCanvas = GetComponentInParent<Canvas>(true);
        if (localCanvas == null)
            return null;

        MultijugadorPlayerHUD[] huds = FindObjectsOfType<MultijugadorPlayerHUD>(true);
        for (int i = 0; i < huds.Length; i++)
        {
            MultijugadorPlayerHUD hud = huds[i];
            if (hud == null || hud.hudCanvas == null)
                continue;

            if (hud.hudCanvas == localCanvas)
            {
                return hud.GetComponent<MultijugadorPlayerContext>();
            }
        }

        return null;
    }
}
