using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacientes : MonoBehaviour
{
    public static Pacientes Actual;

    public string nombre;
    public string imagenCamara;
    public Texture dniImagen;

    public Transform puertaSalida;
    public Transform cuartoFacil1;

    private bool procesando = false;
    public float velocidadCaminar = 3.5f;

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
    }

    public void Aceptar()
    {
        if (procesando) return;
        procesando = true;
        StopAllCoroutines();
        StartCoroutine(CaminarHacia(cuartoFacil1, 1.5f, alLlegar: () => gameObject.SetActive(false)));
    }

    IEnumerator CaminarHacia(Transform destino, float delayInicial, System.Action alLlegar)
    {
        yield return new WaitForSeconds(delayInicial);

        while (Vector3.Distance(transform.position, destino.position) > 0.15f)
        {
            transform.position = Vector3.MoveTowards(transform.position, destino.position, velocidadCaminar * Time.deltaTime);
            transform.rotation = Quaternion.LookRotation(destino.position - transform.position);
            yield return null;
        }

        alLlegar?.Invoke();
    }

}
