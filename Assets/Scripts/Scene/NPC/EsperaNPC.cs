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

    private bool esElPrimero = true;
    private bool dniMostrado = false;
    private Coroutine corrutinaOcultarDialogo;

    void Start()
    {
        foreach (var patient in patientList)
            pacienteEspera.Enqueue(patient);

        anim = GetComponent<Animator>();

        panelDialogue.SetActive(false);
        ProximoPaciente();
    }

    public void DenegarAcceso()
    {
        if (currentVisitor == null || !currentVisitor.llegoAlCounter) return;

        tarjetaDNI.SetActive(false);
        currentVisitor.Rechazar();
        ProximoPaciente();
    }

    public void AceptarAcceso()
    {
        if (currentVisitor == null || !currentVisitor.llegoAlCounter) return;

        tarjetaDNI.SetActive(false);
        currentVisitor.Aceptar();
        ProximoPaciente();
    }

    public void ProximoPaciente()
    {
        StopAllCoroutines();

        if (pacienteEspera.Count == 0)
        {
            currentVisitor = null;
            panelDialogue.SetActive(true);
            inventario.SetActive(false);
            dialogueText.text = "No hay mas pacientes por hoy. Termina de curar a los pacientes para comenzar el proximo dia.";

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
        inventario.SetActive(false);
        panelDialogue.SetActive(true);

        if (!esElPrimero)
            yield return new WaitForSeconds(tiempoEntrePacientes);

        esElPrimero = false;
        dniMostrado = false;

        currentVisitor = pacienteEspera.Dequeue();
        currentVisitor.GenerarDatos();
        currentVisitor.gameObject.SetActive(true);
    }

    public void MostrarDNI()
    {
        if (currentVisitor == null) return;

        if (!currentVisitor.llegoAlCounter)
            dniMostrado = true;

        tarjetaDNI.SetActive(true);
        panelDialogue.SetActive(true);
        inventario.SetActive(false);
        dialogueText.text = "Hola, mi nombre es " + currentVisitor.nombre;

        // Si ya hay un conteo en marcha (ej: presionaste MostrarDNI de nuevo), lo reinicia
        if (corrutinaOcultarDialogo != null)
            StopCoroutine(corrutinaOcultarDialogo);

        // Oculta únicamente el diálogo después de 5 segundos
        corrutinaOcultarDialogo = StartCoroutine(OcultarDialogoDespuesDe(5f));
    }

    IEnumerator OcultarDialogoDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        panelDialogue.SetActive(false);
        inventario.SetActive(true);
    }

    IEnumerator OcultarDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        panelDialogue.SetActive(false);
        inventario.SetActive(true);
    }
}