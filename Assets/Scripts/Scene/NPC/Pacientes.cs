using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.AI;
using UnityEngine.UI;

public class Pacientes : MonoBehaviour, IInteractable
{
    public static Pacientes Actual;

    public string nombre;
    public string nombreEnDialogo;
    public AnomaliaData anomalia;
    public Transform anclaAnomalia;
    public bool EsAnomalia { get { return anomalia != null; } }

    public bool Ataca { get { return anomalia != null; } }

    [Header("Paciencia")]
    public float tiempoPaciencia = 45f;

    [Header("Curación")]
    public float tiempoCuracion = 10f;          
    public float tiempoDesaparecerMuerto = 3f;   

    [Header("Ataque (anomalías)")]
    public float velocidadAtaque = 8f;
    public float distanciaAtaque = 3f;          
    public float alturaAtaque = 1.2f;            
    public float duracionSubida = 0.4f;          
    public string triggerAtaqueAnim = "Ataque";
    public float momentoDano = 0.8f;              // segundos desde que empieza la animación hasta que quita sanidad
    public float duracionAtaque = 2f;             
    public float tiempoMaxCorrer = 15f;

    public float duracionGiroCamara = 0.5f;   

    [Header("Diálogo raro")]
    [Range(0f, 1f)] public float probabilidadCorromper = 0.3f;   
    private string saludoAnomalo = "";
    private string repeticionAnomala = "";

    [Header("Anomalía de cámara")]
    public float giroExtraInfectado = 0f;        
    public float escalaInfectado = 1f;
    private Transform ultimaCamara;

    // Layers que ya existen en el proyecto
    const string layerSoloCamara = "ModeloInfectado";  
    const string layerSoloJugador = "ModeloNormal";    

    private bool esCamaraAnomalia = false;
    private bool modoCamaraActivo = false;
    private GameObject modeloInfectado;
    private Renderer[] renderersNormales;
    private int[] layersOriginales;

    [Header("Ajuste al acostarse (distinto por prefab)")]
    public Vector3 offsetAcostado = Vector3.zero;
    public Vector3 rotacionExtraAcostado = Vector3.zero;

    private Transform puntoAcostado;
    private bool acostado = false;
    private CuartoCamilla cuartoAsignado;
    private bool muerto = false;

    public string imagenCamara;
    public Texture dniImagen;
    public Texture[] dniImagenesAnomalas;
    private RawImage dniFotoRawImage;
    public string dniFechaVencimiento;


    public GameObject tarjetaDNI;
    public TMP_Text textoNombre;
    public TMP_Text fechaVencimiento;

    public string[] textoNombrePosibles = { "QuiQui", "Pumis", "Kirk Charles", "Titi Liss", "Jeffrey", "Alfonso Amat", "Patrick Jane", "Gregory House", "Ted" };
    public string[] fechasVencimientoPosibles = { "03/23/28", "05/19/27", "07/01/30", "11/21/29", "09/25/31" };

    public Transform puertaSalida;
    public Transform cuartoFacil1;   
    public Transform counter;

    private EsperaNPC npcManager;
    private GameObject confetiPrefab;

    public ItemData[] itemData;
    public bool enCuarto = false;
    public int[] medicamentos = new int[6];

    public Vector3 rotacionEnCounter = new Vector3(0f, 90f, 0f);
    public float velocidadCaminar = 3.5f;
    public Animator anim;

    public bool llegoAlCounter = false;
    public bool PuedeInteractuar { get { return llegoAlCounter && !procesando; } }
    private bool procesando = false;
    private NavMeshAgent agent;

    public Enfermedad queEnfermedadtiene;

    public void OnFocus() { }
    public void OnUnfocus() { }

    public bool estaCurado = false;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
        if (anim == null) anim = GetComponentInChildren<Animator>();
        medicamentos = new int[6];
        confetiPrefab = Resources.Load<GameObject>("Confetti");
    }

    public void Iniciar()
    {
        Actual = this;
        procesando = false;
        llegoAlCounter = false;

        agent.speed = velocidadCaminar;
        StartCoroutine(IrHaciaCounter());
    }

    public void Inicializar(EsperaNPC npcManager, Transform counter, Transform puertaSalida, Transform cuartoFacil1, GameObject tarjetaDNI, TMP_Text textoNombre, TMP_Text fechaVencimiento, RawImage dniFotoRawImage)
    {
        this.npcManager = npcManager;
        this.counter = counter;
        this.puertaSalida = puertaSalida;
        this.cuartoFacil1 = cuartoFacil1;
        this.tarjetaDNI = tarjetaDNI;
        this.textoNombre = textoNombre;
        this.fechaVencimiento = fechaVencimiento;
        this.dniFotoRawImage = dniFotoRawImage;
    }

    public void Interact()
    {
        if (enCuarto && !estaCurado && !muerto)
        {
            UsarItemEquipado();
            return;
        }

        npcManager.MostrarDNI(this);
    }

    void UsarItemEquipado()
    {
        if (Inventario.instancia == null) return;

        ItemData item = Inventario.instancia.ItemEquipado;
        if (item == null) return;   

        Inventario.instancia.ConsumirEquipado();

        if (item.queCura == queEnfermedadtiene)
            CurarPacientes();
        else
            Morir();
    }

    public void AplicarAnomalia()
    {
        if (anomalia == null) return;

        switch (anomalia.tipo)
        {
            case TipoAnomalia.NombreIncorrecto:

                do { nombreEnDialogo = textoNombrePosibles[Random.Range(0, textoNombrePosibles.Length)]; }
                while (nombreEnDialogo == nombre);
                break;

            case TipoAnomalia.FotoDiferente:
                if (dniImagenesAnomalas != null && dniImagenesAnomalas.Length > 0)
                    dniImagen = dniImagenesAnomalas[Random.Range(0, dniImagenesAnomalas.Length)];
                break;

            case TipoAnomalia.DialogoRaro:
                // Algunas letras del diálogo cambian por símbolos (a -> @, e -> 3, etc.)
                saludoAnomalo = CorromperAlgunas("Hola, mi nombre es " + nombreEnDialogo, probabilidadCorromper);
                repeticionAnomala = CorromperAlgunas("Ehmm.. ya te dije que mi nombre es " + nombreEnDialogo, probabilidadCorromper);
                break;

            case TipoAnomalia.Visual:
                // El cambio es el modelo infectado: lo elige EsperaNPC al instanciar el prefab
                break;

            case TipoAnomalia.VisualCamara:
                // Se ve normal a simple vista; la cámara de vigilancia ve el modelo infectado
                PrepararModeloInfectado();
                break;
        }
    }

    // ---------- Anomalía de cámara ----------
    void PrepararModeloInfectado()
    {
        Pacientes prefabInf = anomalia.ElegirPrefab();
        if (prefabInf == null) return;

        int layerCam = LayerMask.NameToLayer(layerSoloCamara);
        if (layerCam < 0 || LayerMask.NameToLayer(layerSoloJugador) < 0)
        {
            return;
        }

        renderersNormales = GetComponentsInChildren<Renderer>(true);
        layersOriginales = new int[renderersNormales.Length];
        for (int i = 0; i < renderersNormales.Length; i++)
            layersOriginales[i] = renderersNormales[i].gameObject.layer;

        GameObject go = Instantiate(prefabInf.gameObject, transform);
        go.transform.localPosition = Vector3.zero;
        go.transform.localRotation = Quaternion.identity;

        Vector3 escalaPadre = transform.lossyScale;
        Vector3 escalaPrefab = prefabInf.transform.localScale * escalaInfectado;
        go.transform.localScale = new Vector3(
            escalaPadre.x != 0f ? escalaPrefab.x / escalaPadre.x : 1f,
            escalaPadre.y != 0f ? escalaPrefab.y / escalaPadre.y : 1f,
            escalaPadre.z != 0f ? escalaPrefab.z / escalaPadre.z : 1f);

        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true)) Destroy(mb);
        foreach (var ag in go.GetComponentsInChildren<NavMeshAgent>(true)) Destroy(ag);
        foreach (var col in go.GetComponentsInChildren<Collider>(true)) Destroy(col);
        foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true)) Destroy(rb);

        foreach (var r in go.GetComponentsInChildren<Renderer>(true))
            r.gameObject.layer = layerCam;

        go.SetActive(false);
        modeloInfectado = go;
        esCamaraAnomalia = true;
    }

    void CambiarModoCamara(bool activar)
    {
        modoCamaraActivo = activar;
        int layerJugador = LayerMask.NameToLayer(layerSoloJugador);

        // Modelo normal: con el infectado activo lo ve solo el jugador; si no, vuelve a su layer original
        for (int i = 0; i < renderersNormales.Length; i++)
            if (renderersNormales[i] != null)
                renderersNormales[i].gameObject.layer = activar ? layerJugador : layersOriginales[i];

        modeloInfectado.SetActive(activar);
    }

    void Update()
    {
        if (!esCamaraAnomalia || modeloInfectado == null) return;

        bool debeVerse = llegoAlCounter && !procesando && !muerto;
        if (debeVerse != modoCamaraActivo) CambiarModoCamara(debeVerse);

        if (modoCamaraActivo && npcManager != null)
        {
            Transform cam = npcManager.CamaraQueMiraElJugador();
            if (cam == null) return;

            if (cam != ultimaCamara)
            {
                ultimaCamara = cam;
            }

            Vector3 dir = cam.position - modeloInfectado.transform.position;
            dir.y = 0f;

            if (dir.sqrMagnitude > 0.001f)
                modeloInfectado.transform.rotation =
                    Quaternion.LookRotation(dir) * Quaternion.Euler(0f, giroExtraInfectado, 0f);
        }
    }

    // EsperaNPC pide estos textos en vez de armarlos él
    public string ObtenerSaludo()
    {
        if (!string.IsNullOrEmpty(saludoAnomalo)) return saludoAnomalo;
        return "Hola, mi nombre es " + nombreEnDialogo;
    }

    public string ObtenerRepeticion()
    {
        if (!string.IsNullOrEmpty(repeticionAnomala)) return repeticionAnomala;
        return "Ehmm.. ya te dije que mi nombre es " + nombreEnDialogo;
    }

    public void GenerarDatos()
    {
        nombre = textoNombrePosibles[Random.Range(0, textoNombrePosibles.Length)];
        nombreEnDialogo = nombre;
        dniFechaVencimiento = fechasVencimientoPosibles[Random.Range(0, fechasVencimientoPosibles.Length)];
        queEnfermedadtiene = (Enfermedad)Random.Range(1, System.Enum.GetValues(typeof(Enfermedad)).Length);

        AplicarAnomalia();

        // Se escriben en la tarjeta ya mismo
        if (textoNombre != null) textoNombre.text = nombre;
        if (fechaVencimiento != null) fechaVencimiento.text = dniFechaVencimiento;

        if (dniFotoRawImage != null && dniImagen != null)
            dniFotoRawImage.texture = dniImagen;
    }

    // solo cambia algunas letras
    string CorromperAlgunas(string texto, float probabilidad)
    {
        var sb = new System.Text.StringBuilder();
        bool cambioAlguna = false;

        for (int i = 0; i < texto.Length; i++)
        {
            string reemplazo = ObtenerReemplazo(texto[i]);
            if (reemplazo != null && Random.value < probabilidad)
            {
                sb.Append(reemplazo);
                cambioAlguna = true;
            }
            else
            {
                sb.Append(texto[i]);
            }
        }

        if (!cambioAlguna) return Corromper(texto);
        return sb.ToString();
    }

    string Corromper(string texto)
    {
        var posiciones = new System.Collections.Generic.List<int>();
        for (int i = 0; i < texto.Length; i++)
        {
            if (ObtenerReemplazo(texto[i]) != null)
                posiciones.Add(i);
        }

        if (posiciones.Count == 0) return texto;

        int elegida = posiciones[Random.Range(0, posiciones.Count)];
        string reemplazo = ObtenerReemplazo(texto[elegida]);

        return texto.Substring(0, elegida) + reemplazo + texto.Substring(elegida + 1);
    }

    string ObtenerReemplazo(char c)
    {
        switch (char.ToLower(c))
        {
            case 'a': return "@";
            case 'e': return "3";
            case 'i': return "!";
            case 'o': return "0";
            case 's': return "5";
            case 't': return "7";
            case 'g': return "6";
            case 'b': return "8";
            case 'l': return "1";
            case 'z': return "2";
            case 'k': return "|<";
            case 'h': return "#";
            default: return null;
        }
    }

    IEnumerator IrHaciaCounter()
    {
        agent.isStopped = false;
        agent.SetDestination(counter.position);

        if (anim != null)
            anim.SetBool("Caminando", true);

        // Esperamos a que Unity calcule el camino
        yield return null;
        while (agent.pathPending)
            yield return null;

        // Esperamos hasta estar a menos de 0.3 metros del counter
        while (agent.remainingDistance > 0.3f)
            yield return null;

        agent.isStopped = true;
        transform.rotation = Quaternion.Euler(0f, -90f, 0f);

        if (anim != null)
            anim.SetBool("Caminando", false);

        llegoAlCounter = true;
        StartCoroutine(PacienciaCounter());
    }

    IEnumerator PacienciaCounter()
    {
        yield return new WaitForSeconds(tiempoPaciencia);

        if (procesando) yield break;
        procesando = true;

        if (npcManager != null) npcManager.PacienteSeFueSinAtender(this);

        if (anim != null)
            anim.SetTrigger("Golpe");

        StartCoroutine(EsperarAnimacionYCaminar(puertaSalida, Salir));
    }

    public void Rechazar()
    {
        if (procesando) return;
        procesando = true;
        StopAllCoroutines();

        if (anim != null)
            anim.SetTrigger("Golpe");

        // Camina a la puerta y desaparece
        StartCoroutine(EsperarAnimacionYCaminar(puertaSalida, Salir));
    }

    public void Aceptar(CuartoCamilla cuarto)
    {
        if (procesando) return;
        procesando = true;
        StopAllCoroutines();

        cuartoAsignado = cuarto;

        if (anim != null)
            anim.SetTrigger("Bailar");

        StartCoroutine(EsperarAnimacionYCaminar(cuarto.puntoAcostado, null));
    }

    IEnumerator EsperarAnimacionYCaminar(Transform destino, System.Action alLlegar)
    {
        yield return null;

        if (anim != null)
        {
            float t = 0f;
            while (t < 1f &&
                   !anim.GetCurrentAnimatorStateInfo(0).IsTag("Reaccion") &&
                   !(anim.IsInTransition(0) && anim.GetNextAnimatorStateInfo(0).IsTag("Reaccion")))
            {
                t += Time.deltaTime;
                yield return null;
            }

            while (anim.GetCurrentAnimatorStateInfo(0).IsTag("Reaccion") || anim.IsInTransition(0))
                yield return null;
        }

        if (anim != null)
            anim.SetBool("Caminando", true);

        if (agent != null && destino != null)
        {
            agent.isStopped = false;
            agent.SetDestination(destino.position);

            yield return null;
            while (agent.pathPending)
                yield return null;

            // El límite de tiempo evita que se quede trabado para siempre si el punto no está bien sobre el NavMesh
            float espera = 0f;
            while (agent.remainingDistance > 0.3f && espera < 30f)
            {
                espera += Time.deltaTime;
                yield return null;
            }

            agent.isStopped = true;
        }

        if (anim != null)
            anim.SetBool("Caminando", false);

        // Llegó a su cuarto: se acuesta y aparece el ícono del medicamento
        if (cuartoAsignado != null && destino == cuartoAsignado.puntoAcostado)
        {
            if (agent != null)
                agent.enabled = false;

            puntoAcostado = destino;
            acostado = true;
            enCuarto = true;

            if (anim != null)
                anim.CrossFade("Acostado", 0.2f);

            if (npcManager != null)
                cuartoAsignado.MostrarBurbuja(npcManager.ItemQueCura(queEnfermedadtiene));
        }

        if (alLlegar != null)
            alLlegar.Invoke();
    }

    // Medicamento correcto
    public void CurarPacientes()
    {
        if (estaCurado || muerto) return;
        estaCurado = true;

        if (cuartoAsignado != null) cuartoAsignado.OcultarBurbuja();
        if (npcManager != null) npcManager.PacienteCurado();

        // Normal curado: +5 de sanidad. Si es anomalía no suma.
        if (!EsAnomalia && SanidadManager.Instance != null)
            SanidadManager.Instance.PacienteCurado();

        StartCoroutine(EsperarYSalir());
    }

    public void Morir()
    {
        if (estaCurado || muerto) return;
        muerto = true;

        if (cuartoAsignado != null) cuartoAsignado.OcultarBurbuja();

        if (EsAnomalia)
        {
            // Matar a una anomalía te da +5 de sanidad y no cuenta como muerte de paciente
            if (SanidadManager.Instance != null)
                SanidadManager.Instance.AnomaliaMuerta();
        }
        else
        {
            if (SanidadManager.Instance != null)
                SanidadManager.Instance.PacienteMuertoPorError();

            if (npcManager != null) npcManager.PacienteMurio();
        }

        Vector3 posConfeti = transform.position + Vector3.up * 1f;

        if (confetiPrefab == null)
            confetiPrefab = Resources.Load<GameObject>("Confetti");

        Salir();   

        if (confetiPrefab != null)
        {
            GameObject c = Instantiate(confetiPrefab, posConfeti, confetiPrefab.transform.rotation);

            ParticleSystem ps = c.GetComponentInChildren<ParticleSystem>();
            if (ps != null) ps.Play();
        }

        if (SanidadManager.Instance != null)
            SanidadManager.Instance.PulsoSusto(0.8f);
    }

    IEnumerator EsperarYSalir()
    {
        yield return new WaitForSeconds(tiempoCuracion);

        Vector3 origen = transform.position;
        if (cuartoAsignado != null && cuartoAsignado.puntoLevantarse != null)
            origen = cuartoAsignado.puntoLevantarse.position;
        else

        acostado = false;
        enCuarto = false;
        LiberarCuarto();

        if (agent != null)
        {
            agent.enabled = true;

            UnityEngine.AI.NavMeshHit hit;
            if (UnityEngine.AI.NavMesh.SamplePosition(origen, out hit, 2f, UnityEngine.AI.NavMesh.AllAreas))
                agent.Warp(hit.position);

            if (!agent.isOnNavMesh)
            {
                yield break;   // no se destruye: así se nota que hay que corregir el punto
            }

            agent.isStopped = false;
        }

        if (anim != null)
        {
            anim.ResetTrigger("Acostado");
            anim.SetBool("Caminando", true);
            anim.CrossFade("Caminata", 0.2f);
        }

        if (Ataca && agent != null && npcManager != null && npcManager.jugador != null)
            yield return StartCoroutine(AtacarJugador());

        if (agent != null && puertaSalida != null)
        {
            agent.SetDestination(puertaSalida.position);

            while (DistanciaPlana(transform.position, puertaSalida.position) > 0.8f)
                yield return null;
        }

        Salir();
    }

    IEnumerator AtacarJugador()
    {
        Transform jugador = npcManager.jugador;
        float alturaOriginal = agent.baseOffset;

        agent.speed = velocidadAtaque;
        agent.isStopped = false;

        float tiempo = 0f;
        while (DistanciaPlana(transform.position, jugador.position) > distanciaAtaque && tiempo < tiempoMaxCorrer)
        {
            agent.SetDestination(jugador.position);
            tiempo += Time.deltaTime;
            yield return null;
        }

        if (DistanciaPlana(transform.position, jugador.position) > distanciaAtaque)
        {
            Salir();
            yield break;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (anim != null)
            anim.SetBool("Caminando", false);

        if (npcManager.controladorJugador != null)
            npcManager.controladorJugador.enabled = false;

        // Se da vuelta hacia el jugador
        Vector3 dir = jugador.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        Coroutine giro = StartCoroutine(GirarJugadorHaciaMi(duracionGiroCamara));
        yield return StartCoroutine(CambiarAltura(alturaOriginal + alturaAtaque, duracionSubida));
        yield return giro;  
        
        if (anim != null)
            anim.SetTrigger(triggerAtaqueAnim);

        yield return new WaitForSeconds(momentoDano);
        if (SanidadManager.Instance != null)
            SanidadManager.Instance.AtaqueAnomalia();

        float resto = Mathf.Max(0f, duracionAtaque - momentoDano);
        if (EfectoAtaque.Instance != null)
            EfectoAtaque.Instance.Reproducir(resto);

        yield return new WaitForSeconds(resto);

        if (npcManager.controladorJugador != null)
            npcManager.controladorJugador.enabled = true;

        if (npcManager.controladorJugador != null)
            npcManager.controladorJugador.SendMessage("ResetearMirada", SendMessageOptions.DontRequireReceiver);

        StartCoroutine(CambiarAltura(alturaOriginal, duracionSubida));

        agent.speed = velocidadCaminar;
        agent.isStopped = false;

        if (anim != null)
        {
            anim.SetBool("Caminando", true);
            anim.CrossFade("Caminata", 0.2f);
        }
    }

    IEnumerator GirarJugadorHaciaMi(float duracion)
    {
        Transform jugador = npcManager.jugador;
        Transform cam = Camera.main != null ? Camera.main.transform : null;

        Vector3 dir = transform.position - jugador.position;
        dir.y = 0f;
        if (dir.sqrMagnitude < 0.001f) yield break;

        Quaternion rotInicial = jugador.rotation;
        Quaternion rotFinal = Quaternion.LookRotation(dir);

        Quaternion camInicial = cam != null ? cam.localRotation : Quaternion.identity;
        Quaternion camFinal = Quaternion.Euler(-10f, 0f, 0f);

        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float k = Mathf.SmoothStep(0f, 1f, t / duracion);
            jugador.rotation = Quaternion.Slerp(rotInicial, rotFinal, k);
            if (cam != null) cam.localRotation = Quaternion.Slerp(camInicial, camFinal, k);
            yield return null;
        }

        jugador.rotation = rotFinal;
        if (cam != null) cam.localRotation = camFinal;
    }

    IEnumerator CambiarAltura(float destino, float duracion)
    {
        float inicio = agent.baseOffset;
        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            agent.baseOffset = Mathf.Lerp(inicio, destino, Mathf.SmoothStep(0f, 1f, t / duracion));
            yield return null;
        }
        agent.baseOffset = destino;
    }

    float DistanciaPlana(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }

    void LiberarCuarto()
    {
        if (cuartoAsignado != null)
        {
            cuartoAsignado.Liberar();
            cuartoAsignado = null;
        }
    }

    private bool yaSalio = false;

    // Todas las salidas pasan por acá: libera el cuarto, avisa al manager y se destruye
    void Salir()
    {
        if (yaSalio) return;
        yaSalio = true;

        LiberarCuarto();
        if (npcManager != null) npcManager.PacienteSalio();
        Destroy(gameObject);
    }

    void LateUpdate()
    {
        if (!acostado || puntoAcostado == null) return;

        transform.position = puntoAcostado.position + puntoAcostado.TransformDirection(offsetAcostado);
        transform.rotation = puntoAcostado.rotation * Quaternion.Euler(rotacionExtraAcostado);
    }
}