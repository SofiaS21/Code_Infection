using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DenegarAcceso : MonoBehaviour
{
    public EsperaNPC esperaNPC;
    public GameObject canvasE;
    private CanvasGroup canvasGroup;
    private Coroutine animacionActual;
    private bool jugadorCerca;

    private void Start()
    {
        canvasGroup = canvasE.GetComponent<CanvasGroup>();

        // Empieza oculto
        canvasE.SetActive(false);
        canvasGroup.alpha = 0;
    }

    void Update()   
    {
        if (canvasE.activeSelf && Input.GetKeyDown(KeyCode.E))
        {
            esperaNPC.DenegarAcceso();
            Debug.Log("Paciente rechazado");
        }
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;

            if (animacionActual != null)
                StopCoroutine(animacionActual);

            animacionActual = StartCoroutine(Mostrar());
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;

            if (animacionActual != null)
                StopCoroutine(animacionActual);

            animacionActual = StartCoroutine(Ocultar());
        }
    }

    IEnumerator Mostrar()
    {
        canvasE.SetActive(true);

        canvasGroup.alpha = 0;

        // Guardamos la escala original
        Vector3 escalaFinal = canvasE.transform.localScale;

        // Empieza un poquito más pequeño
        Vector3 escalaInicial = escalaFinal * 0.7f;

        canvasE.transform.localScale = escalaInicial;

        float t = 0;

        while (t < 1)
        {
            t += Time.deltaTime * 5f;

            // Fade in
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);

            // Mini animación de crecimiento
            canvasE.transform.localScale =
                Vector3.Lerp(escalaInicial, escalaFinal, t);

            yield return null;
        }

        canvasGroup.alpha = 1;
        canvasE.transform.localScale = escalaFinal;
    }

    IEnumerator Ocultar()
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * 6f;

            yield return null;
        }

        canvasGroup.alpha = 0;
        canvasE.SetActive(false);
    }
}
