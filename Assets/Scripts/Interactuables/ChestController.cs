using UnityEngine;
using System.Collections;

public class ChestController : MonoBehaviour
{
    [Header("Animator")]
    public Animator animator;

    [Header("State flags (runtime)")]
    public bool CofreAbierto = false;
    public bool ObjetoTomado = false;
    public bool Interactuado = false;

    private bool isRunningSequence = false;

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

        // start close sequence only if not already running
        if (!isRunningSequence)
            StartCoroutine(CloseSequence());
    }

    private IEnumerator OpenSequence()
    {
        isRunningSequence = true;

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
        }
        else
        {
            // no animator: simulate immediate open
            CofreAbierto = true;
        }

        isRunningSequence = false;
    }

    private IEnumerator CloseSequence()
    {
        isRunningSequence = true;

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
        }
        else
        {
            CofreAbierto = false;
            ObjetoTomado = false;
            Interactuado = false;
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
}
