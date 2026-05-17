using UnityEngine;
using UnityEngine.InputSystem;

[DisallowMultipleComponent]
public class MultijugadorPlayerContext : MonoBehaviour
{
    public enum ControlMode
    {
        KeyboardMouse,
        Gamepad
    }

    [Header("Runtime")]
    [SerializeField] private ControlMode controlMode = ControlMode.KeyboardMouse;

    private InputActionAsset inputActionsAsset;
    private Gamepad assignedGamepad;

    private InputAction moveAction;
    private InputAction lookAction;
    private InputAction attackAction;
    private InputAction interactAction;
    private InputAction jumpAction;
    private InputAction sprintAction;
    private InputAction previousAction;
    private InputAction nextAction;

    public ControlMode Mode => controlMode;
    public bool IsKeyboardMouse => controlMode == ControlMode.KeyboardMouse;
    public bool IsGamepad => controlMode == ControlMode.Gamepad;

    public void InitializeKeyboardMouse(InputActionAsset actionsAsset)
    {
        controlMode = ControlMode.KeyboardMouse;
        inputActionsAsset = actionsAsset;
        CacheActions(null);
    }

    public void InitializeGamepad(InputActionAsset actionsAsset, Gamepad gamepad)
    {
        controlMode = ControlMode.Gamepad;
        inputActionsAsset = actionsAsset;
        assignedGamepad = gamepad;

        CacheActions(inputActionsAsset);
    }

    public Vector2 Move => IsKeyboardMouse
        ? ReadKeyboardMove()
        : assignedGamepad != null ? assignedGamepad.leftStick.ReadValue() : ReadVector2(moveAction);

    public Vector2 Look => IsKeyboardMouse
        ? ReadMouseLook()
        : assignedGamepad != null ? assignedGamepad.rightStick.ReadValue() : ReadVector2(lookAction);

    public bool JumpPressedThisFrame => IsKeyboardMouse
        ? WasKeyboardPressed(KeyCode.Space)
        : assignedGamepad != null ? assignedGamepad.buttonSouth.wasPressedThisFrame : WasPressed(jumpAction);

    public bool SprintHeld => IsKeyboardMouse
        ? IsKeyboardHeld(KeyCode.LeftShift)
        : assignedGamepad != null ? assignedGamepad.leftStickButton.isPressed : IsPressed(sprintAction);

    public bool AttackPressedThisFrame => IsKeyboardMouse
        ? WasMouseButtonPressed(0)
        : assignedGamepad != null ? assignedGamepad.buttonWest.wasPressedThisFrame : WasPressed(attackAction);

    public bool InteractPressedThisFrame => IsKeyboardMouse
        ? WasKeyboardPressed(KeyCode.E)
        : assignedGamepad != null ? assignedGamepad.buttonNorth.wasPressedThisFrame : WasPressed(interactAction);

    public bool HelpTogglePressedThisFrame => IsKeyboardMouse
        ? WasKeyboardPressed(KeyCode.Tab)
        : assignedGamepad != null ? assignedGamepad.leftShoulder.wasPressedThisFrame : Gamepad.current != null && Gamepad.current.leftShoulder.wasPressedThisFrame;

    public bool CyclePreviousPressedThisFrame => IsKeyboardMouse
        ? WasKeyboardPressed(KeyCode.Alpha1)
        : assignedGamepad != null ? assignedGamepad.dpad.left.wasPressedThisFrame : WasPressed(previousAction);

    public bool CycleNextPressedThisFrame => IsKeyboardMouse
        ? WasKeyboardPressed(KeyCode.Alpha2)
        : assignedGamepad != null ? assignedGamepad.dpad.right.wasPressedThisFrame : WasPressed(nextAction);

    public bool TryGetDirectSlotSelection(out int slotIndex)
    {
        slotIndex = -1;

        if (!IsKeyboardMouse)
            return false;

        if (WasKeyboardPressed(KeyCode.Alpha1)) { slotIndex = 0; return true; }
        if (WasKeyboardPressed(KeyCode.Alpha2)) { slotIndex = 1; return true; }
        if (WasKeyboardPressed(KeyCode.Alpha3)) { slotIndex = 2; return true; }
        if (WasKeyboardPressed(KeyCode.Alpha4)) { slotIndex = 3; return true; }
        if (WasKeyboardPressed(KeyCode.Alpha5)) { slotIndex = 4; return true; }

        return false;
    }

    public float ScrollDeltaY => IsKeyboardMouse ? ReadMouseScrollDelta() : 0f;

    private void CacheActions(InputActionAsset asset)
    {
        moveAction = FindAction(asset, "Move");
        lookAction = FindAction(asset, "Look");
        attackAction = FindAction(asset, "Attack");
        interactAction = FindAction(asset, "Interact");
        jumpAction = FindAction(asset, "Jump");
        sprintAction = FindAction(asset, "Sprint");
        previousAction = FindAction(asset, "Previous");
        nextAction = FindAction(asset, "Next");
    }

    private InputAction FindAction(InputActionAsset asset, string actionName)
    {
        if (asset == null)
            return null;

        InputAction action = asset.FindAction($"Player/{actionName}", false);
        if (action != null)
            return action;

        return asset.FindAction(actionName, false);
    }

    private Vector2 ReadVector2(InputAction action)
    {
        if (action == null)
            return Vector2.zero;

        return action.ReadValue<Vector2>();
    }

    private bool WasPressed(InputAction action)
    {
        return action != null && action.WasPressedThisFrame();
    }

    private bool IsPressed(InputAction action)
    {
        return action != null && action.IsPressed();
    }

    private Vector2 ReadKeyboardMove()
    {
        if (Keyboard.current == null)
            return Vector2.zero;

        float horizontal = 0f;
        float vertical = 0f;

        if (Keyboard.current.aKey.isPressed || Keyboard.current.leftArrowKey.isPressed)
            horizontal -= 1f;

        if (Keyboard.current.dKey.isPressed || Keyboard.current.rightArrowKey.isPressed)
            horizontal += 1f;

        if (Keyboard.current.sKey.isPressed || Keyboard.current.downArrowKey.isPressed)
            vertical -= 1f;

        if (Keyboard.current.wKey.isPressed || Keyboard.current.upArrowKey.isPressed)
            vertical += 1f;

        return new Vector2(horizontal, vertical);
    }

    private Vector2 ReadMouseLook()
    {
        if (Mouse.current == null)
            return Vector2.zero;

        return Mouse.current.delta.ReadValue();
    }

    private bool WasKeyboardPressed(KeyCode key)
    {
        if (Keyboard.current == null)
            return false;

        return key switch
        {
            KeyCode.Space => Keyboard.current.spaceKey.wasPressedThisFrame,
            KeyCode.Tab => Keyboard.current.tabKey.wasPressedThisFrame,
            KeyCode.E => Keyboard.current.eKey.wasPressedThisFrame,
            KeyCode.Alpha1 => Keyboard.current.digit1Key.wasPressedThisFrame,
            KeyCode.Alpha2 => Keyboard.current.digit2Key.wasPressedThisFrame,
            KeyCode.Alpha3 => Keyboard.current.digit3Key.wasPressedThisFrame,
            KeyCode.Alpha4 => Keyboard.current.digit4Key.wasPressedThisFrame,
            KeyCode.Alpha5 => Keyboard.current.digit5Key.wasPressedThisFrame,
            _ => false
        };
    }

    private bool IsKeyboardHeld(KeyCode key)
    {
        if (Keyboard.current == null)
            return false;

        return key switch
        {
            KeyCode.LeftShift => Keyboard.current.leftShiftKey.isPressed,
            _ => false
        };
    }

    private bool WasMouseButtonPressed(int buttonIndex)
    {
        if (Mouse.current == null)
            return false;

        return buttonIndex switch
        {
            0 => Mouse.current.leftButton.wasPressedThisFrame,
            1 => Mouse.current.rightButton.wasPressedThisFrame,
            2 => Mouse.current.middleButton.wasPressedThisFrame,
            _ => false
        };
    }

    private float ReadMouseScrollDelta()
    {
        if (Mouse.current == null)
            return 0f;

        return Mouse.current.scroll.ReadValue().y;
    }
}