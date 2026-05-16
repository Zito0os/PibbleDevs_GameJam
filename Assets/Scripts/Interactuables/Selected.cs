using UnityEngine;
using TMPro;

public class Selected : MonoBehaviour
{
    LayerMask mask;
    public float distancia = 15.0f;

    public Texture2D puntero;
    public GameObject TextDetect;
    GameObject ultimoReconocido = null;

    // TMP widget inside the "Interactuable-Widget" empty object
    private TextMeshProUGUI interactableWidgetText;
    private bool? cachedWidgetActive = null;
    void Start()
    {
        mask = LayerMask.GetMask("RayCastDetect");
        if (TextDetect != null)
            TextDetect.SetActive(false);

        GameObject widget = GameObject.Find("Interactuable-Widget");
        if (widget != null)
        {
            interactableWidgetText = widget.GetComponentInChildren<TextMeshProUGUI>(true);
        }
    }

    void Update()
    {
        bool running = DoorWheelMinigame.IsRunning;

        if (running)
        {
            Deselect();
            if (TextDetect != null)
                TextDetect.SetActive(false);

            if (interactableWidgetText != null)
            {
                if (!cachedWidgetActive.HasValue)
                    cachedWidgetActive = interactableWidgetText.gameObject.activeSelf;

                interactableWidgetText.gameObject.SetActive(false);
            }

            return;
        }

        // restore widget state when minigame finished
        if (cachedWidgetActive.HasValue)
        {
            if (interactableWidgetText != null)
                interactableWidgetText.gameObject.SetActive(cachedWidgetActive.Value);

            cachedWidgetActive = null;
        }

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
                    PlayerMovement player = GetComponentInParent<PlayerMovement>();
                    if (player == null)
                        player = GetComponent<PlayerMovement>();

                    if (doorController != null)
                    {
                        doorController.AbrirCofre(player);
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
