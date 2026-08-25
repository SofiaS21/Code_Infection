using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Nuevo Item", menuName = "Inventario/Item")]

public class ItemData : ScriptableObject
{
    public string nombre;
    [TextArea] public string descripcion;
    public Sprite icono;
    public GameObject modeloEnMano;

}
