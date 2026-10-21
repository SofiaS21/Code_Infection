using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TiendaUI : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject panelTienda;
    public GameObject panelCarrito;
    public GameObject inventarioUi;
    public GameObject sanidadUi;

    [Header("Catálogo (6 items, mismo orden que los botones BUY)")]
    public ItemData[] items;
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

    [Header("Cierre por distancia")]
    public Transform jugador;           
    public Transform computadoraModelo;       
    public float distanciaCierre = 4f;


    readonly List<ItemData> carrito = new List<ItemData>();
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
        if (!EstaAbierta) return;

        if (Input.GetKeyDown(KeyCode.Escape)) { Cerrar(); return; }

        if (jugador != null && computadoraModelo != null &&
            Vector3.Distance(jugador.position, computadoraModelo.position) > distanciaCierre)
        {
            Cerrar();
        }
    }

    void LateUpdate()
    {
        if (!EstaAbierta) return;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Abrir()
    {
        Debug.Log("Tienda abierta");
        EstaAbierta = true;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var s in scriptsADesactivar)
            if (s != null) s.enabled = false;

        MostrarCarrito(false);
    }

    public void Cerrar()
    {
        EstaAbierta = false;
        panelTienda.SetActive(false);
        panelCarrito.SetActive(false);

        foreach (var s in scriptsADesactivar)
            if (s != null) s.enabled = true;

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

        Inventario.instancia.ReemplazarItems(carrito);

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