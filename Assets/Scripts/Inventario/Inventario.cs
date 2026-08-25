using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Inventario : MonoBehaviour
{
    public ItemData[] items; 
    public SlotUI[] slotsUI;
    public Transform objetoEnMano;     
    public GameObject canvasInvLleno; 
    private GameObject modelObjetoEquipado; 

    public static Inventario instancia;
    
    void Start()
    {
        canvasInvLleno.SetActive(false);
    }

    void Update()
    {
        
        for (int i = 0; i < items.Length; i++)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1 + i))
            {
                EquiparItem(i);
            }
        }
    }

     void EquiparItem(int index)
    {
        if (items[index] == null) return; // si hay slot vacio

        if (modelObjetoEquipado != null)
        {
            Destroy(modelObjetoEquipado);
        }

        // Si el item tiene modelo asignado
        if (items[index].modeloEnMano != null)
        {
            modelObjetoEquipado = Instantiate(
                items[index].modeloEnMano,
                objetoEnMano.position,
                objetoEnMano.rotation,
                objetoEnMano
            );
        }
    }

     void Awake()
    {
        instancia = this;
    }

    public bool AgregarItem(ItemData nuevoItem)
    {
        for (int i = 0; i < items.Length; i++)
        {
            if (items[i] == null)
            {
                items[i] = nuevoItem;
                slotsUI[i].ActualizarSlot(nuevoItem);
                return true;
            }
        }

        MostrarInventarioLleno();
        return false; 
    }

    void MostrarInventarioLleno()
    {
        Debug.Log("Entrando a mostrar inventario lleno");
        StopAllCoroutines();
        StartCoroutine(OcultarPanelLuegoDeTiempo());
    }

    IEnumerator OcultarPanelLuegoDeTiempo()
    {
        canvasInvLleno.SetActive(true);
        yield return new WaitForSeconds(1.5f);
        canvasInvLleno.SetActive(false);
    }


    public void QuitarItem(int index)
    {
        items[index] = null;
        slotsUI[index].LimpiarSlot();
    }
}
