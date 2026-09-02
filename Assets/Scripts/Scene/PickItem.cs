using System.Collections;
using UnityEngine;
using TMPro;

public class PickItem : MonoBehaviour, IInteractable
{
    public GameObject canvasE;
    public ItemData item;
    public TMP_Text dialogueText;
    private CanvasGroup canvasGroup;
    private Coroutine animacionActual;
    public float tiempoMensajeVisible = 5f;

    void Start()
    {
        canvasGroup = canvasE.GetComponent<CanvasGroup>();
        canvasE.SetActive(false);
        canvasGroup.alpha = 0;
    }

    public void OnFocus()
    {
        if (animacionActual != null)
            StopCoroutine(animacionActual);
        animacionActual = StartCoroutine(MostrarCartelE());
    }

    public void OnUnfocus()
    {
        if (animacionActual != null)
            StopCoroutine(animacionActual);
        animacionActual = StartCoroutine(OcultarCartelE());
    }

    public void Interact()
    {
        bool agregado = Inventario.instancia.AgregarItem(item);
        if (agregado)
            Debug.Log("Recogido " + gameObject.name);
        else
            Debug.Log("Inventario lleno");
    }

    IEnumerator MostrarCartelE()
    {
        canvasE.SetActive(true);
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

    IEnumerator OcultarCartelE()
    {
        while (canvasGroup.alpha > 0)
        {
            canvasGroup.alpha -= Time.deltaTime * 6f;
            yield return null;
        }
        canvasGroup.alpha = 0;
        canvasE.SetActive(false);
    }

    IEnumerator OcultarMensajeDespuesDe(float segundos)
    {
        yield return new WaitForSeconds(segundos);
        dialogueText.gameObject.SetActive(false);
    }
}