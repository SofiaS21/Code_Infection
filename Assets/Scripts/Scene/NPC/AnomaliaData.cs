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
    public string nombreInterno;
    public TipoAnomalia tipo;
    public int diaMinimo = 2;

    [Header("Visual (modelos infectados)")]
    public Pacientes[] prefabsAnomalos;

    [Header("Secreto")]
    [TextArea] public string frase;

    public Pacientes ElegirPrefab()
    {
        var lista = new List<Pacientes>();
        if (prefabsAnomalos != null)
            foreach (var p in prefabsAnomalos)
                if (p != null) lista.Add(p);

        if (lista.Count == 0)
        {
            Debug.LogWarning("AnomaliaData '" + name + "' es Visual pero 'Prefabs Anomalos' está vacío");
            return null;
        }

        return lista[Random.Range(0, lista.Count)];
    }
}