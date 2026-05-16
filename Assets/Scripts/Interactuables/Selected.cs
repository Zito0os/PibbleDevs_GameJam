using UnityEngine;

public class Selected : MonoBehaviour
{
    LayerMask mask;
    public float distancia = 15.0f;

    public Texture2D puntero;
    public GameObject TextDetect;
    GameObject ultimoReconocido = null;
    void Start()
    {
        mask = LayerMask.GetMask("RayCastDetect");
        TextDetect.SetActive(false);
    }

    void Update()
    {
        RaycastHit hit;

        if (Physics.Raycast(transform.position, transform.TransformDirection(Vector3.forward), out hit, distancia, mask))
        {
            Deselect();
            SelectedObject(hit.transform);
            
            // Check for chest interaction
            if (hit.collider.tag == "Cofre")
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    hit.collider.GetComponent<ChestController>().AbrirCofre();
                    hit.collider.GetComponent<ChestController>().OnAfterAbrirCofre();
                }
            }
            // Check for item pickup
            else if (hit.collider.tag == "Item")
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    ItemPickup itemPickup = hit.collider.GetComponent<ItemPickup>();
                    if (itemPickup != null)
                    {
                        PlayerMovement player = GetComponentInParent<PlayerMovement>();
                        if (player == null)
                            player = GetComponent<PlayerMovement>();
                        
                        if (player != null)
                        {
                            itemPickup.PickUp(player);
                        }
                        else
                        {
                            Debug.LogWarning("Selected: no se encontró PlayerMovement para recoger item");
                        }
                    }
                }
            }
            else if (hit.collider.tag == "Puerta")
            {
                Door_Controller doorController = hit.collider.GetComponentInParent<Door_Controller>();
                bool esPuerta = hit.collider.CompareTag("Puerta") || (doorController != null && doorController.CompareTag("Puerta"));

                if (esPuerta && Input.GetKeyDown(KeyCode.E))
                {

                    if (doorController != null)
                    {
                        doorController.AbrirCofre();
                        doorController.OnAfterAbrirCofre();
                    }
                }
            }

            //Debug.DrawRay(transform.position, transform.TransformDirection(Vector3.forward) * hit.distance, Color.red);
        }
        else
        {
            Deselect();
        }
    }

    void SelectedObject(Transform transform)
    {
        //transform.GetComponent<Renderer>().material.color = Color.green;
        ultimoReconocido = transform.gameObject;
    }

    void Deselect()
    {
        if (ultimoReconocido != null)
        {
            //ultimoReconocido.GetComponent<Renderer>().material.color = Color.white;
            ultimoReconocido = null;
        }
    }

    void OnGUI()
    {
        GUILayout.BeginHorizontal();
        GUILayout.BeginVertical();
        //Nuevo puntero
        //Rect rect = new Rect((Screen.width - puntero.width) / 2, (Screen.height - puntero.height) / 2, puntero.width, puntero.height);
        //GUI.DrawTexture(rect, puntero);

        //Rect rect = new Rect(Screen.width / 2, Screen.height / 2, puntero.width, puntero.height);
        //GUI.DrawTexture(rect, puntero);
        GUILayout.EndVertical();
        GUILayout.EndHorizontal();


        

        if (ultimoReconocido)
        {
            TextDetect.SetActive(true);
        }
        else
        {
            TextDetect.SetActive(false);
        }
    }
}
