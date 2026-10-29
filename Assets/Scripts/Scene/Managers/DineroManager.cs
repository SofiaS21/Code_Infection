using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class DineroManager : MonoBehaviour
{
    public static DineroManager instancia;

    [Header("Valores")]
    public int dineroInicial = 100;
    public int precioItem = 15;
    public int premioCurar = 20;
    public int castigoError = 10;

    [Header("UI")]
    public TMP_Text textoDinero; 

    public int Dinero { get; private set; }
    public int GananciasDia { get; private set; }   // solo curados/errores, SIN compras
    public int ComprasDia { get; private set; }

    void Awake()
    {
        instancia = this;
        Dinero = dineroInicial;
        ActualizarUI();
    }

    public void PacienteCurado()
    {
        Sumar(premioCurar);
    }

    // cuando rechazas un normal o se fue
    public void ErrorPacienteNormal()
    {
        Sumar(-castigoError);
    }

    void Sumar(int cantidad)
    {
        GananciasDia += cantidad;
        Dinero = Mathf.Max(0, Dinero + cantidad);
        ActualizarUI();
    }

    public bool PuedePagar(int total) => Dinero >= total;

    public bool Gastar(int total)
    {
        if (!PuedePagar(total)) return false;
        Dinero -= total;
        ComprasDia += total;
        ActualizarUI();
        return true;
    }

    public void NuevoDia()
    {
        GananciasDia = 0;
        ComprasDia = 0;
    }

    void ActualizarUI()
    {
        if (textoDinero != null) textoDinero.text = "$" + Dinero;
    }
}
