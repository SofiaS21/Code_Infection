using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Teletransportador : MonoBehaviour
{
    public Transform destino;

    private void OnTriggerEnter(Collider other)
    {
        // Verifica si el objeto que entró al trigger es el jugador
        if (other.CompareTag("Player"))
        {
            // Si usas CharacterController, debes apagarlo un milisegundo para evitar conflictos de físicas
            CharacterController cc = other.GetComponent<CharacterController>();

            if (cc != null)
            {
                cc.enabled = false;
            }

            // Mueve al jugador a la posición del destino
            other.transform.position = destino.position;

            // Si tiene rotación el destino, también la aplica
            other.transform.rotation = destino.rotation;

            if (cc != null)
            {
                cc.enabled = true;
            }
        }
    }
}