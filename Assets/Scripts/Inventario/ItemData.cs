using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum Enfermedad { DolorCabeza, Hemorragia, Infeccion }
[CreateAssetMenu(fileName = "Nuevo Item", menuName = "Inventario/Item")]

public class ItemData : ScriptableObject
{
    public string nombre;
    [TextArea] public string descripcion;

    public Sprite icono;
    public GameObject modeloEnMano;

    public float escalaEnMano = 1f;
    public Vector3 rotacionEnMano = Vector3.zero;

    public Enfermedad queCura;


}
