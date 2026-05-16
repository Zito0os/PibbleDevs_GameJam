using UnityEngine;
using System.Collections;
using System.Collections.Generic;


public class Cameralook : MonoBehaviour
{

    public float sensitivity = 80f;
    //posicion del player
    public Transform playerBody;

    float xRotation = 0f;


    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        Debug.Log("Juego iniciado");
        Cursor.lockState = CursorLockMode.Locked; // esto bloquea el cursor en el centro de la pantalla
    }

    // Update is called once per frame
    void Update()
    {
        // Si el EmotePanel está abierto, no procesar input de cámara
        //if (EmotePanel.isEmotePanelActive)
        //    return;

        // esto guarda la rotacion del mouse en el eje X y Y
        float mouseX = Input.GetAxis("Mouse X") * sensitivity * Time.deltaTime;


        float mouseY = Input.GetAxis("Mouse Y") * sensitivity * Time.deltaTime;

        xRotation -= mouseY;

        xRotation = Mathf.Clamp(xRotation, -90f, 90f);//minimo y maximo de la rotacion de la camara en el eje X

        transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);// esto hace que la camara gire en el eje X, y se le asigna a la camara la rotacion en el eje X, Y y Z



        //el rotate hace que la camara gire en el eje X
        playerBody.Rotate(Vector3.up * mouseX);



    }
}