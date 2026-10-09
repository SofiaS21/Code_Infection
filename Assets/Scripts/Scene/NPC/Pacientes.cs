using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.AI;
using UnityEngine.UI;

public class Pacientes : MonoBehaviour , IInteractable
{
    public static Pacientes Actual;

    // Datos del paciente
    public string nombre;
    public string imagenCamara;
    public Texture dniImagen;
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
    public GameObject canvasBurbuja;           
    public Image iconoBurbuja;
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
        tarjetaDNI.SetActive(false);

        agent.speed = velocidadCaminar;
        StartCoroutine(IrHaciaCounter());
    }

    public void Inicializar(EsperaNPC npcManager, Transform counter, Transform puertaSalida, Transform cuartoFacil1, GameObject tarjetaDNI, TMP_Text textoNombre, TMP_Text fechaVencimiento)
    {
        this.npcManager = npcManager;
        this.counter = counter;
        this.puertaSalida = puertaSalida;
        this.cuartoFacil1 = cuartoFacil1;
        this.tarjetaDNI = tarjetaDNI;   
        this.textoNombre = textoNombre;
        this.fechaVencimiento = fechaVencimiento;
    }
    
    public void Interact()
    {
        npcManager.MostrarDNI(this);
    }

    // EsperaNPC la llama ANTES de activar al paciente
    public void GenerarDatos()
    {
        int posNombre = Random.Range(0, textoNombrePosibles.Length);
        nombre = textoNombrePosibles[posNombre];

        int posFecha = Random.Range(0, fechasVencimientoPosibles.Length);
        dniFechaVencimiento = fechasVencimientoPosibles[posFecha];

        queEnfermedadtiene = (Enfermedad)Random.Range(1, System.Enum.GetValues(typeof(Enfermedad)).Length);
        

        // Se escriben en la tarjeta ya mismo
        if (textoNombre != null) textoNombre.text = nombre;
        if (fechaVencimiento != null) fechaVencimiento.text = dniFechaVencimiento;
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

        if (alLlegar != null)
            alLlegar.Invoke();
    }
}