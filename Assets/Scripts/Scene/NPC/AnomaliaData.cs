using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public enum TipoAnomalia
{
    NombreIncorrecto,
    FotoDiferente,
    Visual,
    DialogoRaro,
    VisualCamara,
}

[CreateAssetMenu(menuName = "Juego/Anomalia")]
public class AnomaliaData : ScriptableObject
{
    public string nombreInterno;
    public TipoAnomalia tipo;
    public int diaMinimo = 2;

    [Header("Visual (modelos infectados)")]
    public Pacientes[] prefabsAnomalos;   // los 4: 2 perros y 2 gatos infectados

    [Header("Secreto")]
    [TextArea] public string frase;

    // Elige al azar uno de los modelos infectados cargados
    public Pacientes ElegirPrefab()
    {
        var lista = new List<Pacientes>();
        if (prefabsAnomalos != null)
            foreach (var p in prefabsAnomalos)
                if (p != null) lista.Add(p);

        if (lista.Count == 0)
        {
            return null;
        }

        return lista[Random.Range(0, lista.Count)];
    }
}