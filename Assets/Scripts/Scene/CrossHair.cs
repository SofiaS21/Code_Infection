using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CrossHair : MonoBehaviour
{
    public GameObject crossHair;
    public GameObject crossHairInt;
    public GameObject Camera;
    public bool isInteracting;
    private RectTransform crossHairIntRect;
    private float currentSize = 4;

    private const float maxSize = 6;
    private const float growSpeed = 15;

    private IInteractable objetoActual; // <- nuevo: guarda a qué objeto le estamos apuntando

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        crossHairIntRect = crossHairInt.GetComponent<RectTransform>();
        crossHair.SetActive(true);
    }

    void Update()
    {
        RaycastHit hit;
        if (Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, isInteracting ? 5.5f : 5f)
            && hit.collider.CompareTag("Interactuable"))
        {
            crossHairInt.SetActive(true);
            crossHair.SetActive(false);
            isInteracting = true;
            currentSize = Mathf.MoveTowards(currentSize, maxSize, growSpeed * Time.deltaTime);
            crossHairIntRect.sizeDelta = new Vector2(currentSize, currentSize);

            // --- lógica nueva de interacción ---
            IInteractable interactuable = hit.collider.GetComponent<IInteractable>();

            if (interactuable != objetoActual)
            {
                objetoActual?.OnUnfocus();
                objetoActual = interactuable;
                objetoActual?.OnFocus();
            }

            if (Input.GetKeyDown(KeyCode.E))
                objetoActual?.Interact();
        }
        else
        {
            crossHairInt.SetActive(false);
            crossHair.SetActive(true);
            isInteracting = false;
            currentSize = 4f;

            // dejamos de apuntar a lo que sea que teníamos
            if (objetoActual != null)
            {
                objetoActual.OnUnfocus();
                objetoActual = null;
            }
        }
    }
}