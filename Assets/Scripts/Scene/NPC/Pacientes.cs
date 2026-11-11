using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.AI;
using UnityEngine.UI;

public class Pacientes : MonoBehaviour, IInteractable
{
    public static Pacientes Actual;

    // Datos del paciente
    public string nombre;
    public string nombreEnDialogo;
    public AnomaliaData anomalia;
    public Transform anclaAnomalia;
    public bool EsAnomalia { get { return anomalia != null; } }

    // Todas las anomalías atacan (visual, diálogo, nombre, foto)
    public bool Ataca { get { return anomalia != null; } }

    [Header("Paciencia")]
    public float tiempoPaciencia = 45f;

    [Header("Curación")]
    public float tiempoCuracion = 10f;            // segundos que espera después de curarse antes de irse (o de atacar, si es anomalía)
    public float tiempoDesaparecerMuerto = 3f;    // segundos que queda en la camilla si muere

    [Header("Ataque (anomalías)")]
    public float velocidadAtaque = 8f;
    public float distanciaAtaque = 3f;            // a qué distancia del jugador frena (más alto = más lejos)
    public float alturaAtaque = 1.2f;             // cuánto sube antes de atacar
    public float duracionSubida = 0.4f;           // cuánto tarda en subir y en bajar
    public string triggerAtaqueAnim = "Ataque";
    public float momentoDano = 0.8f;              // segundos desde que empieza la animación hasta que quita sanidad
    public float duracionAtaque = 2f;             // cuánto tiempo dura la animación de ataque (el jugador queda quieto)
    public float tiempoMaxCorrer = 15f;           // por seguridad: si no llega al jugador en este tiempo, desaparece

    [Header("Diálogo raro")]
    [Range(0f, 1f)] public float probabilidadCorromper = 0.3f;   // chance de que cada letra reemplazable cambie
    private string saludoAnomalo = "";
    private string repeticionAnomala = "";

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


    // Tarjeta DNI (objetos de la escena)
    public GameObject tarjetaDNI;
    public TMP_Text textoNombre;
    public TMP_Text fechaVencimiento;

    // Listas para elegir al azar
    public string[] textoNombrePosibles = { "QuiQui", "Pumis", "Kirk Charles", "Titi Liss", "Jeffrey", "Alfonso Amat", "Patrick Jane", "Gregory House", "Ted" };
    public string[] fechasVencimientoPosibles = { "03/23/28", "05/19/27", "07/01/30", "11/21/29", "09/25/31" };

    // Lugares a los que camina
    public Transform puertaSalida;
    public Transform cuartoFacil1;   // ya no se usa para acostarse (ahora va al cuarto asignado), se deja para no romper referencias
    public Transform counter;

    private EsperaNPC npcManager;

    public ItemData[] itemData;
    public bool enCuarto = false;
    public int[] medicamentos = new int[6];

    public Vector3 rotacionEnCounter = new Vector3(0f, 90f, 0f);
    public float velocidadCaminar = 3.5f;
    public Animator anim;

    // EsperaNPC lee esto para saber si ya se pueden usar los botones
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
        // Si está acostado esperando su medicina, se intenta curar con el ítem en mano
        if (enCuarto && !estaCurado && !muerto)
        {
            UsarItemEquipado();
            return;
        }

        Debug.Log("Interact llamado en " + name);
        npcManager.MostrarDNI(this);
    }

    void UsarItemEquipado()
    {
        if (Inventario.instancia == null) return;

        ItemData item = Inventario.instancia.ItemEquipado;
        if (item == null) return;   // no tiene nada en la mano

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

    // Cambia solo ALGUNAS letras del texto (cada letra reemplazable tiene 'probabilidad' de cambiar).
    // Si por azar no cambió ninguna, cambia una para que la anomalía siempre se note.
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

        // Si el nombre no tiene ninguna letra reemplazable, no hace nada
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

    // EsperaNPC le pasa el cuarto que le tocó
    public void Aceptar(CuartoCamilla cuarto)
    {
        if (procesando) return;
        procesando = true;
        StopAllCoroutines();

        cuartoAsignado = cuarto;

        if (anim != null)
            anim.SetTrigger("Bailar");

        // Camina al cuarto y se queda ahí
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

    // Medicamento incorrecto
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
            // Paciente normal muerto por error: -10 de sanidad
            if (SanidadManager.Instance != null)
                SanidadManager.Instance.PacienteMuertoPorError();

            if (npcManager != null) npcManager.PacienteMurio();
        }

        StartCoroutine(DesaparecerTrasMorir());
    }

    IEnumerator DesaparecerTrasMorir()
    {
        yield return new WaitForSeconds(tiempoDesaparecerMuerto);
        Salir();
    }

    IEnumerator EsperarYSalir()
    {
        // Sigue acostado durante tiempoCuracion
        yield return new WaitForSeconds(tiempoCuracion);

        // Punto del piso donde se levanta (hay que leerlo antes de liberar el cuarto)
        Vector3 origen = transform.position;
        if (cuartoAsignado != null && cuartoAsignado.puntoLevantarse != null)
            origen = cuartoAsignado.puntoLevantarse.position;
        else
            Debug.LogWarning("Falta asignar Punto Levantarse en " + (cuartoAsignado != null ? cuartoAsignado.name : "el cuarto"));

        // Se levanta: la camilla queda libre para otro paciente
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
                Debug.LogError("El punto de levantarse no está sobre el NavMesh");
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

        Debug.Log("[" + name + "] terminó de curarse | EsAnomalia: " + EsAnomalia
            + " | tipo: " + (anomalia != null ? anomalia.tipo.ToString() : "-")
            + " | Ataca: " + Ataca
            + " | jugador asignado: " + (npcManager != null && npcManager.jugador != null));

        // Si es una anomalía, antes de irse corre hacia el jugador y lo ataca
        if (Ataca && agent != null && npcManager != null && npcManager.jugador != null)
            yield return StartCoroutine(AtacarJugador());

        if (agent != null && puertaSalida != null)
        {
            agent.SetDestination(puertaSalida.position);

            // Solo se destruye cuando llega de verdad a la puerta
            while (DistanciaPlana(transform.position, puertaSalida.position) > 0.8f)
                yield return null;
        }

        Salir();
    }

    IEnumerator AtacarJugador()
    {
        Transform jugador = npcManager.jugador;
        float alturaOriginal = agent.baseOffset;

        // Corre hacia el jugador
        agent.speed = velocidadAtaque;
        agent.isStopped = false;

        float tiempo = 0f;
        while (DistanciaPlana(transform.position, jugador.position) > distanciaAtaque && tiempo < tiempoMaxCorrer)
        {
            agent.SetDestination(jugador.position);
            tiempo += Time.deltaTime;
            yield return null;
        }

        // Si se acabó el tiempo y todavía no llegó, desaparece sin atacar
        if (DistanciaPlana(transform.position, jugador.position) > distanciaAtaque)
        {
            Debug.Log("[" + name + "] no llegó al jugador en " + tiempoMaxCorrer + "s, desaparece");
            Salir();
            yield break;
        }

        agent.isStopped = true;
        agent.velocity = Vector3.zero;

        if (anim != null)
            anim.SetBool("Caminando", false);

        // Congela al jugador
        if (npcManager.controladorJugador != null)
            npcManager.controladorJugador.enabled = false;

        // Se da vuelta hacia el jugador
        Vector3 dir = jugador.position - transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            transform.rotation = Quaternion.LookRotation(dir);

        // Sube antes de atacar
        yield return StartCoroutine(CambiarAltura(alturaOriginal + alturaAtaque, duracionSubida));

        if (anim != null)
            anim.SetTrigger(triggerAtaqueAnim);

        // El golpe quita sanidad y dispara los efectos en el momento justo de la animación
        yield return new WaitForSeconds(momentoDano);
        if (SanidadManager.Instance != null)
            SanidadManager.Instance.AtaqueAnomalia();

        float resto = Mathf.Max(0f, duracionAtaque - momentoDano);
        if (EfectoAtaque.Instance != null)
            EfectoAtaque.Instance.Reproducir(resto);

        yield return new WaitForSeconds(resto);

        // Libera al jugador
        if (npcManager.controladorJugador != null)
            npcManager.controladorJugador.enabled = true;

        // Baja de nuevo y vuelve a caminar a la puerta
        StartCoroutine(CambiarAltura(alturaOriginal, duracionSubida));

        agent.speed = velocidadCaminar;
        agent.isStopped = false;

        if (anim != null)
        {
            anim.SetBool("Caminando", true);
            anim.CrossFade("Caminata", 0.2f);
        }
    }

    // Sube o baja el modelo suavemente (sin tocar el NavMeshAgent de lugar)
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