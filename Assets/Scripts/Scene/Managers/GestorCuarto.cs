using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GestorCuarto : MonoBehaviour
{
    public static GestorCuarto instancia;

    [System.Serializable]
    public class Cuarto
    {
        public Transform punto;   // el destino al que camina el NPC
        public bool ocupado = false;
    }

    public Cuarto[] cuartos;

    void Awake()
    {
        instancia = this;
    }

    public Cuarto ObtenerCuartoLibre()
    {
        foreach (Cuarto c in cuartos)
        {
            if (!c.ocupado)
            {
                c.ocupado = true;
                return c;
            }
        }

        Debug.LogWarning("No hay cuartos libres");
        return null;
    }

    public void LiberarCuarto(Cuarto c)
    {
        if (c != null)
            c.ocupado = false;
    }
}
