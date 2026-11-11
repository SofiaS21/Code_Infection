using System.Collections;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class EfectoAtaque : MonoBehaviour
{
    public static EfectoAtaque Instance;

    [Header("Volumen de post-processing del ataque")]
    public PostProcessVolume volumen;

    [Header("Tiempos")]
    public float duracionEntrada = 0.15f;   // qué tan rápido aparece el efecto
    public float duracionSalida = 1.0f;     // qué tan lento se va

    void Awake()
    {
        Instance = this;
        if (volumen != null) volumen.weight = 0f;
    }

    public void Reproducir(float sostener)
    {
        if (volumen == null) return;

        StopAllCoroutines();
        StartCoroutine(Rutina(sostener));
    }

    IEnumerator Rutina(float sostener)
    {
        // Entra
        float t = 0f;
        while (t < duracionEntrada)
        {
            t += Time.deltaTime;
            volumen.weight = Mathf.Clamp01(t / duracionEntrada);
            yield return null;
        }
        volumen.weight = 1f;

        // Se mantiene
        yield return new WaitForSeconds(sostener);

        // Sale
        t = 0f;
        while (t < duracionSalida)
        {
            t += Time.deltaTime;
            volumen.weight = 1f - Mathf.Clamp01(t / duracionSalida);
            yield return null;
        }
        volumen.weight = 0f;
    }
}