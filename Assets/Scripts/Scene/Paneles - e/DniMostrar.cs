using UnityEngine;

public class DniMostrar : MonoBehaviour, IInteractable
{
    public GameObject dniUI;
    public RectTransform panelRect; // el RectTransform del panel que se agranda/achica

    [Header("Cierre automático al alejarse")]
    public Transform jugador;
    public float distanciaMaxima = 3f;

    [Header("Animación de escala")]
    public float escalaMinima = 0.4f;   
    public float velocidadEscala = 8f; 

    bool abierto;

    void Update()
    {
        if (!abierto) return;

        if (jugador == null) return;

        float distancia = Vector3.Distance(jugador.position, transform.position);

        if (distancia > distanciaMaxima)
        {
            Cerrar();
            return;
        }

        float factor = 1f - (distancia / distanciaMaxima); // 1 = pegado, 0 = en el límite
        float escalaObjetivo = Mathf.Lerp(escalaMinima, 1f, factor);

        panelRect.localScale = Vector3.Lerp(
            panelRect.localScale,
            Vector3.one * escalaObjetivo,
            Time.deltaTime * velocidadEscala
        );
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

        panelRect.localScale = Vector3.zero; // arranca invisible/chiquito -> el Update lo hace "crecer"
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