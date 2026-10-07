using System.Collections;
using System.Collections.Generic;
using UnityEditor.Search;
using UnityEngine;

public class ItemEnMano : MonoBehaviour
{

    private int indiceEquipado = -1;
    public ItemData ItemEquipado
    {
        get { return indiceEquipado >= 0 ? items[indiceEquipado] : null; }
    }

    public void ConsumirEquipado()
    {
        if (indiceEquipado >= 0) return;
        if (modelObjetoEquipado != null) Destroy(modelObjetoEquipado);
        QuitarItem(indiceEquipado);
        indiceEquipado = -1;
    } 
}
