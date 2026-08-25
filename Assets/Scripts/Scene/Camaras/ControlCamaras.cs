using UnityEngine;
using TMPro;

public class ControlCamaras : MonoBehaviour
{
    public Camera[] camaras;
    public GameObject panelCamaras; 
    public GameObject panelInventario; 
    public TextMeshProUGUI textoNombreCamara;

    int indiceActual = 0;

    void Start()
    {
        foreach (Camera cam in camaras)
            cam.enabled = false;

        panelCamaras.SetActive(false);
    }

    void Update()
    {
        if (!panelCamaras.activeSelf) return; // si el panel está cerrado, ignoramos flechas

        if (Input.GetKeyDown(KeyCode.RightArrow)) 
            CambiarCamara(1);

        if (Input.GetKeyDown(KeyCode.LeftArrow))
            CambiarCamara(-1);
    }

    public void Activar()
    {
        panelCamaras.SetActive(true);
        panelInventario.SetActive(false);
        MostrarCamara(indiceActual);
    }

    public void Desactivar()
    {
        camaras[indiceActual].enabled = false;
        panelCamaras.SetActive(false);
        panelInventario.SetActive(false);
    }

    void CambiarCamara(int direccion)
    {
        camaras[indiceActual].enabled = false; // apagamos la vieja

        indiceActual += direccion;
        if (indiceActual >= camaras.Length) indiceActual = 0;
        if (indiceActual < 0) indiceActual = camaras.Length - 1;

        MostrarCamara(indiceActual);
    }

    void MostrarCamara(int i)
    {
        camaras[i].enabled = true;
        textoNombreCamara.text = "CAMARA " + (i + 1); // se actualiza SOLO el texto
    }
}