using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuertaAnimation : MonoBehaviour
{
    public Animator anim;

    [Header("Rango de interacción")]
    public float rangoFrente = 3f;   // alcance hacia adelante/atrás (más largo)
    public float rangoCostado = 1f;  // alcance hacia los lados (más corto)

    private Transform jugador;
    private bool puertaAbierta = false;

    void Start()
    {
        anim = GetComponent<Animator>();
        jugador = GameObject.FindGameObjectWithTag("Player").transform;
    }

    void OnMouseDown()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (JugadorEnRango())
            {
                puertaAbierta = !puertaAbierta;
                anim.SetBool("PuertaAbierta", puertaAbierta);
            }
            else
            {
                Debug.Log("Estás muy lejos o de costado a la puerta");
            }
        }
    }

    bool JugadorEnRango()
    {

        Vector3 posLocal = transform.InverseTransformPoint(jugador.position);
        bool dentroFrente = Mathf.Abs(posLocal.z) <= rangoFrente;
        bool dentroCostado = Mathf.Abs(posLocal.x) <= rangoCostado;

        return dentroFrente && dentroCostado;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(rangoCostado * 2, 2f, rangoFrente * 2));
    }
}