using UnityEngine;

public class DniMostrar : MonoBehaviour, IInteractable
{
    public GameObject dniUI;

    [Header("Cierre automático al alejarse")]
    public Transform jugador;
    public float distanciaMaxima = 3f;

    bool abierto;

    void Update()
    {
        if (abierto && jugador != null)
        {
            float distancia = Vector3.Distance(jugador.position, transform.position);
            if (distancia > distanciaMaxima)
                Cerrar();
        }
    }

    public void Interact()
    {
        if (abierto) Cerrar();
        else Abrir();
    }

    void Abrir()
    {

        if (Pacientes.Actual == null) return;

        DniDisplay.Instancia.Mostrar(
            Pacientes.Actual.dniImagen,
            Pacientes.Actual.nombre,
            Pacientes.Actual.dniFechaVencimiento
        );

        dniUI.SetActive(true);
        abierto = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Cerrar()
    {
        dniUI.SetActive(false);
        DniDisplay.Instancia.Ocultar();
        abierto = false;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void OnFocus() { }
    public void OnUnfocus() { }
}