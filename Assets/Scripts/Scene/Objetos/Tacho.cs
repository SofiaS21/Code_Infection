using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tacho : MonoBehaviour, IInteractable
{
    public void Interact()
    {
        Inventario.instancia.TirarEquipado();
    }

    public void OnFocus() { }
    public void OnUnfocus() { }

}
