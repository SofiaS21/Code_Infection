using UnityEngine;


public class PosterMostrar : MonoBehaviour, IInteractable
{
    [Header("Panel del póster")]
    public GameObject posterUI;
    public GameObject inventarioUI;
    public GameObject sanidadUI;

    [Header("Cierre automático al alejarse")]
    public Transform jugador;
    public float distanciaMaxima = 3f;

    [Header("Tecla para cerrar")]
    public KeyCode teclaCerrar = KeyCode.Q;

    public static bool PosterAbierto { get; private set; }

    bool abierto;
    int frameAbierto = -1;
    int frameCerrado = -1;
    float timeScaleAnterior = 1f;

    void Update()
    {
        if (!abierto) return;

        if (Input.GetKeyDown(teclaCerrar) && Time.frameCount != frameAbierto)
        {
            Cerrar();
            return;
        }

        if (jugador != null &&
            Vector3.Distance(jugador.position, transform.position) > distanciaMaxima)
        {
            Cerrar();
        }
    }

    public void Interact()
    {
        // Evita reabrir en el mismo frame en que se cerró
        if (Time.frameCount == frameCerrado) return;

        if (abierto) Cerrar();
        else Abrir();
    }

    void Abrir()
    {
        if (inventarioUI != null) inventarioUI.SetActive(false);
        if (sanidadUI != null) sanidadUI.SetActive(false);
        posterUI.SetActive(true);

        abierto = true;
        PosterAbierto = true;          // <-- faltaba
        frameAbierto = Time.frameCount; // <-- faltaba

        timeScaleAnterior = Time.timeScale;
        Time.timeScale = 0f;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    void Cerrar()
    {
        posterUI.SetActive(false);
        if (inventarioUI != null) inventarioUI.SetActive(true);
        if (sanidadUI != null) sanidadUI.SetActive(true);

        abierto = false;
        PosterAbierto = false;
        frameCerrado = Time.frameCount;

        Time.timeScale = timeScaleAnterior;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void OnDisable()
    {
        if (abierto)
        {
            Time.timeScale = timeScaleAnterior;
            PosterAbierto = false;
            abierto = false;
        }
    }

    public void OnFocus() { }
    public void OnUnfocus() { }
}