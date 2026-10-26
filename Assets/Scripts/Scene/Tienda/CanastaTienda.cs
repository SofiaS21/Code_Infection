using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Collections;

public class CanastaTienda : MonoBehaviour
{
    public Transform[] puntosItems;
    public float escalaItems = 1f;

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
            modelo.transform.localScale *= items[i].escalaEnMano * escalaItems;

            PrepararFisica(modelo);
        }
    }
    void PrepararFisica(GameObject modelo)
    {
        foreach (var mf in modelo.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.GetComponent<Collider>() == null)
            {
                var mc = mf.gameObject.AddComponent<MeshCollider>();
                mc.sharedMesh = mf.sharedMesh;
            }
        }

        foreach (var mc in modelo.GetComponentsInChildren<MeshCollider>())
            mc.convex = true;

        foreach (var t in modelo.GetComponentsInChildren<Transform>())
            t.gameObject.tag = "Interactuable";

        var rb = modelo.GetComponent<Rigidbody>();
        if (rb == null) rb = modelo.AddComponent<Rigidbody>();
        rb.useGravity = true;
        rb.isKinematic = false;
        rb.collisionDetectionMode = CollisionDetectionMode.ContinuousDynamic;

    }


    public void Interact()
    {
        Inventario.instancia.ReemplazarItems(items);
        Destroy(gameObject);
    }

    public void OnFocus() { }
    public void OnUnfocus() { }
}
