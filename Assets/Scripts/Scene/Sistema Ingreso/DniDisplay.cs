using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DniDisplay : MonoBehaviour
{
    public static DniDisplay Instancia;

    public RawImage imagenDniUI;

    [Header("Detección por distancia")]
    public Transform player;          
    public Transform puntoAtencion;  
    public float distanciaMaxima = 3f;

    bool jugadorCerca;
    bool viendoDni;

    void Awake()
    {
        Instancia = this;
    }

    public void Update()
    {
        if (player != null && puntoAtencion != null)
        {
            float distancia = Vector3.Distance(player.position, puntoAtencion.position);
            jugadorCerca = distancia <= distanciaMaxima;
        }

        if (jugadorCerca && Input.GetKeyDown(KeyCode.E))
        {
            if (!viendoDni) EntrarDni();
            else SalirDni();
        }

        if (!jugadorCerca && viendoDni)
            SalirDni();
    }

    void EntrarDni()
    {
        viendoDni = true;
        if (imagenDniUI != null)
            imagenDniUI.enabled = true;
    }

    void SalirDni()
    {
        viendoDni = false;
        if (imagenDniUI != null)
            imagenDniUI.enabled = false;
    }

    public void Mostrar(Texture textura)
    {
        if (imagenDniUI == null) return;

        if (textura == null)
        {
            // Todavía no tenés la imagen final, se oculta para no romper nada
            imagenDniUI.enabled = false;
            return;
        }

        imagenDniUI.texture = textura;
    }

    public void Ocultar()
    {
        if (imagenDniUI != null)
            imagenDniUI.enabled = false;
    }
}
