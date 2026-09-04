using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class DniDisplay : MonoBehaviour
{
 public static DniDisplay Instancia;

    public Image imagenDniUI; // arrastrá acá el Image de tu Canvas donde va la foto del DNI

    void Awake()
    {
        Instancia = this;
    }

    public void Mostrar(Sprite sprite)
    {
        if (imagenDniUI == null) return;

        if (sprite == null)
        {
            // Todavía no tenés la imagen final, se oculta para no romper nada
            imagenDniUI.enabled = false;
            return;
        }

        imagenDniUI.sprite = sprite;
        imagenDniUI.enabled = true;
    }

    public void Ocultar()
    {
        if (imagenDniUI != null)
            imagenDniUI.enabled = false;
    }
}
