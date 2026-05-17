using UnityEngine;
using System.Collections;

public class Door_Controller : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Debug")]
    public bool debugDoorLogs = true;

    private Transform doorPivot;

    public int Tipo_de_puerta = 0; // 0: normal, 1: plata, 2: oro

    [Header("Key Requirements")]
    public bool requiereLlave = false;
    public ItemType llaveRequerida = ItemType.KeySilver;

    [Header("Victory")]
    public bool Puerta_Ganadora = false;

    private const string ParamAbriendoPuerta = "abriendo_puerta";
    private const string ParamPuertaAbierta = "puerta_abierta";
    private const string ParamCerrandoPuerta = "cerrando_puerta";
    private const string ParamPuertaCerrada = "puerta_cerrada";

    private const string StateAbriendo = "Open_door";
    private const string StateAbierta = "Idle_abierto";
    private const string StateCerrando = "Close_Door";
    private const string StateCerrada = "Idle_puerta";

    [Header("State flags (runtime)")]
    public bool CofreAbierto = false;
    public bool ObjetoTomado = false;
    public bool Interactuado = false;

    private bool isRunningSequence = false;
    private InventoryUI cachedInventoryUI;
    private DoorWheelMinigame cachedWheelMinigame;
    private PlayerMovement currentInteractorPlayer;

    private void Awake()
    {
        CreatePivotIfNeeded();
        SyncKeySettingsWithDoorType();
    }

    private void OnValidate()
    {
        SyncKeySettingsWithDoorType();
    }

    private void SyncKeySettingsWithDoorType()
    {
        if (Tipo_de_puerta <= 0)
        {
            requiereLlave = false;
            return;
        }

        requiereLlave = true;
        llaveRequerida = Tipo_de_puerta == 1 ? ItemType.KeySilver : ItemType.KeyGold;
    }

    private void CreatePivotIfNeeded()
    {
        if (doorPivot != null)
            return;

        GameObject pivotObject = new GameObject(gameObject.name + "_Pivot");
        pivotObject.transform.SetPositionAndRotation(transform.position, transform.rotation);
        pivotObject.transform.localScale = transform.lossyScale;

        Transform originalParent = transform.parent;
        pivotObject.transform.SetParent(originalParent, true);
        transform.SetParent(pivotObject.transform, true);
        transform.localPosition = Vector3.zero;
        transform.localRotation = Quaternion.identity;
        transform.localScale = Vector3.one;

        doorPivot = pivotObject.transform;
    }

    // Called by Selected.cs when player presses E
    public void AbrirCofre()
    {
        AbrirCofre(null);
    }

    public void AbrirCofre(PlayerMovement player)
    {
        currentInteractorPlayer = player;

        if (debugDoorLogs)
        {
            Debug.Log("[Door] Input recibido -> puerta=" + name + " | isRunningSequence=" + isRunningSequence + " | CofreAbierto=" + CofreAbierto);
        }

        if (DoorWheelMinigame.IsRunning)
        {
            if (debugDoorLogs)
                Debug.Log("[Door] Input ignorado: minijuego activo en " + name);
            return;
        }

        if (isRunningSequence)
        {
            if (debugDoorLogs)
                Debug.Log("[Door] Input ignorado: secuencia en ejecucion en " + name);
            return;
        }

        if (CofreAbierto)
        {
            if (debugDoorLogs)
                Debug.Log("[Door] Iniciando cierre en " + name);
            StartCoroutine(CloseSequence());
            return;
        }

        if (requiereLlave)
        {
            if (!TieneLlaveSeleccionadaValida(player))
            {
                if (debugDoorLogs)
                    Debug.LogWarning("[Door] No se pudo iniciar la ruleta en " + name + ". Llave no válida o no seleccionada.");
                return;
            }

            DoorWheelMinigame wheelMinigame = GetWheelMinigame();
            if (wheelMinigame == null)
            {
                if (debugDoorLogs)
                    Debug.LogWarning("[Door] No se encontró DoorWheelMinigame para " + name);
                return;
            }

            bool started = wheelMinigame.StartChallenge(this, player, llaveRequerida);
            if (!started && debugDoorLogs)
            {
                Debug.LogWarning("[Door] No se pudo iniciar la ruleta en " + name);
            }
            return;
        }

        if (debugDoorLogs)
            Debug.Log("[Door] Iniciando apertura en " + name);
        StartCoroutine(OpenSequence());
    }

    // kept for compatibility with existing calls from Selected.cs
    public void OnAfterAbrirCofre()
    {
        // No-op: the open/close sequence is handled in the coroutine.
    }

    // Kept for compatibility with existing calls from Selected.cs
    public void ItemTaken()
    {
        // No-op for door logic.
    }

    private IEnumerator OpenSequence()
    {
        isRunningSequence = true;
        Interactuado = true;

        SoundManager.PlaySound(SoundType.AbrirPuerta);

        if (debugDoorLogs)
            Debug.Log("[Door] OpenSequence START -> " + name);

        if (animator != null)
        {
            animator.SetBool(ParamCerrandoPuerta, false);
            animator.SetBool(ParamPuertaCerrada, false);
            animator.SetBool(ParamPuertaAbierta, false);
            animator.SetBool(ParamAbriendoPuerta, true);

            bool statePlayed = PlayAnimatorStateImmediate(StateAbriendo);
            if (debugDoorLogs)
                Debug.Log("[Door] OpenSequence Play(" + StateAbriendo + ") -> " + statePlayed + " en " + name);

            if (!statePlayed)
                Debug.LogWarning("[Door] No existe el estado '" + StateAbriendo + "' en el Animator de " + name);

            yield return null;

            yield return WaitForSeconds(GetCurrentStateLength(1f));

            animator.SetBool(ParamAbriendoPuerta, false);
            animator.SetBool(ParamPuertaAbierta, true);

            bool idlePlayed = PlayAnimatorStateImmediate(StateAbierta);
            if (debugDoorLogs)
                Debug.Log("[Door] OpenSequence Play(" + StateAbierta + ") -> " + idlePlayed + " en " + name);

            if (!idlePlayed)
                Debug.LogWarning("[Door] No existe el estado '" + StateAbierta + "' en el Animator de " + name);

            animator.Update(0f);
        }

        CofreAbierto = true;
        ObjetoTomado = false;
        Interactuado = false;
        isRunningSequence = false;

        if (debugDoorLogs)
            Debug.Log("[Door] OpenSequence END -> " + name + " | CofreAbierto=" + CofreAbierto);
    }

    private IEnumerator CloseSequence()
    {
        isRunningSequence = true;
        Interactuado = true;

        SoundManager.PlaySound(SoundType.CerrarPuerta);

        if (debugDoorLogs)
            Debug.Log("[Door] CloseSequence START -> " + name);

        if (animator != null)
        {
            animator.SetBool(ParamAbriendoPuerta, false);
            animator.SetBool(ParamPuertaAbierta, false);
            animator.SetBool(ParamPuertaCerrada, false);
            animator.SetBool(ParamCerrandoPuerta, true);

            bool statePlayed = PlayAnimatorStateImmediate(StateCerrando);
            if (debugDoorLogs)
                Debug.Log("[Door] CloseSequence Play(" + StateCerrando + ") -> " + statePlayed + " en " + name);

            if (!statePlayed)
                Debug.LogWarning("[Door] No existe el estado '" + StateCerrando + "' en el Animator de " + name);

            yield return null;

            yield return WaitForSeconds(GetCurrentStateLength(1f));

            animator.SetBool(ParamCerrandoPuerta, false);
            animator.SetBool(ParamPuertaCerrada, true);

            bool idlePlayed = PlayAnimatorStateImmediate(StateCerrada);
            if (debugDoorLogs)
                Debug.Log("[Door] CloseSequence Play(" + StateCerrada + ") -> " + idlePlayed + " en " + name);

            if (!idlePlayed)
                Debug.LogWarning("[Door] No existe el estado '" + StateCerrada + "' en el Animator de " + name);

            animator.Update(0f);
        }

        CofreAbierto = false;
        ObjetoTomado = false;
        Interactuado = false;
        isRunningSequence = false;

        if (debugDoorLogs)
            Debug.Log("[Door] CloseSequence END -> " + name + " | CofreAbierto=" + CofreAbierto);
    }

    private float GetCurrentStateLength(float fallback)
    {
        if (animator == null)
            return fallback;

        float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
        return Mathf.Max(clipLength, fallback);
    }

    public void ResolveWheelSuccess(PlayerMovement player, ItemType keyType)
    {
        currentInteractorPlayer = player;

        if (player == null)
            return;

        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null)
            return;

        if (inventory.hasActiveItem && inventory.activeItemType == keyType)
        {
            inventory.UseSelectedItem();
        }

        EventManager eventManager = EventManager.Instance != null ? EventManager.Instance : FindFirstObjectByType<EventManager>();
        if (eventManager != null)
        {
            eventManager.NotifyGoldenDoorOpened(this, player);
        }

        ShowDoorMessage("Puerta abierta");
        StartCoroutine(OpenSequence());
    }

    public void ResolveWheelFailure(PlayerMovement player, ItemType keyType)
    {
        currentInteractorPlayer = player;

        if (player != null)
        {
            Inventory inventory = player.GetComponent<Inventory>();
            if (inventory != null && inventory.hasActiveItem && inventory.activeItemType == keyType)
            {
                inventory.UseSelectedItem();
            }
        }

        SoundManager.PlaySound(SoundType.LlaveRota);

        ShowDoorMessage("La llave se rompió");
    }

    private bool TieneLlaveSeleccionadaValida(PlayerMovement player)
    {
        if (!requiereLlave)
            return true;

        if (player == null)
        {
            ShowDoorMessage("Se necesita llave");
            return false;
        }

        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null)
        {
            ShowDoorMessage("Se necesita llave");
            return false;
        }

        if (!inventory.hasActiveItem)
        {
            ShowDoorMessage("Se necesita llave");
            return false;
        }

        if (inventory.activeItemType != llaveRequerida)
        {
            if (EsLlaveInventario(inventory.activeItemType))
            {
                SoundManager.PlaySound(SoundType.ErrorEleccion);
                ShowDoorMessage("LLave equivocada");
            }
            else
                ShowDoorMessage("Se necesita llave");

            return false;
        }

        return true;
    }

    private bool EsLlaveInventario(ItemType itemType)
    {
        return itemType == ItemType.KeySilver || itemType == ItemType.KeyGold;
    }

    private void ShowDoorMessage(string message)
    {
        if (currentInteractorPlayer != null)
        {
            MultijugadorPlayerHUD hud = currentInteractorPlayer.GetComponent<MultijugadorPlayerHUD>();
            if (hud != null && hud.inventoryUI != null)
            {
                hud.inventoryUI.SetDoorMessage(message);
                return;
            }
        }

        InventoryUI inventoryUI = GetInventoryUI();
        if (inventoryUI != null)
        {
            inventoryUI.SetDoorMessage(message);
        }
        else if (debugDoorLogs)
        {
            Debug.LogWarning("[Door] No se encontró InventoryUI para mostrar el mensaje: " + message);
        }
    }

    private InventoryUI GetInventoryUI()
    {
        if (cachedInventoryUI == null)
        {
            cachedInventoryUI = FindFirstObjectByType<InventoryUI>();
        }

        return cachedInventoryUI;
    }

    private DoorWheelMinigame GetWheelMinigame()
    {
        if (cachedWheelMinigame == null)
        {
            cachedWheelMinigame = FindFirstObjectByType<DoorWheelMinigame>();
        }

        return cachedWheelMinigame;
    }

    private bool PlayAnimatorStateImmediate(string stateName)
    {
        if (animator == null)
            return false;

        if (string.IsNullOrEmpty(stateName))
            return false;

        int hash = Animator.StringToHash(stateName);
        if (!animator.HasState(0, hash))
            return false;

        animator.Play(hash, 0, 0f);
        return true;
    }

    private IEnumerator WaitForSeconds(float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            yield return null;
        }
    }
}
