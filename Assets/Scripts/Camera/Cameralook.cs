using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Cameralook : MonoBehaviour
{

    [Header("Sensitivity")]
    public float mouseSensitivity = 80f;
    public float gamepadSensitivity = 80f;
    //posicion del player
    public Transform playerBody;
    private MultijugadorPlayerContext playerInputContext;

    float xRotation = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Juego iniciado");
        Cursor.lockState = CursorLockMode.Locked; // esto bloquea el cursor en el centro de la pantalla
        RefreshInputContext();
    }

    public void SetInputContext(MultijugadorPlayerContext inputContext)
    {
        playerInputContext = inputContext;
    }

    private void RefreshInputContext()
    {
        if (playerInputContext != null)
            return;

        playerInputContext = GetComponentInParent<MultijugadorPlayerContext>();
        if (playerInputContext == null)
            playerInputContext = GetComponent<MultijugadorPlayerContext>();
    }

    // Update is called once per frame
    void Update()
    {
        if (DoorWheelMinigame.IsRunning)
            return;

        RefreshInputContext();

        // Si el EmotePanel est� abierto, no procesar input de c�mara
        //if (EmotePanel.isEmotePanelActive)
        //    return;

        // esto guarda la rotacion del mouse en el eje X y Y
        Vector2 lookInput = playerInputContext != null ? playerInputContext.Look : Vector2.zero;
        float sensitivity = GetCurrentSensitivity();
        float mouseX = lookInput.x * sensitivity * Time.deltaTime;


        float mouseY = lookInput.y * sensitivity * Time.deltaTime;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);//minimo y maximo de la rotacion de la camara en el eje X

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);// esto hace que la camara gire en el eje X, y se le asigna a la camara la rotacion en el eje X, Y y Z



        //el rotate hace que la camara gire en el eje X
        playerBody.Rotate(Vector3.up * mouseX);



    }

    private float GetCurrentSensitivity()
    {
        if (playerInputContext != null && playerInputContext.IsGamepad)
            return gamepadSensitivity;

        return mouseSensitivity;
    }
}