using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PuertaAnimation : MonoBehaviour
{

    public Animator anim;
    public float distanciaMaxima = 3f;

    private Transform jugador;

    // Start is called before the first frame update
    void Start()
    {
        anim = GetComponent<Animator>();
        jugador = GameObject.FindGameObjectWithTag("Player").transform; 
    }

    // Update is called once per frame
    void Update()
    {
        float distancia = Vector3.Distance(transform.position, jugador.position);

        if (distancia <= distanciaMaxima)
        {
            anim.SetTrigger("Presionar");
        }
    }
}
