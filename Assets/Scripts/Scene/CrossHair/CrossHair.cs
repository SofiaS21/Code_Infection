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

    [Header("Alcance")]
    public float alcancePacientes = 15f;   
    public float alcanceNormal = 5.5f;

    private RectTransform crossHairIntRect;
    private float currentSize = 4;

    private const float maxSize = 6;
    private const float growSpeed = 15;

    private IInteractable objetoActual;

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
        bool pego = Physics.Raycast(Camera.transform.position, Camera.transform.forward, out hit, alcancePacientes, ~0, QueryTriggerInteraction.Collide);

        bool valido = false;
        if (pego)
        {
            if (hit.collider.CompareTag("Npcs"))
            {
                valido = true;   //de lejos
            }
            else if (hit.distance <= alcanceNormal &&
                    (hit.collider.CompareTag("Interactuable") || hit.collider.CompareTag("Collectable")))
            {
                valido = true;   //solo de cerca
            }
        }

        if (valido)
        {
            crossHairInt.SetActive(true);
            crossHair.SetActive(false);
            isInteracting = true;
            currentSize = Mathf.MoveTowards(currentSize, maxSize, growSpeed * Time.deltaTime);
            crossHairIntRect.sizeDelta = new Vector2(currentSize, currentSize);

            IInteractable interactuable = hit.collider.GetComponentInParent<IInteractable>();

            if (interactuable != objetoActual)
            {
                objetoActual?.OnUnfocus();
                objetoActual = interactuable;
                objetoActual?.OnFocus();
            }

            if (Input.GetMouseButtonDown(0) || Input.GetKeyDown(KeyCode.E))
            {
                objetoActual?.Interact();
            }
        }
        else
        {
            crossHairInt.SetActive(false);
            crossHair.SetActive(true);
            isInteracting = false;
            currentSize = 4f;

            if (objetoActual != null)
            {
                objetoActual.OnUnfocus();
                objetoActual = null;
            }
        }
    }
}