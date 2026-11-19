using System.Collections;
using TMPro;
using UnityEngine;

// Ponlo en un objeto vacío que SIEMPRE esté activo (por ejemplo hijo del Canvas),
// NO en el panel, porque el panel se apaga.
public class AvisoCuarto : MonoBehaviour
{
    public GameObject panel;        // el panelcito de arriba
    public TMP_Text texto;
    public float tiempoVisible = 2f;
    public float duracionFade = 0.3f;

    private Coroutine actual;

    void Awake()
    {
        panel.SetActive(false);
    }

    public void Mostrar(int numeroCuarto)
    {
        if (actual != null) StopCoroutine(actual);
        actual = StartCoroutine(Rutina(numeroCuarto));
    }

    IEnumerator Rutina(int numeroCuarto)
    {
        texto.text = "Cuarto " + numeroCuarto;

        yield return UITransiciones.FadeIn(panel, duracionFade, false);
        yield return new WaitForSeconds(tiempoVisible);
        yield return UITransiciones.FadeOut(panel, duracionFade);
    }
}