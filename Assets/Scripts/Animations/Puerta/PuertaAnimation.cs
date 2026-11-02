using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuertaAnimation : MonoBehaviour, IInteractable
{
    public Animator anim;

    [Header("Rango de interacción")]
    public bool usarRangoRectangular = true;
    public float rangoFrente = 3f;   // alcance hacia adelante/atrás (más largo)
    public float rangoCostado = 1f;  // alcance hacia los lados (más corto)
    public float rangoLongitud = 3f;

    private Transform jugador;
    private bool puertaAbierta = false;

    void Start()
    {
        if (anim == null)
            anim = GetComponent<Animator>();

        jugador = GameObject.FindGameObjectWithTag("Player").transform;
    }

    public void Interact()
    {
        if (!Input.GetMouseButtonDown(0)) return;
        
        if (usarRangoRectangular && !JugadorEnRango())
        {
            Debug.Log("Estás muy lejos o de costado a la puerta");
            return;
        }
        puertaAbierta = !puertaAbierta;
        anim.SetBool("PuertaAbierta", puertaAbierta);
    }

    public void OnFocus() { }
    public void OnUnfocus() { }

    bool JugadorEnRango()
    {

        Vector3 posLocal = transform.InverseTransformPoint(jugador.position);
        bool dentroFrente = Mathf.Abs(posLocal.z) <= rangoFrente;
        bool dentroCostado = Mathf.Abs(posLocal.x) <= rangoCostado;
        bool dentroLongitud = Mathf.Abs(posLocal.y) <= rangoLongitud;
        return dentroFrente && dentroCostado && dentroLongitud;
    }

    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.matrix = transform.localToWorldMatrix;
        Gizmos.DrawWireCube(Vector3.zero, new Vector3(rangoCostado * 2, 2f, rangoFrente * 2));
    }
}