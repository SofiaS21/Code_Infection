using System.Collections;
using UnityEngine;

public class AceptarAcceso : MonoBehaviour , IInteractable
{
    public EsperaNPC esperaNPC;
    public GameObject canvasE;
    private CanvasGroup canvasGroup;
    private Coroutine animacionActual;

    private void Start()
    {
        canvasGroup = canvasE.GetComponent<CanvasGroup>();
        canvasE.SetActive(false);
        canvasGroup.alpha = 0;
    }

    public void OnFocus()
    {
        if (animacionActual != null)
            StopCoroutine(animacionActual);
        animacionActual = StartCoroutine(Mostrar());
    }

    public void OnUnfocus()
    {
        if (animacionActual != null)
            StopCoroutine(animacionActual);
        animacionActual = StartCoroutine(Ocultar());
    }

    public void Interact()
    {
        esperaNPC.AceptarAcceso();
        Debug.Log("Paciente Aceptado");
    }

    IEnumerator Mostrar()
    {
        canvasE.SetActive(false);
        canvasGroup.alpha = 0;
        Vector3 escalaFinal = canvasE.transform.localScale;
        Vector3 escalaInicial = escalaFinal * 0.7f;
        canvasE.transform.localScale = escalaInicial;
        float t = 0;
        while (t < 1)
        {
            t += Time.deltaTime * 5f;
            canvasGroup.alpha = Mathf.Lerp(0, 1, t);
            canvasE.transform.localScale = Vector3.Lerp(escalaInicial, escalaFinal, t);
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