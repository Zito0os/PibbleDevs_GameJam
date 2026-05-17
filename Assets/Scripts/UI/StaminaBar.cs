using UnityEngine;
using UnityEngine.UI;
using System.Collections;




public class StaminaBar : MonoBehaviour
{

    public Slider staminaSlider;

    public float maxStamina = 100;

    private float currentStamina;

    private float regenerateStaminaTime = 0.1f;
    private float regeneratesAmount = 2;

    private float losingStaminaTime = 0.1f;

    private Coroutine myCoroutineRegenerate;
    private PlayerMovement ownerPlayer;

    public bool HasStamina => currentStamina > 0f;
    public float CurrentStamina => currentStamina;


    void Start()
    {
        if (ownerPlayer == null)
        {
            ownerPlayer = GetComponentInParent<PlayerMovement>();
            if (ownerPlayer == null)
                ownerPlayer = FindFirstObjectByType<PlayerMovement>();
        }

        currentStamina = maxStamina;
        staminaSlider.maxValue = maxStamina;
        staminaSlider.value = maxStamina;
    }

    public void ConfigureForPlayer(PlayerMovement targetPlayer)
    {
        ownerPlayer = targetPlayer;
    }


    public void UseStamina(float amount)
    {
        if (amount <= 0f)
            return;

        if (myCoroutineRegenerate != null)
        {
            StopCoroutine(myCoroutineRegenerate);
            myCoroutineRegenerate = null;
        }

        currentStamina = Mathf.Max(currentStamina - amount, 0f);

        if (staminaSlider != null)
            staminaSlider.value = currentStamina;

        if (currentStamina <= 0f)
        {
            Debug.Log("No hay stamina");
            if (ownerPlayer != null)
                ownerPlayer.isSprinting = false;

            if (myCoroutineRegenerate != null)
                StopCoroutine(myCoroutineRegenerate);

            myCoroutineRegenerate = StartCoroutine(RegenerateStaminaCoroutineStart());
        }
    }

    public void StopSprinting()
    {
        //Iniciar regeneraci�n cuando se detiene el sprint
        if (myCoroutineRegenerate != null)
        {
            StopCoroutine(myCoroutineRegenerate);
        }
        myCoroutineRegenerate = StartCoroutine(RegenerateStaminaCoroutineStart());
    }

    private IEnumerator RegenerateStaminaCoroutineStart()
    {
        //esperar 1 segundo antes de empezar a regenerar stamina
        yield return new WaitForSeconds(1);

        while (currentStamina < maxStamina)
        {
            //darle la stamina poco a poco
            currentStamina += regeneratesAmount;
            currentStamina = Mathf.Min(currentStamina, maxStamina); //No superar el m�ximo
            //ponerle el valor a la barra de stamina 
            staminaSlider.value = currentStamina;

            yield return new WaitForSeconds(regenerateStaminaTime);

        }
        myCoroutineRegenerate = null;

    }


}