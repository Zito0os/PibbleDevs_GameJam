using UnityEngine;
using System.Collections;

public class PlayerMovement : MonoBehaviour
{
    public CharacterController characterController;
    public float speed = 15f;
    public float gravity = -3f;

    private Vector3 velocity;

    public Transform groundCheck;
    public float sphereRadius = 0.3f;
    public LayerMask groundMask;

    private bool isGrounded;

    public Animator animator;

    public float jumpheigth = 3f;

    public bool isSprinting;
    public float sprintSpeedMultiplier = 2f;
    private float sprintSpeed = 1f;

    public float staminaUseAmount = 5f;
    [SerializeField] private string attackAnimationBoolName = "Atack";
    [SerializeField] private float attackAnimationHoldTime = 0.15f;

    private StaminaBar staminaSlider;
    private Inventory inventory;
    private MultijugadorPlayerContext playerInputContext;
    private Coroutine attackAnimationRoutine;

    [Header("Audio pasos")]
    public AudioSource audioPasosCaminar;
    public AudioSource audioPasosCorrer;
    public AudioClip sonidoCaminar;
    public AudioClip sonidoCorrer;
    [Range(0f, 1f)] public float volumenCaminar = 0.7f;
    [Range(0f, 1f)] public float volumenCorrer = 0.85f;
    [SerializeField] private float caminarIntervalo = 0.45f;
    [SerializeField] private float correrIntervalo = 0.3f;
    private float pasoTimer;

    private void Start()
    {
        inventory = GetComponent<Inventory>();

        if (inventory == null)
            inventory = FindFirstObjectByType<Inventory>();

        playerInputContext = GetComponent<MultijugadorPlayerContext>();

        ConfigurarAudioPasos();
    }

    public void SetInputContext(MultijugadorPlayerContext inputContext)
    {
        playerInputContext = inputContext;
    }

    public void SetStaminaBar(StaminaBar targetBar)
    {
        staminaSlider = targetBar;
    }

    private void Update()
    {
        if (DoorWheelMinigame.IsRunning)
            return;

        if (groundCheck != null)
        {
            isGrounded = Physics.CheckSphere(groundCheck.position, sphereRadius, groundMask);
        }

        if (isGrounded && velocity.y < 0f)
            velocity.y = -2f;

        Vector2 moveInput = GetMoveInput();
        float x = moveInput.x;
        float z = moveInput.y;

        bool estaMoviendose = Mathf.Abs(x) > 0.01f || Mathf.Abs(z) > 0.01f;
        Vector3 move = transform.right * x + transform.forward * z;
        animator.SetFloat("VelX", x);
        animator.SetFloat("VelZ", z);
        

        JunpCheck();
        RunCheck();
        SelectInventorySlotCheck();
        ScrollInventorySlotCheck();
        UseItemCheck();
        ActualizarAudioPasos(estaMoviendose);

        if (characterController != null)
        {
            characterController.Move(move * speed * Time.deltaTime * sprintSpeed);

            velocity.y += gravity * Time.deltaTime;
            characterController.Move(velocity * Time.deltaTime);
        }
    }

    private Vector2 GetMoveInput()
    {
        if (playerInputContext != null)
            return playerInputContext.Move;

        if (HasMultiplayerContext())
            return Vector2.zero;

        return new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical"));
    }

    public void JunpCheck()
    {
        bool jumpPressed = playerInputContext != null ? playerInputContext.JumpPressedThisFrame : (!HasMultiplayerContext() && Input.GetKeyDown(KeyCode.Space));

        if (jumpPressed && isGrounded)
            velocity.y = Mathf.Sqrt(jumpheigth * -2f * gravity);
    }

    public void RunCheck()
    {
        bool holdSprint = playerInputContext != null ? playerInputContext.SprintHeld : (!HasMultiplayerContext() && Input.GetKey(KeyCode.LeftShift));

        bool canSprint = staminaSlider == null || staminaSlider.HasStamina;
        

        if (holdSprint && !isSprinting && canSprint)
            isSprinting = true;

        if (!canSprint && isSprinting)
            isSprinting = false;

        animator.SetBool("isSprinting", isSprinting);

        if (!holdSprint && isSprinting)
        {
            isSprinting = false;
            if (staminaSlider != null)
                staminaSlider.StopSprinting();
            animator.SetBool("isSprinting", isSprinting);
        }

        if (isSprinting)
        {
            sprintSpeed = sprintSpeedMultiplier;
            if (staminaSlider != null)
                staminaSlider.UseStamina(staminaUseAmount * Time.deltaTime);
        }
        else
        {
            sprintSpeed = 1f;
        }

        
    }

    private void UseItemCheck()
    {
        bool attackPressed = playerInputContext != null ? playerInputContext.AttackPressedThisFrame : (!HasMultiplayerContext() && Input.GetMouseButtonDown(0));

        if (!attackPressed || inventory == null)
            return;

        // Hechizos se consumen en DisparoHechizo.cs
        if (EsHechizoInventario(inventory.activeItemType) || EsLlaveInventario(inventory.activeItemType))
            return;

        bool used = inventory.UseSelectedItem();
        if (used)
            Debug.Log($"PlayerMovement: item usado con click izquierdo desde slot {inventory.activeSlotIndex + 1}.");
    }

    private bool EsLlaveInventario(ItemType itemType)
    {
        return itemType == ItemType.KeyGold || itemType == ItemType.KeySilver;
    }

    private bool EsHechizoInventario(ItemType itemType)
    {
        return itemType == ItemType.SpellSlow
            || itemType == ItemType.SpellFreeze
            || itemType == ItemType.SpellClear;
    }

    private void SelectInventorySlotCheck()
    {
        if (inventory == null)
            return;

        if (playerInputContext != null && playerInputContext.TryGetDirectSlotSelection(out int directSlot))
        {
            inventory.SetActiveSlot(directSlot);
            return;
        }

        if (HasMultiplayerContext())
            return;

        if (Input.GetKeyDown(KeyCode.Alpha1)) inventory.SetActiveSlot(0);
        if (Input.GetKeyDown(KeyCode.Alpha2)) inventory.SetActiveSlot(1);
        if (Input.GetKeyDown(KeyCode.Alpha3)) inventory.SetActiveSlot(2);
        if (Input.GetKeyDown(KeyCode.Alpha4)) inventory.SetActiveSlot(3);
        if (Input.GetKeyDown(KeyCode.Alpha5)) inventory.SetActiveSlot(4);
    }

    private void ScrollInventorySlotCheck()
    {
        if (inventory == null)
            return;

        if (playerInputContext != null && !playerInputContext.IsKeyboardMouse)
        {
            if (playerInputContext.CyclePreviousPressedThisFrame)
                inventory.SetActiveSlot(inventory.activeSlotIndex - 1);
            else if (playerInputContext.CycleNextPressedThisFrame)
                inventory.SetActiveSlot(inventory.activeSlotIndex + 1);

            return;
        }

        if (HasMultiplayerContext())
            return;

        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f)
            inventory.SetActiveSlot(inventory.activeSlotIndex - 1);
        else if (scroll < 0f)
            inventory.SetActiveSlot(inventory.activeSlotIndex + 1);
    }

    private bool HasMultiplayerContext()
    {
        return GetComponent<MultijugadorPlayerContext>() != null;
    }

    public void ApplySlow(float multiplier, float duration)
    {
        StartCoroutine(ApplySlowRoutine(multiplier, duration));
    }

    public void ApplyFreeze(float duration)
    {
        StartCoroutine(ApplyFreezeRoutine(duration));
    }

    public void ClearInventoryFromSpell()
    {
        if (inventory != null)
            inventory.Clear();
    }

    public void PlaySpellCastAnimation()
    {
        if (animator == null)
            return;

        if (attackAnimationRoutine != null)
            StopCoroutine(attackAnimationRoutine);

        attackAnimationRoutine = StartCoroutine(PlayAttackAnimationRoutine());
    }

    private IEnumerator PlayAttackAnimationRoutine()
    {
        if (animator != null)
            animator.SetBool(attackAnimationBoolName, true);

        yield return new WaitForSeconds(attackAnimationHoldTime);

        if (animator != null)
            animator.SetBool(attackAnimationBoolName, false);

        attackAnimationRoutine = null;
    }

    private IEnumerator ApplySlowRoutine(float multiplier, float duration)
    {
        float originalSpeed = speed;
        speed = originalSpeed * Mathf.Clamp(multiplier, 0.05f, 1f);
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
    }

    private IEnumerator ApplyFreezeRoutine(float duration)
    {
        float originalSpeed = speed;
        speed = 0f;
        yield return new WaitForSeconds(duration);
        speed = originalSpeed;
    }

    private void ConfigurarAudioPasos()
    {
        if (audioPasosCaminar == null)
            audioPasosCaminar = gameObject.AddComponent<AudioSource>();

        if (audioPasosCorrer == null)
            audioPasosCorrer = gameObject.AddComponent<AudioSource>();

        audioPasosCaminar.playOnAwake = false;
        audioPasosCaminar.loop = true;
        audioPasosCaminar.spatialBlend = 0f;
        audioPasosCaminar.clip = sonidoCaminar;
        audioPasosCaminar.volume = volumenCaminar;

        audioPasosCorrer.playOnAwake = false;
        audioPasosCorrer.loop = true;
        audioPasosCorrer.spatialBlend = 0f;
        audioPasosCorrer.clip = sonidoCorrer;
        audioPasosCorrer.volume = volumenCorrer;
    }

    private void ActualizarAudioPasos(bool estaMoviendose)
    {
        if (!estaMoviendose)
        {
            pasoTimer = 0f;
            if (audioPasosCaminar != null && audioPasosCaminar.isPlaying)
                audioPasosCaminar.Stop();

            if (audioPasosCorrer != null && audioPasosCorrer.isPlaying)
                audioPasosCorrer.Stop();

            return;
        }

        pasoTimer += Time.deltaTime;

        if (isSprinting)
        {
            if (audioPasosCaminar != null && audioPasosCaminar.isPlaying)
                audioPasosCaminar.Stop();

            if (pasoTimer >= correrIntervalo)
            {
                pasoTimer = 0f;
                SoundManager.PlaySound(SoundType.Correr, volumenCorrer);
            }
        }
        else
        {
            if (audioPasosCorrer != null && audioPasosCorrer.isPlaying)
                audioPasosCorrer.Stop();

            if (pasoTimer >= caminarIntervalo)
            {
                pasoTimer = 0f;
                SoundManager.PlaySound(SoundType.Footstep, volumenCaminar);
            }
        }
    }
}
