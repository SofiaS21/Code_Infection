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
    private List<Pacientes> bolsaPrefabs = new List<Pacientes>();
    private Pacientes ultimoPrefab;

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

    [Header("Cuartos (uno por camilla)")]
    public CuartoCamilla[] cuartos;

    [Header("Medicamentos (los 6 ItemData: Bisturin, Corazon, Jeringa, Pastilla, Tijeras, Vendaje)")]
    public ItemData[] todosLosItems;

    [Header("Game Over por rechazos")]
    public int maxRechazosNormales = 3;
    private int rechazosNormales = 0;

    [Header("Fin del dia")]
    public GameObject panelReloj;
    public GameObject panelFinalDia;
    public TMP_Text textoApuestas;   
    public TMP_Text textoReloj;
    public TMP_Text textoFinalDia;
    public TMP_Text textoIngresos, textoCompras, textoMuertes, textoTotal;
    public float tiempoRelojVisible = 3f;
    public float tiempoReloj9 = 1.5f; 
    public float retrasoPanelFinDia = 1.5f;

    private int pacientesEnEscena = 0;
    private bool diaTerminado = false;
    private int curadosHoy = 0;
    private int muertosHoy = 0;


    [Header("Transiciones")]
    public float duracionFade = 0.8f;
    public float tiempoBloqueoClic = 3f;

    [Header("Referencias de escena que se le pasan al paciente")]
    public Transform counter;
    public Transform puertaSalida;
    public Transform cuartoFacil1;
    public TMP_Text textoNombreDNI;
    public TMP_Text fechaVencimientoDNI;
    public RawImage dniFotoRawImage;

    [Header("Jugador (para el ataque de anomalías)")]
    public Transform jugador;
    public MonoBehaviour controladorJugador;

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
    public GameObject sanidadPanel;
    public GameObject tarjetaDNI;
    public Animator anim;

    public TiendaUI tienda;
    public MonoBehaviour[] scriptsADesactivar;
    bool popupAbierto;

    public Transform camaraVigilancia;

    [Header("Cámaras de vigilancia")]
    public Camera[] camarasVigilancia; 

    public Transform CamaraQueMiraElJugador()
    {
        if (camarasVigilancia != null)
            foreach (var c in camarasVigilancia)
                if (c != null && c.enabled && c.gameObject.activeInHierarchy)
                    return c.transform;

        return camaraVigilancia;
    }

    [Header("Aviso de cuarto")]
    public AvisoCuarto avisoCuarto;

    [Header("Tiempos")]
    public float tiempoMensajeVisible = 5f;
    public float tiempoEntrePacientes = 10f;
    public float tiempoAntesDelPanel = 5f;
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

        panelReloj.SetActive(false);
        panelFinalDia.SetActive(false);
    }

    public void IniciarDia()
    {
        bolsaPrefabs.Clear();
        ultimoPrefab = null;
        diaTerminado = false;
        pacientesEnEscena = 0;
        curadosHoy = 0;
        muertosHoy = 0;
        panelReloj.SetActive(false);
        panelFinalDia.SetActive(false);
        if (sanidadPanel != null) sanidadPanel.SetActive(true);

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
        DineroManager.instancia.NuevoDia();
        IniciarDia();
    }

    public void DenegarAcceso()
    {
        if (currentVisitor == null || !currentVisitor.llegoAlCounter) return;

        bool eraNormal = !currentVisitor.EsAnomalia;   // antes de rechazar

        tarjetaDNI.SetActive(false);
        currentVisitor.Rechazar();

        if (eraNormal)
        {
            rechazosNormales++;
            if (rechazosNormales >= maxRechazosNormales)
            {
                GameOverPorRechazos();
                return;   // no seguir con el próximo paciente
            }
        }

        ProximoPaciente();

        if (DniDisplay.Instancia != null)
            DniDisplay.Instancia.Ocultar();
    }

    void GameOverPorRechazos()
    {
        diaTerminado = true;      
        StopAllCoroutines();
        currentVisitor = null;

        panelDialogue.SetActive(false);
        tarjetaDNI.SetActive(false);
        inventario.SetActive(false);
        if (DniDisplay.Instancia != null) DniDisplay.Instancia.Ocultar();

        SanidadManager.Instance.PerderPorRechazos();
    }

    public void AceptarAcceso()
    {
        if (currentVisitor == null || !currentVisitor.llegoAlCounter) return;

        // Busca una camilla libre; si están todas ocupadas no lo deja pasar todavía
        CuartoCamilla cuarto = ObtenerCuartoLibre();
        if (cuarto == null)
        {
            return;
        }

        cuarto.Ocupar();

        int numeroCuarto = System.Array.IndexOf(cuartos, cuarto) + 1;

        if (avisoCuarto != null) avisoCuarto.Mostrar(numeroCuarto);   

        // Aceptar a cualquiera (anomalía o no): +5 de sanidad
        if (SanidadManager.Instance != null)
            SanidadManager.Instance.AceptarPaciente();

        tarjetaDNI.SetActive(false);
        currentVisitor.Aceptar(cuarto);
        ProximoPaciente();

        if (DniDisplay.Instancia != null)
            DniDisplay.Instancia.Ocultar();

    }

    CuartoCamilla ObtenerCuartoLibre()
    {
        var libres = new List<CuartoCamilla>();
        foreach (var c in cuartos)
            if (c != null && !c.Ocupado) libres.Add(c);

        if (libres.Count == 0) return null;
        return libres[Random.Range(0, libres.Count)];
    }

    // Devuelve el ItemData que cura esa enfermedad (para mostrar su ícono en la camilla)
    public ItemData ItemQueCura(Enfermedad enfermedad)
    {
        foreach (var it in todosLosItems)
            if (it != null && it.queCura == enfermedad) return it;

        return null;
    }

    AnomaliaData ElegirAnomalia()
    {
        if (anomaliasPosibles == null || anomaliasPosibles.Length == 0)
        {
            return null;
        }

        var validas = new List<AnomaliaData>();
        foreach (var a in anomaliasPosibles)
        {
            if (a == null)
            {
                continue;
            }

            if (a.diaMinimo <= diaActual) validas.Add(a);
        }

        if (validas.Count == 0) return null;
        return validas[Random.Range(0, validas.Count)];
    }

    public void ProximoPaciente()
    {
        if (diaTerminado) return;
        StopAllCoroutines();
        pacienteHablando = false;

        if (pacientesRestantes <= 0)
        {

            currentVisitor = null;
            panelDialogue.SetActive(false);

            ComprobarFinDelDia();
            if (!diaTerminado)
                StartCoroutine(MostrarNoMasPacientes());
        }
        else
        {
            StartCoroutine(SiguientePaciente());
        }
    }

    void ComprobarFinDelDia()
    {

        if (diaTerminado) return;
        if (pacientesRestantes > 0 || pacientesEnEscena > 0) return;

        diaTerminado = true;

        if (SanidadManager.Instance != null)
            SanidadManager.Instance.BonusFinDeDia();

        StopAllCoroutines();
        StartCoroutine(PanelesFinDelDia());
    }

    void RellenarPanelFinal()
    {
        var d = DineroManager.instancia;

        if (textoFinalDia != null) textoFinalDia.text = "DIA " + diaActual;
        if (textoIngresos != null) textoIngresos.text = FormatoDinero(d.GananciasDia);
        if (textoCompras != null) textoCompras.text = FormatoDinero(-d.ComprasDia);
        if (textoMuertes != null) textoMuertes.text = muertosHoy.ToString();
        if (textoTotal != null) textoTotal.text = FormatoDinero(d.TotalDia);
        if (textoApuestas != null)
        {
            int a = d.ApuestasDia;
            textoApuestas.gameObject.SetActive(a != 0);   // si no jugaste, no aparece
            textoApuestas.text = a > 0 ? "Casino: ganaste $" + a : "Casino: perdiste $" + (-a);
            textoApuestas.color = a > 0 ? new Color(0.4f, 0.9f, 0.4f) : new Color(1f, 0.45f, 0.45f);
        }
    }

    string FormatoDinero(int v)
    {
        string signo = v > 0 ? "+" : (v < 0 ? "-" : "");
        return signo + "$" + Mathf.Abs(v);
    }

    IEnumerator FadeTexto(TMP_Text t, float desde, float hasta, float duracion)
    {
        float x = 0f;
        while (x < duracion)
        {
            x += Time.unscaledDeltaTime;
            t.alpha = Mathf.Lerp(desde, hasta, x / duracion);
            yield return null;
        }
        t.alpha = hasta;
    }

    Pacientes SiguientePrefabNormal()
    {
        if (bolsaPrefabs.Count == 0)
            bolsaPrefabs.AddRange(prefabsPacientes);

        int i = Random.Range(0, bolsaPrefabs.Count);

        if (bolsaPrefabs.Count > 1 && bolsaPrefabs[i] == ultimoPrefab)
            i = (i + 1) % bolsaPrefabs.Count;

        Pacientes elegido = bolsaPrefabs[i];
        bolsaPrefabs.RemoveAt(i);
        ultimoPrefab = elegido;
        return elegido;
    }

    IEnumerator PanelesFinDelDia()
    {

        yield return new WaitForSeconds(retrasoPanelFinDia);

        currentVisitor = null;
        panelDialogue.SetActive(false);
        if (popupAbierto) CerrarNoMasPacientes();
        else panelNoMasPacientes.SetActive(false);
        tarjetaDNI.SetActive(false);
        sanidadPanel.SetActive(false);
        inventario.SetActive(false);

        RellenarPanelFinal();

        // Reloj: 8:59 -> 9:00
        if (textoReloj != null)
        {
            textoReloj.text = "8:59";
            textoReloj.alpha = 1f;
        }

        yield return UITransiciones.FadeIn(panelReloj, duracionFade);
        yield return UITransiciones.EsperarSegundosOClic(tiempoRelojVisible);   // se ve el 8:59

        if (textoReloj != null)
        {
            yield return FadeTexto(textoReloj, 1f, 0f, 0.5f);   // el 8:59 se desvanece
            textoReloj.text = "9:00";
            yield return FadeTexto(textoReloj, 0f, 1f, 0.5f);   // aparece el 9:00
        }

        yield return UITransiciones.EsperarSegundosOClic(tiempoReloj9);         // se ve el 9:00
        yield return UITransiciones.FadeOut(panelReloj, duracionFade);

        yield return UITransiciones.FadeIn(panelFinalDia, duracionFade);
        yield return UITransiciones.EsperarClic(tiempoBloqueoClic);
        yield return UITransiciones.FadeOut(panelFinalDia, duracionFade);

        AplicarEfectosCambioDeDia();
        SiguienteDia();
    }

    void AplicarEfectosCambioDeDia()
    {

    }

    public void BotonSiguienteDia()
    {
        panelFinalDia.SetActive(false);
        SiguienteDia();
    }

    public void PacienteSalio()
    {
        pacientesEnEscena = Mathf.Max(0, pacientesEnEscena - 1);
        ComprobarFinDelDia();
    }

    public void PacienteCurado()
    {
        curadosHoy++;
        DineroManager.instancia.PacienteCurado();
    }

    public void PacienteMurio()
    {
        muertosHoy++;
    }

    public void PacienteSeFueSinAtender(Pacientes p)
    {
        if (p != currentVisitor) return;

        // Solo los normales castigan; si se va una anomalía no pasa nada
        if (!p.EsAnomalia)
            DineroManager.instancia.ErrorPacienteNormal();

        tarjetaDNI.SetActive(false);
        if (DniDisplay.Instancia != null)
            DniDisplay.Instancia.Ocultar();

        ProximoPaciente();
    }

    IEnumerator MostrarNoMasPacientes()
    {
        yield return new WaitForSeconds(tiempoAntesDelPanel);

        // Si la tienda está abierta, se cierra sola (el carrito se conserva)
        if (tienda != null)
        {
            tienda.ForzarCierre();
            tienda.bloqueada = true;
        }

        popupAbierto = true;
        foreach (var s in scriptsADesactivar)
            if (s != null) s.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        panelNoMasPacientes.SetActive(true);
        inventario.SetActive(false);
        noMasPacientes.text = "No hay mas pacientes por hoy. Termina de curar a los pacientes para comenzar el proximo dia.";
    }

    public void CerrarNoMasPacientes()
    {
        popupAbierto = false;
        if (tienda != null) tienda.bloqueada = false;

        foreach (var s in scriptsADesactivar)
            if (s != null) s.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        panelNoMasPacientes.SetActive(false);
        inventario.SetActive(true);
    }
    public
    IEnumerator SiguientePaciente()
    {
        currentVisitor = null;
        inventario.SetActive(true);
        panelDialogue.SetActive(false);


        if (!esElPrimero)
            yield return new WaitForSeconds(tiempoEntrePacientes);

        esElPrimero = false;
        dniMostrado = false;

        int numeroPaciente = totalDelDia - pacientesRestantes;
        pacientesRestantes--;

        bool garantizado = indicesAnomalos.Contains(numeroPaciente);
        float dado = Random.value;
        bool esAnomalo = garantizado || dado < rangoActual.probabilidadAnomalia;
        AnomaliaData anomalia = esAnomalo ? ElegirAnomalia() : null;

        // Anomalía visual: elige al azar uno de los modelos infectados; si no hay, usa uno normal
        Pacientes prefab = null;
        if (anomalia != null && anomalia.tipo == TipoAnomalia.Visual)
            prefab = anomalia.ElegirPrefab();

        if (prefab == null)
            prefab = SiguientePrefabNormal();

        currentVisitor = Instantiate(prefab, puntoSpawn.position, puntoSpawn.rotation);
        currentVisitor.anomalia = anomalia;
        pacientesEnEscena++;

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
            Decir(currentVisitor.ObtenerSaludo());
        }

        else
        {
            inventario.SetActive(false);
            Decir(currentVisitor.ObtenerRepeticion());
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
        RellenarPanelFinal();
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

    void LateUpdate()
    {
        if (!popupAbierto) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }
}