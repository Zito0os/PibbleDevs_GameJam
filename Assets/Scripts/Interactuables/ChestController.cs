using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ChestController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("Contents")]
    [SerializeField] private List<ItemStack> contents = new List<ItemStack>();

    [Header("State flags (runtime)")]
    public bool CofreAbierto = false;
    public bool ObjetoTomado = false;
    public bool Interactuado = false;

    [Header("Raycast Layer Swap")]
    public bool cambiarLayerAlAbrir = true;
    public string layerAbierto = "Default";
    public string layerCerrado = "RayCastDetect";

    [Header("Auto Close When Empty")]
    public bool cerrarSiVacio = true;
    public float tiempoCerrarSiVacio = 5f;

    private bool isRunningSequence = false;
    private Dictionary<Transform, int> cachedLayers = new Dictionary<Transform, int>();
    private Coroutine autoCloseRoutine;
    private ItemSpawner cachedSpawner;
    private Transform cachedSpawnPoint;

    private void Start()
    {
        CacheLayersIfNeeded();

        SyncAnimatorFlags();

        CacheSpawnRefs();

        // Inicializar contenido de prueba si no hay nada
        if (contents.Count == 0)
        {
            contents.Add(new ItemStack(ItemType.KeySilver, 1));
            contents.Add(new ItemStack(ItemType.SpellSlow, 2));
        }
    }

    // Called by Selected.cs when player presses E
    public void AbrirCofre()
    {
        if (isRunningSequence)
            return;

        if (CofreAbierto)
        {
            Debug.Log("ChestController: ya está abierto.");
            return;
        }

        Interactuado = true;
        if (animator != null)
        {
            animator.SetBool("Interactuado", true);
            animator.SetBool("CofreAbierto", false);
            animator.SetBool("ObjetoTomado", false);
        }

        StartCoroutine(OpenSequence());
    }

    // kept for compatibility with existing calls from Selected.cs
    public void OnAfterAbrirCofre()
    {
        // No-op: the open sequence is handled in the coroutine.
    }

    // Call this when the player actually takes the object from the chest
    public void ItemTaken()
    {
        if (!CofreAbierto)
            return;

        ObjetoTomado = true;

        StopAutoClose();

        if (animator != null)
        {
            animator.SetBool("ObjetoTomado", true);
            animator.SetBool("CofreAbierto", true);
        }

        // start close sequence only if not already running
        if (!isRunningSequence)
            StartCoroutine(CloseSequence());
    }

    // Transfer items from chest to player inventory
    public void TransferItemsToPlayer(PlayerMovement player)
    {
        Inventory inventory = player.GetComponent<Inventory>();
        if (inventory == null)
        {
            Debug.LogWarning("ChestController: jugador sin Inventory");
            return;
        }

        int transferredCount = 0;
        foreach (ItemStack stack in contents)
        {
            bool added = inventory.AddItem(stack.itemType, stack.quantity);
            if (added)
            {
                Debug.Log($"ChestController: {stack} transferido al inventario del jugador.");
                transferredCount++;
            }
            else
            {
                Debug.LogWarning($"ChestController: inventario lleno, no se pudo transferir {stack}.");
                break;
            }
        }

        if (transferredCount > 0)
        {
            contents.RemoveRange(0, transferredCount);
        }
    }

    // Get chest contents (for UI or inspection)
    public IReadOnlyList<ItemStack> GetContents() => contents;

    private IEnumerator OpenSequence()
    {
        isRunningSequence = true;

        SoundManager.PlaySound(SoundType.AbrirCofre);

        // wait until animator enters the opening state
        if (animator != null)
        {
            // play the opening state immediately without blending to avoid visual revert
            PlayAnimatorStateImmediate(new string[] { "Abrir_Cofre", "Abrir Cofre", "Open_Cofre", "Open" });

            // wait until the animation is active (safety timeout)
            float timeout = 3f;
            float t = 0f;
            while (t < timeout)
            {
                var st = animator.GetCurrentAnimatorStateInfo(0);
                if (StateMatches(st, new string[] { "Abrir_Cofre", "Abrir Cofre", "Open_Cofre", "Open" }))
                    break;
                t += Time.deltaTime;
                yield return null;
            }

            // wait until the animation clip finishes (or timeout)
            t = 0f;
            float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
            // if length is zero (mismatch), fallback to 1s
            float targetWait = Mathf.Max(clipLength, 1f);
            while (t < targetWait)
            {
                t += Time.deltaTime;
                yield return null;
            }

            // force the idle_open state immediately to avoid brief revert frame
            // also force a visual update so the animator doesn't show the opening frames again
            PlayAnimatorStateImmediate(new string[] { "Idle_Abierto", "Idle abiertO", "Idle_abierto", "Idle Open", "Idle_Abierto" });
            animator.Update(0f);

            CofreAbierto = true;
            Interactuado = false;
            if (animator != null)
            {
                animator.SetBool("CofreAbierto", true);
                animator.SetBool("ObjetoTomado", false);
                animator.SetBool("Interactuado", false);
            }
            ApplyOpenLayer();
            TryAutoCloseIfEmpty();
        }
        else
        {
            // no animator: simulate immediate open
            CofreAbierto = true;
            Interactuado = false;
            ApplyOpenLayer();
            TryAutoCloseIfEmpty();
        }

        isRunningSequence = false;
    }

    private IEnumerator CloseSequence()
    {
        isRunningSequence = true;
        StopAutoClose();

        SoundManager.PlaySound(SoundType.CerrarCofre);

        // set parameter to start closing if needed
        Interactuado = true;
        if (animator != null)
        {
            // play closing state immediately
            PlayAnimatorStateImmediate(new string[] { "Cerrar_Cofre", "Cerrar Cofre", "Close_Cofre", "Close" });

            // wait for clip length (or fallback)
            float clipLength = animator.GetCurrentAnimatorStateInfo(0).length;
            float wait = Mathf.Max(clipLength, 1f);
            float elapsed = 0f;
            while (elapsed < wait)
            {
                elapsed += Time.deltaTime;
                yield return null;
            }

            // force idle_cofre state and update animator instantly
            PlayAnimatorStateImmediate(new string[] { "Idle_Cofre", "Idle Cofre", "Idle" });
            animator.Update(0f);

            CofreAbierto = false;
            ObjetoTomado = false;
            Interactuado = false;
            if (animator != null)
            {
                animator.SetBool("CofreAbierto", false);
                animator.SetBool("ObjetoTomado", false);
                animator.SetBool("Interactuado", false);
            }
            RestoreClosedLayer();
        }
        else
        {
            CofreAbierto = false;
            ObjetoTomado = false;
            Interactuado = false;
            RestoreClosedLayer();
        }

        isRunningSequence = false;
    }

    private void PlayAnimatorStateImmediate(string[] names)
    {
        foreach (var n in names)
        {
            if (string.IsNullOrEmpty(n))
                continue;

            try
            {
                animator.Play(n, 0, 0f);
                return;
            }
            catch
            {
                // ignore and try next
            }
        }
    }

    private bool StateMatches(AnimatorStateInfo st, string[] names)
    {
        foreach (var n in names)
        {
            if (st.IsName(n))
                return true;
        }
        return false;
    }

    private void CacheLayersIfNeeded()
    {
        if (cachedLayers.Count > 0)
            return;

        Transform[] all = GetComponentsInChildren<Transform>(true);
        for (int i = 0; i < all.Length; i++)
        {
            Transform t = all[i];
            if (ShouldSkipLayer(t))
                continue;

            if (!cachedLayers.ContainsKey(t))
            {
                cachedLayers.Add(t, t.gameObject.layer);
            }
        }
    }

    private bool ShouldSkipLayer(Transform t)
    {
        if (t.name == "SpawnObjetos")
            return true;

        if (t.GetComponent<ItemPickup>() != null)
            return true;

        return false;
    }

    private void ApplyOpenLayer()
    {
        if (!cambiarLayerAlAbrir)
            return;

        CacheLayersIfNeeded();

        int openLayer = LayerMask.NameToLayer(layerAbierto);
        if (openLayer < 0)
        {
            openLayer = 0;
        }

        foreach (var pair in cachedLayers)
        {
            if (pair.Key != null)
            {
                pair.Key.gameObject.layer = openLayer;
            }
        }
    }

    private void RestoreClosedLayer()
    {
        if (!cambiarLayerAlAbrir)
            return;

        CacheLayersIfNeeded();

        int closedLayer = LayerMask.NameToLayer(layerCerrado);
        foreach (var pair in cachedLayers)
        {
            if (pair.Key == null)
                continue;

            if (closedLayer >= 0)
            {
                pair.Key.gameObject.layer = closedLayer;
            }
            else
            {
                pair.Key.gameObject.layer = pair.Value;
            }
        }
    }

    private void SyncAnimatorFlags()
    {
        if (animator == null)
            return;

        animator.SetBool("CofreAbierto", CofreAbierto);
        animator.SetBool("ObjetoTomado", ObjetoTomado);
        animator.SetBool("Interactuado", Interactuado);
    }

    private void CacheSpawnRefs()
    {
        if (cachedSpawner == null)
        {
            cachedSpawner = GetComponentInChildren<ItemSpawner>(true);
        }

        if (cachedSpawnPoint == null)
        {
            Transform[] all = GetComponentsInChildren<Transform>(true);
            for (int i = 0; i < all.Length; i++)
            {
                if (all[i].name == "SpawnObjetos")
                {
                    cachedSpawnPoint = all[i];
                    break;
                }
            }
        }
    }

    private bool IsEmptyChest()
    {
        CacheSpawnRefs();

        if (cachedSpawner != null)
        {
            return cachedSpawner.IsEmpty;
        }

        if (cachedSpawnPoint != null)
        {
            return cachedSpawnPoint.childCount == 0;
        }

        return false;
    }

    private void TryAutoCloseIfEmpty()
    {
        if (!cerrarSiVacio)
            return;

        if (!IsEmptyChest())
            return;

        StopAutoClose();
        autoCloseRoutine = StartCoroutine(AutoCloseEmptyRoutine());
    }

    private IEnumerator AutoCloseEmptyRoutine()
    {
        yield return new WaitForSeconds(tiempoCerrarSiVacio);

        if (!CofreAbierto || ObjetoTomado)
            yield break;

        if (!IsEmptyChest())
            yield break;

        ObjetoTomado = true;
        if (animator != null)
        {
            animator.SetBool("ObjetoTomado", true);
            animator.SetBool("CofreAbierto", true);
        }

        if (!isRunningSequence)
            StartCoroutine(CloseSequence());
    }

    private void StopAutoClose()
    {
        if (autoCloseRoutine != null)
        {
            StopCoroutine(autoCloseRoutine);
            autoCloseRoutine = null;
        }
    }
}
