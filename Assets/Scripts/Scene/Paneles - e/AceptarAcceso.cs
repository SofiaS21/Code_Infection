using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AceptarAcceso : MonoBehaviour
{

    public EsperaNPC esperaNPC;
    public GameObject panelAceptar;

    void Update()
    {

        if (panelAceptar.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            esperaNPC.AceptarAcceso();
            Debug.Log("Paciente acceptado");
        }
    }
}

