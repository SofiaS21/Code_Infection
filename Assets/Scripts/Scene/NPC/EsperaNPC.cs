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

    public float velocidadTexto = 0.05f;

    private bool esElPrimero = true;
    private bool dniMostrado = false;
    private bool pacienteHablando = false;
    private Coroutine corrutinaOcultarDialogo;
    private Coroutine corrutinaEscribiendo;


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

    public void MostrarDNI(Pacientes quien)
    {
        if (currentVisitor == null || quien != currentVisitor) return;
        if (!currentVisitor.PuedeInteractuar) return;
        if (pacienteHablando) return;

        panelDialogue.SetActive(true);

        if (!dniMostrado)
        {
            dniMostrado = true;
            tarjetaDNI.SetActive(true);
            inventario.SetActive(false);
            Decir("Hola, mi nombre es " + currentVisitor.nombre);
        }

        else
        {
            inventario.SetActive(false);
            Decir("Ehmm.. ya te dije que mi nombre es " + currentVisitor.nombre);
        }

        if (!currentVisitor.llegoAlCounter)
            dniMostrado = true;

        if (corrutinaOcultarDialogo != null)
            StopCoroutine(corrutinaOcultarDialogo);

        // Oculta diálogo después de 5 segundos
        corrutinaOcultarDialogo = StartCoroutine(OcultarDialogoDespuesDe(5f));
    }

    void Decir(string texto)
    {
        //Textos no se superponen
        if (corrutinaEscribiendo != null) StopCoroutine(corrutinaEscribiendo);
        corrutinaEscribiendo = StartCoroutine(EscribirTexto(texto));
    }

    IEnumerator OcultarDialogoDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        panelDialogue.SetActive(false);
        inventario.SetActive(true);
    }

    IEnumerator EscribirTexto(string textoCompleto)
    {
        pacienteHablando = true;

        dialogueText.text = textoCompleto;
        dialogueText.maxVisibleCharacters = 0; 

        for (int i = 1; i <= textoCompleto.Length; i++)
        {
            dialogueText.maxVisibleCharacters = i;
            yield return new WaitForSeconds(velocidadTexto);
        }

        pacienteHablando = false;
    }

    IEnumerator OcultarDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        panelDialogue.SetActive(false);
        inventario.SetActive(true);
    }
}