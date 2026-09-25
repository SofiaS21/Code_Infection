using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.AI;

public class Pacientes : MonoBehaviour
{
    public static Pacientes Actual;

    public string nombre;
    public string imagenCamara;
    public Texture dniImagen;
    public string dniFechaVencimiento;

    public Transform puertaSalida;
    public Transform cuartoFacil1;
    public Transform counter;

    [Tooltip("Rotacion (en grados, Euler) que va a tener el personaje al llegar al counter")]
    public Vector3 rotacionEnCounter = new Vector3(0f, 90f, 0f);

    private bool procesando = false;
    public float velocidadCaminar = 3.5f;
    private bool dialogoEntrada = false;
    private NavMeshAgent agent;

    public Animator anim;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null)
        {
            agent.speed = velocidadCaminar;

            if (counter != null)
            {
                StartCoroutine(IrHaciaCounter());
            }
        }
    }

    IEnumerator IrHaciaCounter()
    {
        agent.isStopped = false;
        agent.SetDestination(counter.position);

        if (anim != null)
            anim.SetBool("Caminando", true);

        // Esperamos a que Unity termine de calcular el path antes de medir distancia
        yield return null;
        while (agent.pathPending)
            yield return null;

        while (agent.remainingDistance > agent.stoppingDistance)
            yield return null;

        agent.isStopped = true;
        transform.rotation = Quaternion.Euler(rotacionEnCounter); // fuerza la rotacion al llegar

        dialogoEntrada = true;

        if (anim != null)
            anim.SetBool("Caminando", false); // vuelve a Idle

        // Aca podes disparar el dialogo de recepcion (ej: abrir UI, llamar a otro metodo, etc)
    }

    void OnEnable()
    {
        Actual = this;
        procesando = false;
    }

    private void OnDisable()
    {
        if (Actual == this)
            Actual = null;
    }

    public void Rechazar()
    {
        if (procesando) return;
        procesando = true;
        StopAllCoroutines();
        if (anim != null)
            anim.SetTrigger("Golpe");
        StartCoroutine(EsperarAnimacionYCaminar(puertaSalida, alLlegar: () => gameObject.SetActive(false)));
    }

    public void Aceptar()
    {
        if (procesando) return;
        procesando = true;
        StopAllCoroutines();
        if (anim != null)
            anim.SetTrigger("Bailar");
        StartCoroutine(EsperarAnimacionYCaminar(cuartoFacil1, alLlegar: () => gameObject.SetActive(false)));
    }

    IEnumerator EsperarAnimacionYCaminar(Transform destino, System.Action alLlegar)
    {
        // Esperamos un frame para que el Animator procese la transicion del trigger
        yield return null;

        if (anim != null)
        {
            while (anim.GetCurrentAnimatorStateInfo(0).IsTag("Reaccion"))
            {
                yield return null;
            }
        }

        if (anim != null)
            anim.SetBool("Caminando", true);

        if (agent != null && destino != null)
        {
            agent.isStopped = false;
            agent.SetDestination(destino.position);

            // Esperamos un frame extra a que Unity termine de calcular el path
            yield return null;
            while (agent.pathPending)
            {
                yield return null;
            }

            // Reci�n ahora remainingDistance refleja el path nuevo, no el anterior
            while (agent.remainingDistance > agent.stoppingDistance)
            {
                yield return null;
            }

            agent.isStopped = true;
        }
        else
        {
            // Fallback por si no hay NavMeshAgent en este objeto
            while (Vector3.Distance(transform.position, destino.position) > 0.15f)
            {
                transform.position = Vector3.MoveTowards(transform.position, destino.position, velocidadCaminar * Time.deltaTime);
                transform.rotation = Quaternion.LookRotation(destino.position - transform.position);
                yield return null;
            }
        }

        if (anim != null)
            anim.SetBool("Caminando", false);

        alLlegar?.Invoke();
    }
}