using UnityEngine;

public class Selected : MonoBehaviour
{
    LayerMask mask;
    public float distancia = 15.0f;

    [Header("Debug")]
    public bool debugInteraccionPuertas = true;

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

            if (hit.collider.CompareTag("Cofre"))
            {
                if (Input.GetKeyDown(KeyCode.E))
                {
                    ChestController chestController = hit.collider.GetComponentInParent<ChestController>();
                    if (debugInteraccionPuertas)
                    {
                        Debug.Log("[Selected] E en Cofre -> hit=" + hit.collider.name + " | parent=" + hit.collider.transform.root.name + " | chestController=" + (chestController != null));
                    }

                    if (chestController != null)
                    {
                        chestController.AbrirCofre();
                        chestController.OnAfterAbrirCofre();
                    }
                }
            }
            else
            {
                Door_Controller doorController = hit.collider.GetComponentInParent<Door_Controller>();
                bool esPuerta = hit.collider.CompareTag("Puerta") || (doorController != null && doorController.CompareTag("Puerta"));

                if (esPuerta && Input.GetKeyDown(KeyCode.E))
                {
                    if (debugInteraccionPuertas)
                    {
                        Debug.Log("[Selected] E en Puerta -> hit=" + hit.collider.name + " | tagHit=" + hit.collider.tag + " | doorController=" + (doorController != null) + " | objetoDoor=" + (doorController != null ? doorController.name : "null"));
                    }

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
