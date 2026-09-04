using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class DniMostrar : MonoBehaviour , IInteractable
{
    public GameObject dniUI;

    public void Interact()
    {
        dniUI.SetActive(true);
        
        Debug.Log("Mostrar DNI");
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        // opcional: si tenés un script de movimiento de jugador, desactivalo acá
        // jugador.puedeMoverse = false;
    }

    public void OnFocus()
    {
        // vacío, o para resaltar el objeto (ej: cambiar material, activar un outline)
    }

    public void OnUnfocus()
    {
        // vacío, o para sacar el resaltado
    }
}
