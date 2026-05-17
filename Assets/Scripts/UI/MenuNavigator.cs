using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

[DisallowMultipleComponent]
public class MenuNavigator : MonoBehaviour
{
    [Header("Buttons")]
    [Tooltip("If empty, will collect all Button components under this GameObject (in scene order by Y).")]
    public List<Button> menuButtons = new List<Button>();

    [Header("Behavior")]
    public int startIndex = 0;

    private int currentIndex = -1;

    private void Start()
    {
        if (menuButtons == null || menuButtons.Count == 0)
        {
            // Find buttons under this transform and order them top->bottom by world Y
            var found = GetComponentsInChildren<Button>(true);
            menuButtons = found.OrderBy(b => -b.transform.position.y).ToList();
        }

        if (menuButtons == null)
            menuButtons = new List<Button>();

        startIndex = Mathf.Clamp(startIndex, 0, Mathf.Max(0, menuButtons.Count - 1));
        if (menuButtons.Count > 0)
            SetSelection(startIndex);
    }

    private void Update()
    {
        if (menuButtons == null || menuButtons.Count == 0)
            return;

        // Read dpad or keyboard up/down
        bool up = (Gamepad.current != null && Gamepad.current.dpad.up.wasPressedThisFrame)
                  || (Keyboard.current != null && Keyboard.current.upArrowKey.wasPressedThisFrame);

        bool down = (Gamepad.current != null && Gamepad.current.dpad.down.wasPressedThisFrame)
                    || (Keyboard.current != null && Keyboard.current.downArrowKey.wasPressedThisFrame);

        if (up)
            MoveSelection(-1);

        if (down)
            MoveSelection(1);

        // Activate with A / Enter / Space
        bool activate = (Gamepad.current != null && Gamepad.current.buttonSouth.wasPressedThisFrame)
                        || (Keyboard.current != null && (Keyboard.current.enterKey.wasPressedThisFrame || Keyboard.current.spaceKey.wasPressedThisFrame));

        if (activate && currentIndex >= 0 && currentIndex < menuButtons.Count)
        {
            Button b = menuButtons[currentIndex];
            if (b != null && b.gameObject.activeInHierarchy && b.interactable)
            {
                b.onClick.Invoke();
            }
        }
    }

    private void MoveSelection(int delta)
    {
        if (menuButtons.Count == 0)
            return;

        int next = currentIndex + delta;
        if (next < 0)
            next = menuButtons.Count - 1;
        else if (next >= menuButtons.Count)
            next = 0;

        SetSelection(next);
    }

    private void SetSelection(int index)
    {
        currentIndex = Mathf.Clamp(index, 0, menuButtons.Count - 1);
        Button b = menuButtons[currentIndex];
        if (b != null)
        {
            EventSystem es = EventSystem.current;
            if (es != null)
            {
                es.SetSelectedGameObject(b.gameObject);
            }
        }
    }
}
