using UnityEngine;

public class PantallaCamaras : MonoBehaviour
{
    public ControlCamaras sistemaCamaras;

    [Header("Detección por Crosshair")]
    public Camera camaraJugador;
    public float distanciaMaxima = 3f;
    public LayerMask capaInteractuable;

    [Header("Salida por distancia")]
    public Transform player;
    public Transform puntoAtencion;
    public float distanciaSalida = 4f;

    bool mirandoTele;
    bool viendoCamaras;

    void Update()
    {
        DetectarConCrosshair();

        // Click izquierdo = "tocar" la tele
        if (mirandoTele && !viendoCamaras && Input.GetMouseButtonDown(0))
        {
            EntrarCamaras();
        }

        if (mirandoTele && !viendoCamaras && Input.GetMouseButtonDown(0))
        {
            EntrarCamaras();
        }

        if (viendoCamaras)
        {
            if (Input.GetKeyDown(KeyCode.E))
            {
                SalirCamaras();
                return;
            }

            if (player != null && puntoAtencion != null)
            {
                float distancia = Vector3.Distance(player.position, puntoAtencion.position);
                if (distancia > distanciaSalida)
                    SalirCamaras();
            }

        }

        void DetectarConCrosshair()
        {
            if (camaraJugador == null) return;

            Ray ray = camaraJugador.ViewportPointToRay(new Vector3(0.5f, 0.5f, 0f));
            RaycastHit hit;

            if (Physics.Raycast(ray, out hit, distanciaMaxima, capaInteractuable))
                mirandoTele = hit.collider.gameObject == this.gameObject;
            else
                mirandoTele = false;
        }


        void EntrarCamaras()
        {
            viendoCamaras = true;
            sistemaCamaras.Activar();
        }

        void SalirCamaras()
        {
            viendoCamaras = false;
            sistemaCamaras.Desactivar();
        }
    }
}