using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TipoAnomalia
{
    NombreIncorrecto,
    FotoDiferente,
    Visual,
    DialogoRaro,
}

[CreateAssetMenu(menuName = "Juego/Anomalia")]
public class AnomaliaData : ScriptableObject
{
    public Pacientes prefabAnomalo;
    public string nombreInterno;
    public TipoAnomalia tipo;
    public int diaMinimo = 2;

    [Header("Visual")]
    public GameObject prefabVisual;

    [Header("Secreto")]
    [TextArea] public string frase;
}


