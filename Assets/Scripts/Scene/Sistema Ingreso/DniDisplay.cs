using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DniDisplay : MonoBehaviour
{
    public static DniDisplay Instancia;

    public RawImage imagenDniUI;
    public TMP_Text nombreTexto;
    public TMP_Text fechaVencimientoTexto;

    void Awake()
    {
        Instancia = this;
    }

    //muestra el DNI del paciente que está en el counter
    public void MostrarActual()
    {
        Pacientes p = Pacientes.Actual;
        if (p == null) return;

        Mostrar(p.dniImagen, p.nombre, p.dniFechaVencimiento);
    }

    public void Mostrar(Texture textura, string nombre, string fechaVencimiento)
    {
        if (imagenDniUI == null) return;

        if (textura == null)
        {
            Ocultar();
            return;
        }

        imagenDniUI.texture = textura;
        imagenDniUI.enabled = true;

        if (nombreTexto != null)
            nombreTexto.text = nombre;

        if (fechaVencimientoTexto != null)
            fechaVencimientoTexto.text = fechaVencimiento;
    }

    public void Ocultar()
    {
        if (imagenDniUI != null)
            imagenDniUI.enabled = false;

        if (nombreTexto != null)
            nombreTexto.text = "";

        if (fechaVencimientoTexto != null)
            fechaVencimientoTexto.text = "";
    }
}