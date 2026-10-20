using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class ItemTienda
{
    public string nombre;
    public Sprite icono;
    public int precio = 5;
}

public class TiendaUI : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelTienda;
    public GameObject panelCarrito;

    [Header("Catálogo (6 items, mismo orden que los botones)")]
    public ItemTienda[] items;
    public Button[] botonesComprar;     

    [Header("Carrito (3 slots)")]
    public TMP_Text[] textosCarrito;     
    public Button[] botonesBasura;       
    public int maxCarrito = 3;

    [Header("Navegación")]
    public Button botonVerCarrito;
    public Button botonVolverDeTienda;   
    public Button botonVolverDeCarrito;  
    public Button botonConfirmar;

    [Header("Bloquear al abrir (movimiento, cámara, etc.)")]
    public MonoBehaviour[] scriptsADesactivar;

    readonly List<ItemTienda> carrito = new List<ItemTienda>();
    public bool EstaAbierta { get; private set; }

    void Start()
    {
        for (int i = 0; i < botonesComprar.Length; i++)
        {
            int idx = i; 
            botonesComprar[i].onClick.AddListener(() => Agregar(idx));
        }
        for (int i = 0; i < botonesBasura.Length; i++)
        {
            int idx = i;
            botonesBasura[i].onClick.AddListener(() => Quitar(idx));
        }

        botonVerCarrito.onClick.AddListener(() => MostrarCarrito(true));
        botonVolverDeCarrito.onClick.AddListener(() => MostrarCarrito(false));
        botonVolverDeTienda.onClick.AddListener(Cerrar);
        botonConfirmar.onClick.AddListener(Confirmar);

        panelTienda.SetActive(false);
        panelCarrito.SetActive(false);
    }

    void Update()
    {
        if (EstaAbierta && Input.GetKeyDown(KeyCode.Escape)) Cerrar();
    }

    public void Abrir()
    {
        EstaAbierta = true;
        MostrarCarrito(false);
        foreach (var s in scriptsADesactivar) s.enabled = false;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Cerrar()
    {
        EstaAbierta = false;
        panelTienda.SetActive(false);
        panelCarrito.SetActive(false);
        foreach (var s in scriptsADesactivar) s.enabled = true;
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void MostrarCarrito(bool verCarrito)
    {
        panelTienda.SetActive(!verCarrito);
        panelCarrito.SetActive(verCarrito);
        Refrescar();
    }

    void Agregar(int idx)
    {
        if (carrito.Count >= maxCarrito) return; // ya tenés 3
        carrito.Add(items[idx]);
        Refrescar();
    }

    void Quitar(int slot)
    {
        if (slot >= carrito.Count) return;
        carrito.RemoveAt(slot);
        Refrescar();
    }

    void Confirmar()
    {
        if (carrito.Count == 0) return;

        for (int i = 0; i < carrito.Count; i++)
        {
            Debug.Log($"Slot {i} del inventario = {carrito[i].nombre}");
        }

        carrito.Clear();
        Refrescar();
        MostrarCarrito(false);
    }

    void Refrescar()
    {
        for (int i = 0; i < textosCarrito.Length; i++)
        {
            bool hay = i < carrito.Count;
            textosCarrito[i].text = hay ? carrito[i].nombre : "";
            botonesBasura[i].gameObject.SetActive(hay);
        }

        bool lleno = carrito.Count >= maxCarrito;
        foreach (var b in botonesComprar) b.interactable = !lleno;
        botonConfirmar.interactable = carrito.Count > 0;
    }
}