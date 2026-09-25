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

    private bool procesando = false;
    public float velocidadCaminar = 3.5f;
    private bool dialogoEntrada = false;
    private NavMeshAgent agent;


    public Animator anim; 

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (agent != null && counter != null);
            agent.SetDestination(counter.position);
    }

    private void Update()
    {
        if(!dialogoEntrada && counter  != null)
        {
            float distancia = Vector3.Distance(transform.position, counter.position);
            if (distancia <= 0.3f)
            {
                dialogoEntrada = true;

                
            }
        }
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
        // Esperamos un frame para que el Animator procese la transición del trigger
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

        while (Vector3.Distance(transform.position, destino.position) > 0.15f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino.position, velocidadCaminar * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(destino.position - transform.position);
            yield return null;
        }

        if (anim != null)
            anim.SetBool("Caminando", false);

        alLlegar?.Invoke();
    }
}