using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class EsperaNPC : MonoBehaviour
{
    public List<Pacientes> patientList;
    private Queue<Pacientes> pacienteEspera = new Queue<Pacientes>();
    private Pacientes currentVisitor;
    public TMP_Text dialogueText;

    void Start()
    {
        foreach (var patient in patientList)
        {
            pacienteEspera.Enqueue(patient);
        }

        ProximoPaciente();
    }

    public void ProximoPaciente()
    {
        if (pacienteEspera.Count == 0)
        {
            currentVisitor = null;
            dialogueText.text = "No hay mas pacientes por hoy. Termina de curar a los pacientes para comenzar el proximo dia.";
        }
        else
        {
            currentVisitor = pacienteEspera.Dequeue();
        }
    }

    public void DenegarAcceso()
    {
        if (currentVisitor == null) return;
        ProximoPaciente();
    }

    public void AceptarAcceso()
    {
        if (currentVisitor == null) return;
        ProximoPaciente();
    }
}
