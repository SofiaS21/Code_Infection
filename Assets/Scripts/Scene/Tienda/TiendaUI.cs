using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TiendaUI : MonoBehaviour
{
    [Header("Paneles")]
    public GameObject canvasTienda;
    public GameObject panelTienda;
    public GameObject panelCarrito;
    public GameObject inventarioUi;
    public GameObject sanidadUi;

    [Header("Catálogo (6 items, mismo orden que los botones BUY)")]
    public ItemData[] items;
    public Button[] botonesComprar;

    [Header("Carrito (3 filas, en orden)")]
    public GameObject[] filasCarrito;     // la fila entera (fondo gris + texto + tacho)
    public TMP_Text[] textosCarrito;      // el texto de cada fila
    public Button[] botonesBasura;        // el tacho de CADA fila (todos distintos)
    public int maxCarrito = 3;

    [Header("Canasta")]
    public CanastaTienda canastaPrefab;
    public Transform puntoCanasta;
    CanastaTienda canastaActual;

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

    [Header("Cursor")]
    public Texture2D cursorManito;
    public Vector2 hotspotManito = new Vector2(10, 0);

    [Header("Pop-up carrito lleno")]
    public GameObject popupLleno;
    public Button botonCerrarPopup;

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
            // Aviso si el mismo tacho está en más de un slot
            for (int j = 0; j < i; j++)
                if (botonesBasura[i] != null && botonesBasura[i] == botonesBasura[j])
                    Debug.LogError("Botones Basura: el slot " + i + " tiene el mismo botón que el slot " + j + ". Cada fila necesita su propio tacho.");

            int idx = i;
            botonesBasura[i].onClick.AddListener(() => Quitar(idx));

        }

        botonVerCarrito.onClick.AddListener(() => MostrarCarrito(true));
        botonVolverDeCarrito.onClick.AddListener(() => MostrarCarrito(false));
        botonVolverDeTienda.onClick.AddListener(Cerrar);
        botonConfirmar.onClick.AddListener(Confirmar);

        if (botonCerrarPopup != null) botonCerrarPopup.onClick.AddListener(CerrarPopup);

        // Efecto (manito + agrandar) en TODOS los botones con tag "Button", incluso los desactivados
        foreach (var b in FindObjectsOfType<Button>(true))
            AgregarCursor(b);

        panelTienda.SetActive(false);
        panelCarrito.SetActive(false);

        if (popupLleno != null) popupLleno.SetActive(false);
    }


    void AgregarCursor(Button b)
    {
        if (b == null || !b.CompareTag("Button")) return;

        var c = b.GetComponent<BotonCursor>();
        if (c == null) c = b.gameObject.AddComponent<BotonCursor>();
        c.cursorManito = cursorManito;
        c.hotspot = hotspotManito;
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
        EstaAbierta = true;
        if (canvasTienda != null) canvasTienda.SetActive(true);

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;

        foreach (var s in scriptsADesactivar)
            if (s != null) s.enabled = false;

        MostrarCarrito(false);
    }

    public void Cerrar()
    {
        EstaAbierta = false;
        if (canvasTienda != null) canvasTienda.SetActive(false);

        CerrarPopup();
        panelTienda.SetActive(false);
        panelCarrito.SetActive(false);

        foreach (var s in scriptsADesactivar)
            if (s != null) s.enabled = true;

        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    void MostrarCarrito(bool verCarrito)
    {
        CerrarPopup();
        panelTienda.SetActive(!verCarrito);
        panelCarrito.SetActive(verCarrito);
        Refrescar();
    }
    void MostrarPopup()
    {
        if (popupLleno != null) popupLleno.SetActive(true);
    }

    void CerrarPopup()
    {
        if (popupLleno != null) popupLleno.SetActive(false);
    }

    void Agregar(int idx)
    {
        if (carrito.Count >= maxCarrito)
        {
            MostrarPopup();
            return;
        }

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

        if (canastaActual != null) Destroy(canastaActual.gameObject);

        canastaActual = Instantiate(canastaPrefab, puntoCanasta.position, puntoCanasta.rotation);
        canastaActual.Configurar(carrito);

        carrito.Clear();
        Refrescar();
        MostrarCarrito(false);
    }

    void Refrescar()
    {
        for (int i = 0; i < filasCarrito.Length; i++)
        {
            bool hay = i < carrito.Count;

            // La fila entera aparece solo si hay un item en ese lugar
            if (filasCarrito[i] != null) filasCarrito[i].SetActive(hay);

            if (i < textosCarrito.Length && textosCarrito[i] != null)
                textosCarrito[i].text = hay ? carrito[i].nombre : "";
        }

        botonConfirmar.interactable = carrito.Count > 0;
    }
}