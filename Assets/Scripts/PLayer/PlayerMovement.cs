using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class PlayerMovement : MonoBehaviour
{

    public CharacterController characterController;
    public float speed = 15f;

    public float gravity = -3f;

    //gravedad velocidad
    Vector3 velocity;




    //comprobacion de suelo
    public Transform groundCheck;

    public float sphereRadius = 0.3f;

    //etiqueta para el suelo, para que el player sepa si esta en el suelo o no
    public LayerMask groundMask;

    bool isGrounded;



    //salto
    public float jumpheigth = 3f;


    public bool isSprinting;

    public float sprintSpeedMultiplier = 2f;

    private float sprintSpeed = 1;


    public float staminaUseAmount = 5f;

    private StaminaBar staminaSlider;
    private Inventory inventory;


    //public Animator animator;

    [Header("Audio pasos")]
    public AudioSource audioPasosCaminar;
    public AudioSource audioPasosCorrer;
    public AudioClip sonidoCaminar;
    public AudioClip sonidoCorrer;
    [Range(0f, 1f)] public float volumenCaminar = 0.7f;
    [Range(0f, 1f)] public float volumenCorrer = 0.85f;



    void Start()
    {
        //encontrar la slider de stamina en la escena
        //como tiene el script stamina bar , lo busca y lo asigna a la variable
        staminaSlider = FindFirstObjectByType<StaminaBar>();
        inventory = GetComponent<Inventory>();

        if (inventory == null)
        {
            inventory = FindFirstObjectByType<Inventory>();
        }

        ConfigurarAudioPasos();
    }


    void Update()
    {
        if (DoorWheelMinigame.IsRunning)
            return;

        //// Si el EmotePanel est� abierto, no procesar input de movimiento
        //if (EmotePanel.isEmotePanelActive)
        //{
        //    // Mantener la gravedad aunque no se pueda mover
        //    velocity.y += gravity * Time.deltaTime;
        //    characterController.Move(velocity * Time.deltaTime);
        //    return;
        //}

        //esto es para saber si el player esta en el suelo o no mediante una funcion de unity
        //CheckSphere crea una esfera en el punto que le digamos, en este caso groundCheck.position
        isGrounded = Physics.CheckSphere(groundCheck.position, sphereRadius, groundMask);

        if (isGrounded && velocity.y < 0)
        {
            //si el player esta en el suelo, la velocidad en y se pone a 0
            velocity.y = -2f;
        }



        //esto es para el movimiento del player asignacion de teclas de movimiento
        float x = Input.GetAxis("Horizontal");

        float z = Input.GetAxis("Vertical");

        bool estaMoviendose = Mathf.Abs(x) > 0.01f || Mathf.Abs(z) > 0.01f;

        //animator.SetFloat("VelX", x);
        //animator.SetFloat("VelZ", z);
        //animator.SetBool("isSprinting", isSprinting);


        //esto es para mover al jugador adelante o hacia atras 
        Vector3 move = transform.right * x + transform.forward * z;


        JunpCheck();
        RunCheck();
        SelectInventorySlotCheck();
        ScrollInventorySlotCheck();
        UseItemCheck();
        ActualizarAudioPasos(estaMoviendose);



        //esto le asigna el movimiento al caracter controler del player 
        //y le asigna la velocidad que se le dio en el inspector
        characterController.Move(move * speed * Time.deltaTime * sprintSpeed);

        // si alguien juega a 30 y alguien a 60 fps, el que juega a 30 fps se movera mas lento
        //por eso se multiplica por Time.deltaTime



        //para activarle la gravedad 
        velocity.y += gravity * Time.deltaTime;
        //esto le asigna la gravedad al player
        characterController.Move(velocity * Time.deltaTime);




    }
    public void JunpCheck()
    {

        //salto

        if (Input.GetKeyDown(KeyCode.Space) && isGrounded)
        {

            //sqrt raiz cuadrada
            velocity.y = Mathf.Sqrt(jumpheigth * -2f * gravity);
            //animator.SetBool("isJumping", true);
        }
        //para que cuando caiga no se quede el bool en true
        if (!isGrounded)
        {
            //animator.SetBool("isJumping", false);
        }
    }

    public void RunCheck()
    {

        // Hold LeftShift to sprint, release to stop. Consume stamina continuously while held.
        bool holdSprint = Input.GetKey(KeyCode.LeftShift);

        if (holdSprint && !isSprinting)
        {
            isSprinting = true;
        }

        if (!holdSprint && isSprinting)
        {
            isSprinting = false;
            if (staminaSlider != null)
                staminaSlider.StopSprinting();
        }

        if (isSprinting)
        {
            sprintSpeed = sprintSpeedMultiplier;
            if (staminaSlider != null)
            {
                // consume per second
                staminaSlider.UseStamina(staminaUseAmount * Time.deltaTime);
            }
        }
        else
        {
            sprintSpeed = 1;
        }
    }

    private void UseItemCheck()
    {
        if (!Input.GetMouseButtonDown(0))
            return;

        if (inventory == null)
            return;

        if (!EsHechizoInventario(inventory.activeItemType))
            return;

        bool used = inventory.UseSelectedItem();
        if (used)
        {
            Debug.Log($"PlayerMovement: item usado con click izquierdo desde slot {inventory.activeSlotIndex + 1}.");
        }
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

        float scroll = Input.mouseScrollDelta.y;
        if (scroll > 0f)
        {
            inventory.SetActiveSlot(inventory.activeSlotIndex - 1);
        }
        else if (scroll < 0f)
        {
            inventory.SetActiveSlot(inventory.activeSlotIndex + 1);
        }
    }


    public void Barrido()
    {


    }

    private void ConfigurarAudioPasos()
    {
        if (audioPasosCaminar == null)
        {
            audioPasosCaminar = gameObject.AddComponent<AudioSource>();
        }

        if (audioPasosCorrer == null)
        {
            audioPasosCorrer = gameObject.AddComponent<AudioSource>();
        }

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
            if (audioPasosCaminar != null && audioPasosCaminar.isPlaying)
                audioPasosCaminar.Stop();

            if (audioPasosCorrer != null && audioPasosCorrer.isPlaying)
                audioPasosCorrer.Stop();

            return;
        }

        if (isSprinting)
        {
            if (audioPasosCaminar != null && audioPasosCaminar.isPlaying)
                audioPasosCaminar.Stop();

            if (audioPasosCorrer != null)
            {
                audioPasosCorrer.clip = sonidoCorrer;
                audioPasosCorrer.volume = volumenCorrer;

                if (sonidoCorrer != null && !audioPasosCorrer.isPlaying)
                    audioPasosCorrer.Play();
            }
        }
        else
        {
            if (audioPasosCorrer != null && audioPasosCorrer.isPlaying)
                audioPasosCorrer.Stop();

            if (audioPasosCaminar != null)
            {
                audioPasosCaminar.clip = sonidoCaminar;
                audioPasosCaminar.volume = volumenCaminar;

                if (sonidoCaminar != null && !audioPasosCaminar.isPlaying)
                    audioPasosCaminar.Play();
            }
        }
    }



}
