using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SlotUI : MonoBehaviour
{
    public Image imagenIcono;

    public void ActualizarSlot(ItemData nuevoItem)
    {
        if (nuevoItem != null)
        {
            imagenIcono.sprite = nuevoItem.icono;
            imagenIcono.enabled = true; // Muestra la imagen
        }
        else
        {
            LimpiarSlot();
        }
    }

    public void LimpiarSlot()
    {
        imagenIcono.sprite = null;
        imagenIcono.enabled = false; 
    }
}
