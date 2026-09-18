using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;


public class Pacientes : MonoBehaviour
{
    public static Pacientes Actual;

    public string nombre;
    public string imagenCamara;
    public Texture dniImagen;
    public string dniFechaVencimiento;

    public Transform puertaSalida;
    public Transform cuartoFacil1;

    private bool procesando = false;
    public float velocidadCaminar = 3.5f;

    public Animator anim;
    public Animator animAceptar;
    public Animator animRechazar;

    void OnEnable()
    {
        Actual = this; // este paciente pasa a ser "el actual"
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
        StartCoroutine(CaminarHacia(puertaSalida, 1.5f, alLlegar: () => gameObject.SetActive(false)));
        if (animRechazar != null)
            animRechazar.SetTrigger("Golpe");
    }

    public void Aceptar()
    {
        if (procesando) return;
        procesando = true;
        StopAllCoroutines();
        StartCoroutine(CaminarHacia(cuartoFacil1, 1.5f, alLlegar: () => gameObject.SetActive(false)));
        if (animAceptar != null)
            animAceptar.SetTrigger("Bailar");
    }

    IEnumerator CaminarHacia(Transform destino, float delayInicial, System.Action alLlegar)
    {
        yield return new WaitForSeconds(delayInicial);

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
