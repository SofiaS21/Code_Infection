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

    [Header("Paciencia")]
    public float tiempoPaciencia = 45f;

    [Header("Curación")]
    public float tiempoCuracion = 10f;            // segundos que espera después de curarse antes de irse
    public float tiempoDesaparecerMuerto = 3f;    // segundos que queda en la camilla si muere

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

            case TipoAnomalia.Visual:
                break;
        }
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

    public void CurarPacientes()
    {
        if (estaCurado || muerto) return;
        estaCurado = true;

        if (cuartoAsignado != null) cuartoAsignado.OcultarBurbuja();
        if (npcManager != null) npcManager.PacienteCurado();

        StartCoroutine(EsperarYSalir());
    }

    // Medicamento incorrecto
    public void Morir()
    {
        if (estaCurado || muerto) return;
        muerto = true;

        if (cuartoAsignado != null) cuartoAsignado.OcultarBurbuja();
        if (npcManager != null) npcManager.PacienteMurio();

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

        if (agent != null && puertaSalida != null)
        {
            agent.SetDestination(puertaSalida.position);

            // Solo se destruye cuando llega de verdad a la puerta
            while (DistanciaPlana(transform.position, puertaSalida.position) > 0.8f)
                yield return null;
        }

        Salir();
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

    // Todas las salidas pasan por acá: libera el cuarto, avisa al manager y se destruye
    void Salir()
    {
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