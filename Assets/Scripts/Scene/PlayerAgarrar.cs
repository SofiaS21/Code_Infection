using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAgarrar : MonoBehaviour
{
    [SerializeField] private float rango = 3.5f;         // qué tan lejos puede agarrar objetos
    [SerializeField] private float radioCrosshair = 0.3f; // qué tan "permisivo" es apuntar
    [SerializeField] private LayerMask capaAgarrable;     // qué capa de objetos puede agarrar

    private PlayerAgarrar objetoActual;

    void Update()
    {
        Ray ray = new Ray(transform.position, transform.forward);

        if (Physics.SphereCast(ray, radioCrosshair, out RaycastHit hit, rango, capaAgarrable))
        {
            // por ahora no hacemos nada más, solo detectamos
            Debug.Log("Apuntando a: " + hit.collider.name);
        }
    }
}
