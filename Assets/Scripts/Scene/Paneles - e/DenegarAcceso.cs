using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DenegarAcceso : MonoBehaviour
{
    public EsperaNPC esperaNPC;
    public GameObject panelRechazar;

    void Update()   
    {
        if (panelRechazar.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            esperaNPC.DenegarAcceso();
            Debug.Log("Paciente rechazado");
        }
    }
}
