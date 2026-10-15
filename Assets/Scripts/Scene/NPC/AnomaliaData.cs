using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum TipoAnomalia
{
    NombreIncorrecto,
    DniVencido,
    Visual,
    DialogoRaro
}

[CreateAssetMenu(menuName = "Juego/Anomalia")]
public class AnomaliaData : ScriptableObject
{

    public string nombreInterno;
    public TipoAnomalia tipo;
    public int diaMinimo = 2;

    [Header("Visual")]
    public GameObject prefabVisual;

    [Header("Secreto")]
    [TextArea] public string frase;


    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
