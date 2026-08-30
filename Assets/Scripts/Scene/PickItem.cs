using System.Collections;
using UnityEngine;
using TMPro;

public class PickItem : MonoBehaviour
{
    public GameObject canvasE;
    public ItemData item;
    public TMP_Text dialogueText;
    private CanvasGroup canvasGroup;
    private bool jugadorCerca;

    private Coroutine animacionActual;
    private Vector3 escalaOriginal; // Guardamos la escala real del canvas

    void Start()
    {
        canvasGroup = canvasE.GetComponent<CanvasGroup>();
        escalaOriginal = canvasE.transform.localScale; // Se guarda una sola vez al inicio

        // Empieza oculto
        canvasE.SetActive(false);
        canvasGroup.alpha = 0;
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            bool agregado = Inventario.instancia.AgregarItem(item);

            if (agregado)
            {
                Debug.Log("Recogido " + gameObject.name);
                // Si querés que el cartel desaparezca inmediatamente al agarrar el ítem:
                jugadorCerca = false;
                if (animacionActual != null) StopCoroutine(animacionActual);
                animacionActual = StartCoroutine(Ocultar());

                // Aquí probablemente quieras destruir el objeto o desactivarlo
                // Destroy(gameObject, 0.5f); 
            }
            else
            {
                Debug.Log("Inventario lleno");
            }
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

            animacionActual = StartCoroutine(Ocultar()); // <- Ahora esto sí va a funcionar
        }
    }

    IEnumerator Mostrar()
    {
        canvasE.SetActive(true);
        canvasGroup.alpha = 0;

        Vector3 escalaInicial = escalaOriginal * 0.7f;
        canvasE.transform.localScale = escalaInicial;

        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            canvasE.transform.localScale = Vector3.Lerp(escalaInicial, escalaOriginal, t);
            yield return null;
        }

        canvasGroup.alpha = 1;
        canvasE.transform.localScale = escalaOriginal;
    }

    IEnumerator Ocultar()
    {
        float t = 0;
        float alphaInicial = canvasGroup.alpha;
        Vector3 escalaInicial = canvasE.transform.localScale;
        Vector3 escalaFinal = escalaOriginal * 0.7f;

        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            canvasGroup.alpha = Mathf.Lerp(alphaInicial, 0, t);
            canvasE.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);
            yield return null;
        }

        canvasGroup.alpha = 0;
        canvasE.transform.localScale = escalaFinal;

        canvasE.SetActive(false); // Apaga el objeto completo al terminar la animación
    }
}