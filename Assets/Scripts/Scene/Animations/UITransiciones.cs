using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public static class UITransiciones
{
    static CanvasGroup ObtenerCG(GameObject panel)
    {
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg == null) cg = panel.AddComponent<CanvasGroup>();
        return cg;
    }

    // Aparece con fade. Si saltableConClic, un clic lo muestra directo.
    public static IEnumerator FadeIn(GameObject panel, float duracion = 0.8f, bool saltableConClic = true)
    {
        var cg = ObtenerCG(panel);
        cg.alpha = 0f;
        panel.SetActive(true);

        float t = 0f;
        while (t < duracion)
        {
            if (saltableConClic && Input.GetMouseButtonDown(0)) break;
            t += Time.unscaledDeltaTime;
            cg.alpha = t / duracion;
            yield return null;
        }

        cg.alpha = 1f;
        yield return null;   // el mismo clic no cuenta dos veces
    }

    public static IEnumerator FadeOut(GameObject panel, float duracion = 0.8f)
    {
        var cg = ObtenerCG(panel);
        float t = 0f;
        while (t < duracion)
        {
            t += Time.unscaledDeltaTime;
            cg.alpha = 1f - (t / duracion);
            yield return null;
        }

        cg.alpha = 0f;
        panel.SetActive(false);
    }

    // Espera X segundos, pero un clic lo adelanta
    public static IEnumerator EsperarSegundosOClic(float segundos)
    {
        float t = 0f;
        while (t < segundos && !Input.GetMouseButtonDown(0))
        {
            t += Time.unscaledDeltaTime;
            yield return null;
        }
        yield return null;
    }

    // Bloquea los clics 'bloqueo' segundos y después espera un clic
    public static IEnumerator EsperarClic(float bloqueo = 0f)
    {
        if (bloqueo > 0f)
            yield return new WaitForSecondsRealtime(bloqueo);

        while (!Input.GetMouseButtonDown(0))
            yield return null;

        yield return null;
    }


}
