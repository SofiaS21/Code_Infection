using UnityEngine;

public class PantallaCamaras : MonoBehaviour
{
    public GameObject canvasE;
    public CanvasGroup canvasGroup;
    public ControlCamaras sistemaCamaras;

    bool jugadorCerca;
    bool viendoCamaras;

    void Start()
    {
        canvasE.SetActive(false);
        canvasGroup.alpha = 0;
    }

    void Update()
    {
        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            if (!viendoCamaras) EntrarCamaras();
            else SalirCamaras();
        }

        if (!jugadorCerca && viendoCamaras)
            SalirCamaras();
    }

    void EntrarCamaras()
    {
        viendoCamaras = true;
        sistemaCamaras.Activar();
        canvasE.SetActive(false);
    }

    void SalirCamaras()
    {
        viendoCamaras = false;
        sistemaCamaras.Desactivar();

        if (jugadorCerca)
            canvasE.SetActive(true);
    }

    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = true;
            if (!viendoCamaras) canvasE.SetActive(true);
        }
    }

    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Player"))
        {
            jugadorCerca = false;
            canvasE.SetActive(false);
        }
    }
}