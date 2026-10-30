using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanastaTienda : MonoBehaviour, IInteractable
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
        foreach (var c in modelo.GetComponentsInChildren<Collider>())
            Destroy(c);

        float grosorMinimo = 0.06f;
        foreach (var mf in modelo.GetComponentsInChildren<MeshFilter>())
        {
            if (mf.sharedMesh == null) continue;

            var box = mf.gameObject.AddComponent<BoxCollider>();
            Bounds b = mf.sharedMesh.bounds;
            Vector3 escala = mf.transform.lossyScale;

            Vector3 size = b.size;
            for (int i = 0; i < 3; i++)
            {
                float minLocal = grosorMinimo / Mathf.Max(Mathf.Abs(escala[i]), 0.0001f);
                size[i] = Mathf.Max(size[i], minLocal);
            }

            box.center = b.center;
            box.size = size;
        }

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
