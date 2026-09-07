using UnityEngine;

public class PantallaCamaras : MonoBehaviour
{
    public ControlCamaras sistemaCamaras;

    [Header("Detección por Crosshair")]
    public Camera camaraJugador;
    public float distanciaMaxima = 3f;
    public LayerMask capaInteractuable;

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

    public void SalirCamaras()
    {
        viendoCamaras = false;
        sistemaCamaras.Desactivar();
    }
}