using UnityEngine;

public class DisparoHechizo : MonoBehaviour
{
    public Transform HechizoSpawnPoint;
    public GameObject HechizoPrefab;
    public float HechizoSpeed = 10f;

    private void Update()
    {
        //Disparar con el bot�n izquierdo del mouse
        if (Input.GetMouseButtonDown(0))
        {
            var hechizo = Instantiate(HechizoPrefab, HechizoSpawnPoint.position, HechizoSpawnPoint.rotation);
            hechizo.GetComponent<Rigidbody>().linearVelocity = HechizoSpawnPoint.forward * HechizoSpeed;

        }
    }
}

