using System.Collections;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class Pacientes : MonoBehaviour, IInteractable
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

    public Vector3 rotacionEnCounter = new Vector3(0f, 90f, 0f);
    public float velocidadCaminar = 3.5f;
    public Animator anim;

    // EsperaNPC lee esto para saber si ya se pueden usar los botones
    public bool llegoAlCounter = false;
    public bool PuedeInteractuar { get { return llegoAlCounter && !procesando; } }
    private bool procesando = false;
    private NavMeshAgent agent;

    void Awake()
    {
        agent = GetComponent<NavMeshAgent>();
    }

    // Se ejecuta en el momento exacto en que se activa el paciente
    void OnEnable()
    {
        Actual = this;
        procesando = false;
        llegoAlCounter = false;

        tarjetaDNI.SetActive(false);

        if (agent != null && counter != null)
        {
            agent.speed = velocidadCaminar;
            StartCoroutine(IrHaciaCounter());
        }
    }

    void OnDisable()
    {
        if (Actual == this)
            Actual = null;
    }


    public void Interact()
    {
        FindObjectOfType<EsperaNPC>().MostrarDNI(this);
    }

    public void OnFocus() { }
    public void OnUnfocus() { }
        
    // EsperaNPC la llama ANTES de activar al paciente
    public void GenerarDatos()
    {
        int posNombre = Random.Range(0, textoNombrePosibles.Length);
        nombre = textoNombrePosibles[posNombre];

        int posFecha = Random.Range(0, fechasVencimientoPosibles.Length);
        dniFechaVencimiento = fechasVencimientoPosibles[posFecha];

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
        StartCoroutine(EsperarAnimacionYCaminar(puertaSalida, () => gameObject.SetActive(false)));
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