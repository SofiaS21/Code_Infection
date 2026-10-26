using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanastaTienda : MonoBehaviour
{
    public Transform[] puntosItems;  

    List<ItemData> items = new List<ItemData>();

    public void Configurar(List<ItemData> lista)
    {
        items = new List<ItemData>(lista);

        for (int i = 0; i < items.Count && i < puntosItems.Length; i++)
        {
            if (items[i].modeloEnMano == null) continue;

            GameObject modelo = Instantiate(items[i].modeloEnMano, puntosItems[i]);
            modelo.transform.localPosition = Vector3.zero;
            modelo.transform.localRotation = Quaternion.identity;
            modelo.transform.localScale *= items[i].escalaEnMano;

            foreach (var col in modelo.GetComponentsInChildren<Collider>())
                col.enabled = false;
        }
    }

    public void Interact()
    {
        Inventario.instancia.ReemplazarItems(items);
        Destroy(gameObject);
    }

    public void OnFocus() { }
    public void OnUnfocus() { }
}
