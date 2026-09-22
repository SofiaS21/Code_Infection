using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;


public class SeguidorDestino : MonoBehaviour
{
    public Transform destino;
    private NavMeshAgent agente;

    void Start()
    {
        agente = GetComponent<NavMeshAgent>();
    }

    void Update()
    {
        if (destino != null)
        {
            agente.SetDestination(destino.position);
        }
    }

    public void CambiarDestino(Transform nuevoDestino)
    {
        destino = nuevoDestino;
    }
}
