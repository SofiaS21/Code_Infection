using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DniMostrar : MonoBehaviour , IInteractable
{
    public GameObject dniUI;

    [Header("Cierre automático al alejarse")]
    public Transform jugador;       // arrastrá tu FPSController acá
    public float distanciaMaxima = 3f;

    void Update()
    {
        if (dniUI != null && dniUI.activeSelf && jugador != null)
        {
            float distancia = Vector3.Distance(jugador.position, transform.position);
            if (distancia > distanciaMaxima)
                Cerrar();
        }
    }

    public void Interact()
    {
        if (dniUI.activeSelf)
            Cerrar();
        else
            Abrir();
    }

    void Abrir()
    {
        dniUI.SetActive(true);
        Debug.Log("Mostrar DNI");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

    }

    void Cerrar()
    {
        dniUI.SetActive(false);
        Debug.Log("Ocultar DNI");
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

    }

    public void OnFocus() { }
    public void OnUnfocus() { }
}
