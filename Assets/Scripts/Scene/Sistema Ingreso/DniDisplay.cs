using UnityEngine;
using UnityEngine.UI;

public class DniDisplay : MonoBehaviour
{
    public static DniDisplay Instancia;

    public RawImage imagenDniUI;

    void Awake()
    {
        Instancia = this;
    }

    public void Mostrar(Texture textura)
    {
        if (imagenDniUI == null) return;

        if (textura == null)
        {
            imagenDniUI.enabled = false;
            return;
        }

        imagenDniUI.texture = textura;
        imagenDniUI.enabled = true;
    }

    public void Ocultar()
    {
        if (imagenDniUI != null)
            imagenDniUI.enabled = false;
    }
}