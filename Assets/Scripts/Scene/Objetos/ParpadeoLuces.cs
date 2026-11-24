using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Un solo manager para todas las luces. Parpadea de vez en cuando desde sanidad 100
// y cada vez más seguido, más fuerte y con apagones largos a medida que baja la sanidad.
public class ParpadeoLuces : MonoBehaviour
{
    [Header("Luces (si lo dejás vacío busca todas las de la escena)")]
    public Light[] luces;
    public bool incluirDireccionales = false;

    [Header("Cada cuánto parpadea (segundos)")]
    public float esperaMinSanidad100 = 25f;
    public float esperaMaxSanidad100 = 50f;
    public float esperaMinSanidad0 = 2f;
    public float esperaMaxSanidad0 = 5f;

    float[] intensidadBase;

    void Start()
    {
        if (luces == null || luces.Length == 0)
        {
            List<Light> encontradas = new List<Light>();
            foreach (Light l in FindObjectsOfType<Light>())
            {
                if (!incluirDireccionales && l.type == LightType.Directional) continue;
                encontradas.Add(l);
            }
            luces = encontradas.ToArray();
        }

        intensidadBase = new float[luces.Length];
        for (int i = 0; i < luces.Length; i++)
            intensidadBase[i] = luces[i] != null ? luces[i].intensity : 0f;

        StartCoroutine(Bucle());
    }

    IEnumerator Bucle()
    {
        while (true)
        {
            float t = 1f - Mathf.Clamp01(SanidadActual() / 100f);   // 0 = sanidad 100, 1 = sanidad 0

            float min = Mathf.Lerp(esperaMinSanidad100, esperaMinSanidad0, t);
            float max = Mathf.Lerp(esperaMaxSanidad100, esperaMaxSanidad0, t);
            yield return new WaitForSeconds(Random.Range(min, max));

            if (luces.Length == 0) continue;

            t = 1f - Mathf.Clamp01(SanidadActual() / 100f);

            // Con sanidad baja es más probable que parpadeen todas juntas
            List<int> indices = new List<int>();
            if (Random.value < Mathf.Lerp(0.1f, 0.7f, t))
            {
                for (int i = 0; i < luces.Length; i++) indices.Add(i);
            }
            else
            {
                indices.Add(Random.Range(0, luces.Length));
            }

            yield return StartCoroutine(Parpadear(indices, t));
        }
    }

    IEnumerator Parpadear(List<int> indices, float t)
    {
        int veces = Random.Range(2, 4 + Mathf.RoundToInt(t * 4f));
        float piso = Mathf.Lerp(0.5f, 0f, t);   // qué tan oscuras se ponen

        for (int i = 0; i < veces; i++)
        {
            Poner(indices, Random.Range(0f, piso + 0.001f));
            yield return new WaitForSeconds(Random.Range(0.03f, 0.1f));

            Poner(indices, Random.Range(0.6f, 1f));
            yield return new WaitForSeconds(Random.Range(0.03f, 0.12f));
        }

        // Con sanidad muy baja, a veces queda apagada un ratito
        if (t > 0.6f && Random.value < 0.3f)
        {
            Poner(indices, 0f);
            yield return new WaitForSeconds(Random.Range(0.3f, 0.8f));
        }

        Poner(indices, 1f);
    }

    void Poner(List<int> indices, float multiplicador)
    {
        foreach (int i in indices)
        {
            if (luces[i] != null)
                luces[i].intensity = intensidadBase[i] * multiplicador;
        }
    }

    float SanidadActual()
    {
        return SanidadManager.Instance != null ? SanidadManager.Instance.Sanidad : 100f;
    }
}