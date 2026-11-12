using UnityEngine;

public class PantallaCamaras : MonoBehaviour
{
    public ControlCamaras sistemaCamaras;

    [Header("Crosshair")]
    public Camera camaraJugador;
    public float distanciaMaxima = 3f;
    public LayerMask capaTele;

    [Header("Salida")]
    public Transform player;
    public float distanciaSalida = 1.5f;

    bool viendoCamaras;
    Vector3 posicionAlEntrar;

    void Update()
    {
        if (viendoCamaras)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                SalirCamaras();
                return;
            }

            if (player != null &&
                Vector3.Distance(player.position, posicionAlEntrar) > distanciaSalida)
            {
                SalirCamaras();
            }
            return;
        }

        if (Input.GetMouseButtonDown(0) && CrosshairTocaTele())
            EntrarCamaras();
    }

    bool CrosshairTocaTele()
    {
        if (camaraJugador == null) return false;

        Ray ray = camaraJugador.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
        return Physics.Raycast(ray, distanciaMaxima, capaTele, QueryTriggerInteraction.Collide);
    }

    void EntrarCamaras()
    {
        viendoCamaras = true;
        if (player != null) posicionAlEntrar = player.position;
        sistemaCamaras.Activar();
    }

    public void SalirCamaras()
    {
        if (!viendoCamaras) return;
        viendoCamaras = false;
        sistemaCamaras.Desactivar();
    }
}