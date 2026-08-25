using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EsperaNPC : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        foreach (var patient in patientList)
        {
            pacienteEspera.Enqueue(patient);
        }
        
        LoadNextPatient();
    }

    public void ProximoPaciente();
    
    if (pacienteEspera.Count = 0)
        {
        currentVisitor = pacienteEspera.Dequeue();
        }

    else
        {
            dialogueText.text = "No hay más pacientes por hoy.";
        }
    

    public void DenegarAcceso()
    {

    }

    public void AcceptarAcceso()
    {

    }


    // Update is called once per frame
    void Update()
    {
        
    }
}
