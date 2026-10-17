using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.AI;
using UnityEngine.UI;


public class EsperaNPC : MonoBehaviour
{

    [Header("Spawn")]
    public Pacientes[] prefabsPacientes;
    public Transform puntoSpawn;

    [System.Serializable]
    public class RangoDia
    {
        public int minPacientes = 2;
        public int maxPacientes = 6;
        public int anomaliasGarantizadas = 0;
        [Range(0f, 1f)] public float probabilidadAnomalia = 0.2f;
    }

    [Header("Anomalías")]
    public AnomaliaData[] anomaliasPosibles;

    private int totalDelDia;
    private RangoDia rangoActual;
    private List<int> indicesAnomalos = new List<int>();

    [Header("Dificultad por día")]
    public RangoDia[] rangosPorDia;     // posición 0 = día 1, posición 1 = día 2...
    public int diaActual = 1;

    [Header("Referencias de escena que se le pasan al paciente")]
    public Transform counter;
    public Transform puertaSalida;
    public Transform cuartoFacil1;
    public TMP_Text textoNombreDNI;
    public TMP_Text fechaVencimientoDNI;
    public RawImage dniFotoRawImage;

    private int pacientesRestantes;

    private Pacientes currentVisitor;

    [Header("Variables")]
    public TMP_Text dialogueText;
    public TMP_Text nombrePaciente;
    public TMP_Text noMasPacientes;
    public GameObject panelDialogue;
    public GameObject panelDni;
    public GameObject panelNoMasPacientes;
    public GameObject inventario;
    public GameObject tarjetaDNI;
    public Animator anim;

    [Header("Tiempos")]
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
        anim = GetComponent<Animator>();
        panelDialogue.SetActive(false);
        panelNoMasPacientes.SetActive(false);
        IniciarDia();
        tarjetaDNI.SetActive(false);
    }

    public void IniciarDia()
    {
        int i = Mathf.Clamp(diaActual - 1, 0, rangosPorDia.Length - 1);
        rangoActual = rangosPorDia[i];

        totalDelDia = Random.Range(rangoActual.minPacientes, rangoActual.maxPacientes + 1);
        pacientesRestantes = totalDelDia;

        indicesAnomalos.Clear();
        int cantidad = Mathf.Min(rangoActual.anomaliasGarantizadas, totalDelDia);
        while (indicesAnomalos.Count < cantidad)
        {
            int n = Random.Range(0, totalDelDia);
            if (!indicesAnomalos.Contains(n))
                indicesAnomalos.Add(n);
        }

        esElPrimero = true;
        ProximoPaciente();
    }

    public void SiguienteDia()
    {
        diaActual++;
        IniciarDia();
    }

    public void DenegarAcceso()
    {
        if (currentVisitor == null || !currentVisitor.llegoAlCounter) return;

        tarjetaDNI.SetActive(false);
        currentVisitor.Rechazar();
        ProximoPaciente();

        if (DniDisplay.Instancia != null)
            DniDisplay.Instancia.Ocultar();
    }

    public void AceptarAcceso()
    {
        if (currentVisitor == null || !currentVisitor.llegoAlCounter) return;

        tarjetaDNI.SetActive(false);
        currentVisitor.Aceptar();
        ProximoPaciente();

        if (DniDisplay.Instancia != null)
            DniDisplay.Instancia.Ocultar();

    }

    AnomaliaData ElegirAnomalia()
    {
        var validas = new List<AnomaliaData>();
        foreach (var a in anomaliasPosibles)
            if (a != null && a.diaMinimo <= diaActual) validas.Add(a);

        if (validas.Count == 0) return null;
        return validas[Random.Range(0, validas.Count)];
    }

    public void ProximoPaciente()
    {
        StopAllCoroutines();
        pacienteHablando = false;

        if (pacientesRestantes <= 0)
        {
            currentVisitor = null;
            panelDialogue.SetActive(false);
            panelNoMasPacientes.SetActive(true);
            inventario.SetActive(false);
            noMasPacientes.text = "No hay mas pacientes por hoy. Termina de curar a los pacientes para comenzar el proximo dia.";

            StartCoroutine(OcultarNoMasPacientes(tiempoMensajeVisible));    
        }
        else
        {
            StartCoroutine(SiguientePaciente());
        }
    }

    IEnumerator OcultarNoMasPacientes(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        panelNoMasPacientes.SetActive(false);
        inventario.SetActive(true);
    }

    IEnumerator SiguientePaciente()
    {
        currentVisitor = null;
        inventario.SetActive(false);

        if (!esElPrimero)
            yield return new WaitForSeconds(tiempoEntrePacientes);

        esElPrimero = false;
        dniMostrado = false;

        int numeroPaciente = totalDelDia - pacientesRestantes;
        pacientesRestantes--;

        // 1. Decidir si es anómalo y cuál anomalía
        bool esAnomalo = indicesAnomalos.Contains(numeroPaciente) || Random.value < rangoActual.probabilidadAnomalia;
        AnomaliaData anomalia = esAnomalo ? ElegirAnomalia() : null;

        // 2. Elegir el prefab: el anómalo si la anomalía es visual, si no uno normal
        Pacientes prefab;
        if (anomalia != null && anomalia.tipo == TipoAnomalia.Visual && anomalia.prefabAnomalo != null)
            prefab = anomalia.prefabAnomalo;
        else
            prefab = prefabsPacientes[Random.Range(0, prefabsPacientes.Length)];

        currentVisitor = Instantiate(prefab, puntoSpawn.position, puntoSpawn.rotation);
        currentVisitor.anomalia = anomalia;

        var agent = currentVisitor.GetComponent<NavMeshAgent>();
        if (agent != null) agent.Warp(puntoSpawn.position);

        currentVisitor.Inicializar(this, counter, puertaSalida, cuartoFacil1, tarjetaDNI, textoNombreDNI, fechaVencimientoDNI, dniFotoRawImage);
        currentVisitor.GenerarDatos();
        currentVisitor.Iniciar();
    }

    public void MostrarDNI(Pacientes quien)
    {
 
        if (currentVisitor == null || quien != currentVisitor) return;
        if (!currentVisitor.PuedeInteractuar) return;
        if (pacienteHablando) return;

        panelDialogue.SetActive(true);
        tarjetaDNI.SetActive(true);
    nombrePaciente.text = currentVisitor.nombreEnDialogo;

        if (!dniMostrado)
        {
            dniMostrado = true;
            inventario.SetActive(false);
            Decir("Hola, mi nombre es " + currentVisitor.nombreEnDialogo);
        }

        else
        {
            inventario.SetActive(false);
            Decir("Ehmm.. ya te dije que mi nombre es " + currentVisitor.nombreEnDialogo);
        }

        if (!currentVisitor.llegoAlCounter)
            dniMostrado = true;

        if (corrutinaOcultarDialogo != null)
            StopCoroutine(corrutinaOcultarDialogo);

        // Oculta diálogo después de 5 segundos
        corrutinaOcultarDialogo = StartCoroutine(OcultarDialogoDespuesDe(5f));

        DniDisplay.Instancia.MostrarActual();
    }

    public void AbrirDNI()
    {
        if (currentVisitor == null || !currentVisitor.llegoAlCounter) return;

        tarjetaDNI.SetActive(true);
        panelDni.SetActive(true);

        if (DniDisplay.Instancia != null)
            DniDisplay.Instancia.MostrarActual();
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
}