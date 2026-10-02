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
    public GameObject panelDialogue;
    public GameObject inventario;
    public GameObject tarjetaDNI;
    public Animator anim;
    public float tiempoMensajeVisible = 5f;

    public float tiempoEntrePacientes = 10f;
    public float velocidadCaminar = 3.5f;

    void Start()
    {
        foreach (var patient in patientList)
        {
            pacienteEspera.Enqueue(patient);
        }

        anim = GetComponent<Animator>();

        panelDialogue.gameObject.SetActive(false);
        ProximoPaciente();
    }

    public void DenegarAcceso()
    {
        if (currentVisitor == null || !currentVisitor.llegoAlCounter) return;
        tarjetaDNI.gameObject.SetActive(false);
        currentVisitor.Rechazar();
        ProximoPaciente();

    }

    public void AceptarAcceso()
    {
        if (currentVisitor == null || !currentVisitor.llegoAlCounter) return;
        tarjetaDNI.gameObject.SetActive(false);
        currentVisitor.Aceptar();
        ProximoPaciente();
    }

    public void ProximoPaciente()
    {
        if (pacienteEspera.Count == 0)
        {
            panelDialogue.SetActive(true);
            inventario.SetActive(false);

            currentVisitor = null;
            dialogueText.text = "No hay mas pacientes por hoy. Termina de curar a los pacientes para comenzar el proximo dia.";

            StopAllCoroutines();
            StartCoroutine(OcultarDespuesDe(tiempoMensajeVisible));
        }

        else
        {
            StartCoroutine(SiguientePaciente());
        }
    }

    IEnumerator SiguientePaciente()
    {
        currentVisitor = null; 
        panelDialogue.SetActive(false);
        inventario.SetActive(true);

        yield return new WaitForSeconds(tiempoEntrePacientes);

        currentVisitor = pacienteEspera.Dequeue();
        currentVisitor.GenerarDatos();
        currentVisitor.gameObject.SetActive(true);
    }

    IEnumerator OcultarDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        panelDialogue.SetActive(false);
        inventario.SetActive(true);

    }
}

