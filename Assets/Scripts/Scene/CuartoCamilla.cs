using UnityEngine;
using UnityEngine.UI;

public class CuartoCamilla : MonoBehaviour
{
    [Header("Dónde se acuesta")]
    public Transform puntoAcostado;

    [Header("Dónde aparece al levantarse (en el piso, al lado de la cama)")]
    public Transform puntoLevantarse;

    [Header("Burbuja de la camilla")]
    public GameObject canvasBurbuja;     
    public RawImage iconoImage;             
    public RawImage iconoRawImage;       

    public bool Ocupado { get; private set; }

    void Awake()
    {
        if (puntoAcostado == null) puntoAcostado = transform;
        OcultarBurbuja();
    }

    public void Ocupar()
    {
        Ocupado = true;
    }

    public void Liberar()
    {
        Ocupado = false;
        OcultarBurbuja();
    }

    public void MostrarBurbuja(ItemData item)
    {
        if (item == null || item.icono == null) return;

        if (iconoRawImage != null)
            iconoRawImage.texture = item.icono.texture;

        if (iconoRawImage != null)
            iconoRawImage.texture = item.icono.texture;

        if (canvasBurbuja != null)
            canvasBurbuja.SetActive(true);
    }

    public void OcultarBurbuja()
    {
        if (canvasBurbuja != null)
            canvasBurbuja.SetActive(false);
    }
}