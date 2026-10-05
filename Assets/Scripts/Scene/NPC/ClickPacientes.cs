using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClickPacientes : MonoBehaviour
{

    public float distanciaMaxima = 30f;

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            Ray rayo = new Ray(transform.position, transform.forward);


            if (Physics.Raycast(rayo, out RaycastHit hit, distanciaMaxima))
            {
                Pacientes paciente = hit.collider.gameObject.GetComponent<Pacientes>();

                if (paciente != null)
                    FindObjectOfType<EsperaNPC>().MostrarDNI();
            }
        }
    }
}
