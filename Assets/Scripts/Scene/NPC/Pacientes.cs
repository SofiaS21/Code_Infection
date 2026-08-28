using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Pacientes : MonoBehaviour
{
    public string nombre;
    public string imagenCamara;
    public string dniImagen;
    public string dni;

    public Transform puertaSalida;
    private bool procesando = false;
    public float velocidadCaminar = 3.5f;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
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
        StartCoroutine(CaminarHacia(puertaSalida, 1.5f, alLlegar: () => gameObject.SetActive(false)));
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
