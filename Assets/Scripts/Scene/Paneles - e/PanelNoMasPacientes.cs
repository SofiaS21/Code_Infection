using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelNoMasPacientes : MonoBehaviour
{
    [Header("Scripts a desactivar mientras el panel está abierto")]
    public MonoBehaviour fpsController;   
    public MonoBehaviour crosshair;     

    [Header("Panel a cerrar con la cruz")]
    public GameObject panel;

    void OnEnable()
    {
        if (fpsController != null) fpsController.enabled = false;
        if (crosshair != null) crosshair.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void OnDisable()
    {
        if (fpsController != null) fpsController.enabled = true;
        if (crosshair != null) crosshair.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void Cerrar()
    {
        if (panel != null) panel.SetActive(false);
    }
}
