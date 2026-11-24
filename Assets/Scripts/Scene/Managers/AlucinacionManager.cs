using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class AlucinacionManager : MonoBehaviour
{
    [Header("Qué aparece (prefabs de pacientes, normales e infectados)")]
    public GameObject[] prefabsAlucinacion;

    [Header("Dónde aparece (objetos vacíos que ponés en cuartos, pasillo, etc.)")]
    public Transform[] puntosAparicion;

    [Header("Jugador")]
    public Transform jugador;         
    public Camera camaraJugador;      

    [Header("Cuándo")]
    [Tooltip("Por debajo de esta sanidad empiezan las alucinaciones")]
    public float sanidadMaxima = 60f;
    [Tooltip("Segundos entre alucinaciones justo en 'Sanidad Maxima'")]
    public float intervaloMax = 60f;
    [Tooltip("Segundos entre alucinaciones con sanidad 0")]
    public float intervaloMin = 8f;

    [Header("Cómo")]
    public float duracionMin = 0.6f;
    public float duracionMax = 1.2f;
    public float distanciaMin = 2f;
    public float distanciaMax = 12f;
    [Tooltip("Solo aparece en puntos que están dentro de la pantalla")]
    public bool soloSiEstaEnPantalla = true;
    [Range(0f, 1f)] public float fuerzaPulso = 0.5f;

    void Start()
    {
        if (camaraJugador == null) camaraJugador = Camera.main;
        if (jugador == null && camaraJugador != null) jugador = camaraJugador.transform;

        StartCoroutine(Bucle());
    }

    IEnumerator Bucle()
    {
        while (true)
        {
            float s = SanidadActual();

            if (s >= sanidadMaxima)
            {
                yield return new WaitForSeconds(2f);
                continue;
            }

            float t = Mathf.InverseLerp(sanidadMaxima, 0f, s);
            float espera = Mathf.Lerp(intervaloMax, intervaloMin, t) * Random.Range(0.7f, 1.3f);
            yield return new WaitForSeconds(espera);

            if (SanidadActual() >= sanidadMaxima) continue;

            yield return StartCoroutine(Alucinar());
        }
    }

    float SanidadActual()
    {
        return SanidadManager.Instance != null ? SanidadManager.Instance.Sanidad : 100f;
    }

    IEnumerator Alucinar()
    {
        if (prefabsAlucinacion == null || prefabsAlucinacion.Length == 0) yield break;
        if (jugador == null || camaraJugador == null) yield break;

        Transform punto = ElegirPunto();
        if (punto == null) yield break;

        GameObject prefab = prefabsAlucinacion[Random.Range(0, prefabsAlucinacion.Length)];
        if (prefab == null) yield break;

        GameObject go = Instantiate(prefab, punto.position, punto.rotation);
        Limpiar(go);

        // Mira hacia el jugador
        Vector3 dir = jugador.position - go.transform.position;
        dir.y = 0f;
        if (dir.sqrMagnitude > 0.001f)
            go.transform.rotation = Quaternion.LookRotation(dir);

        if (SanidadManager.Instance != null)
            SanidadManager.Instance.PulsoSusto(fuerzaPulso);

        yield return new WaitForSeconds(Random.Range(duracionMin, duracionMax));

        if (go != null) Destroy(go);
    }

    Transform ElegirPunto()
    {
        List<Transform> validos = new List<Transform>();

        foreach (Transform p in puntosAparicion)
        {
            if (p == null) continue;

            float d = Vector3.Distance(jugador.position, p.position);
            if (d < distanciaMin || d > distanciaMax) continue;

            if (soloSiEstaEnPantalla)
            {
                Vector3 vp = camaraJugador.WorldToViewportPoint(p.position + Vector3.up);
                if (vp.z <= 0f || vp.x < 0.05f || vp.x > 0.95f || vp.y < 0f || vp.y > 1f) continue;
            }

            validos.Add(p);
        }

        if (validos.Count == 0) return null;
        return validos[Random.Range(0, validos.Count)];
    }

    // Deja solo el modelo y la animación (sin scripts, agente, colliders ni físicas)
    void Limpiar(GameObject go)
    {
        foreach (var mb in go.GetComponentsInChildren<MonoBehaviour>(true)) Destroy(mb);
        foreach (var ag in go.GetComponentsInChildren<NavMeshAgent>(true)) Destroy(ag);
        foreach (var col in go.GetComponentsInChildren<Collider>(true)) Destroy(col);
        foreach (var rb in go.GetComponentsInChildren<Rigidbody>(true)) Destroy(rb);
    }
}