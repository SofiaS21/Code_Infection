using System.Collections.Generic;
using UnityEngine;

public class PuertaAutomatica : MonoBehaviour
{
    public Animator animPuerta;
    public float tiempoAntesDeCerrar = 2.5f;

    [Header("Modo")]
    [Tooltip("ON = puerta de cuarto: abre solo para el paciente autorizado (por distancia). OFF = entrada/salida: abre para cualquier paciente que entre al trigger.")]
    public bool soloAutorizados = true;

    [Header("Solo autorizados")]
    public Transform puntoReferencia;     // opcional: centro de la puerta
    public float distanciaAbrir = 4f;
    public float distanciaCerrar = 5f;

    private const string parametroAbierta = "PuertaAbierta";

    // Puertas de entrada/salida (trigger): paciente -> cantidad de colliders suyos adentro
    private readonly Dictionary<Pacientes, int> enTrigger = new Dictionary<Pacientes, int>();
    private readonly List<Pacientes> paraQuitar = new List<Pacientes>();

    // Puertas de cuarto
    private readonly List<Pacientes> autorizados = new List<Pacientes>();
    private readonly HashSet<Pacientes> estuvoCerca = new HashSet<Pacientes>();

    private bool abierta;
    private float tiempoSinNadie;

    // ---------- Puertas de cuarto ----------
    public void Autorizar(Pacientes p)
    {
        if (p != null && !autorizados.Contains(p)) autorizados.Add(p);
    }

    public void Desautorizar(Pacientes p)
    {
        autorizados.Remove(p);
        estuvoCerca.Remove(p);
    }

    // ---------- Puertas de entrada/salida (trigger) ----------
    void OnTriggerEnter(Collider other)
    {
        if (soloAutorizados) return;

        Pacientes p = other.GetComponentInParent<Pacientes>();
        if (p == null) return;

        int n;
        enTrigger.TryGetValue(p, out n);
        enTrigger[p] = n + 1;
    }

    void OnTriggerExit(Collider other)
    {
        if (soloAutorizados) return;

        Pacientes p = other.GetComponentInParent<Pacientes>();
        if (p == null) return;

        int n;
        if (!enTrigger.TryGetValue(p, out n)) return;

        n--;
        if (n <= 0) enTrigger.Remove(p);
        else enTrigger[p] = n;
    }

    void Update()
    {
        // Limpia pacientes destruidos
        paraQuitar.Clear();
        foreach (var kv in enTrigger)
            if (kv.Key == null) paraQuitar.Add(kv.Key);
        foreach (var p in paraQuitar) enTrigger.Remove(p);

        autorizados.RemoveAll(p => p == null);

        bool hayAlguien = soloAutorizados ? HayAutorizadoCerca() : enTrigger.Count > 0;

        if (hayAlguien)
        {
            tiempoSinNadie = 0f;
            if (!abierta) SetAbierta(true);
        }
        else if (abierta)
        {
            tiempoSinNadie += Time.deltaTime;
            if (tiempoSinNadie >= tiempoAntesDeCerrar) SetAbierta(false);
        }
    }

    bool HayAutorizadoCerca()
    {
        Vector3 centro = (puntoReferencia != null ? puntoReferencia : transform).position;
        bool hay = false;

        for (int i = autorizados.Count - 1; i >= 0; i--)
        {
            Pacientes p = autorizados[i];
            float d = DistanciaPlana(p.transform.position, centro);

            if (d <= (abierta ? distanciaCerrar : distanciaAbrir))
            {
                hay = true;
                estuvoCerca.Add(p);
            }
            else if (estuvoCerca.Contains(p))
            {
                // ya pasó por la puerta y se alejó: deja de estar autorizado
                autorizados.RemoveAt(i);
                estuvoCerca.Remove(p);
            }
        }

        return hay;
    }

    void SetAbierta(bool valor)
    {
        abierta = valor;
        if (animPuerta != null) animPuerta.SetBool(parametroAbierta, valor);
    }

    float DistanciaPlana(Vector3 a, Vector3 b)
    {
        a.y = 0f;
        b.y = 0f;
        return Vector3.Distance(a, b);
    }
}