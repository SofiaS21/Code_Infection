using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TiendaVirtual : MonoBehaviour
{
    public TiendaUI tienda;
    public float distanciaMax = 3f;
    public KeyCode tecla = KeyCode.E;


    void Update()
    {
        if (tienda.EstaAbierta) return;

        bool apretoE = Input.GetKeyDown(tecla);
        bool hizoClic = Input.GetMouseButtonDown(0); 

        if (!apretoE && !hizoClic) return;

        Ray ray = Camera.main.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0));
        if (Physics.Raycast(ray, out RaycastHit hit, distanciaMax) && hit.collider.gameObject == gameObject)
            tienda.Abrir();
    }
}
