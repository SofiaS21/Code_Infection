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
    public Transform cuartoFacil1;
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

    public float tiempoEsperarCuracion = 40f;
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
        Debug.Log("Interact llamado en " + name);
        npcManager.MostrarDNI(this);
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
        StartCoroutine(EsperarAnimacionYCaminar(puertaSalida, () => Destroy(gameObject)));
    }

    public void Rechazar()
    {
        if (procesando) return;
        procesando = true;
        StopAllCoroutines();

        if (anim != null)
            anim.SetTrigger("Golpe");

        // Camina a la puerta y desaparece
        StartCoroutine(EsperarAnimacionYCaminar(puertaSalida, () => Destroy(gameObject)));
    }

    public void Aceptar()
    {
        if (procesando) return;
        procesando = true;
        StopAllCoroutines();

        if (anim != null)
            anim.SetTrigger("Bailar");

        // Camina al cuarto y se queda ahí
        StartCoroutine(EsperarAnimacionYCaminar(cuartoFacil1, null));
    }

    IEnumerator EsperarAnimacionYCaminar(Transform destino, System.Action alLlegar)
    {
        // Un frame para que el Animator procese el trigger
        yield return null;

        if (anim != null)
        {
            while (anim.GetCurrentAnimatorStateInfo(0).IsTag("Reaccion"))
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

            while (agent.remainingDistance > 0.3f)
                yield return null;

            agent.isStopped = true;
        }

        if (anim != null)
            anim.SetBool("Caminando", false);

        if ( destino == cuartoFacil1)
        {
            if (agent != null)
                agent.enabled = false;

            transform.position = destino.position;
            transform.rotation = destino.rotation;

            if (anim != null)
            {
                anim.SetTrigger("Acostado");
            }
        }

        if (alLlegar != null)
            alLlegar.Invoke();
    }

    IEnumerator EsperarYSalir()
    {
        yield return new WaitForSeconds(tiempoEsperarCuracion);

        if (agent != null)
        {
            agent.enabled = true;
            agent.isStopped = false;
        }

        if (anim != null)
        {
            anim.ResetTrigger("Acostado");
            anim.SetBool("Caminando", true);
        }

        if (agent != null && puertaSalida != null)
        {
            agent.SetDestination(puertaSalida.position);

            yield return null;
            while (agent.pathPending)
                yield return null;

            while (agent.remainingDistance > 0.3f)
                yield return null;
        }

        Destroy(gameObject);
    }

    public void CurarPacientes()
    {
        if (estaCurado) return;
        estaCurado = true;

        if (npcManager != null) npcManager.PacienteCurado();
        StartCoroutine(EsperarYSalir());
    }
}