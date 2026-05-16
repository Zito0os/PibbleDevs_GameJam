using UnityEngine;

public class SingleHechizo : MonoBehaviour
{
    public float lifetime = 5f; // Tiempo de vida del hechizo en segundos
    private void Awake()
    {
        // Destruir el hechizo después de su tiempo de vida
        Destroy(gameObject, lifetime);
    }

    private void OnCollisionEnter(Collision collision)
    {
        
        Destroy(gameObject);
    }
}

