using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RuletaUI : MonoBehaviour
{
    [Header("UI")]
    public Button botonRojo;
    public Button botonVerde;
    public Button botonNegro;
    public GameObject[] marcas;            // 0 = rojo, 1 = verde, 2 = negro (un aro o tilde que se prende al elegir)
    public TMP_InputField inputApuesta;
    public Button botonGirar;
    public TMP_Text textoDineroCasino;     // opcional: tu plata dentro del casino
    public TMP_Text textoMensaje;          // el cartel gris de arriba
    public RectTransform rueda;            // la imagen de la ruleta

    [Header("Reglas")]
    public int apuestaMinima = 1;
    public int apuestaMaxima = 100;
    public int pagoColor = 2;              // rojo/negro: te devuelven el doble
    public int pagoVerde = 36;             // verde: te devuelven 36 veces (paga 35 a 1, como la real)

    [Header("Animación")]
    public float duracion = 4f;
    public int vueltas = 4;
    public float offsetGrados = 0f;        // para corregir si el 0 no queda justo arriba

    // Orden real de la ruleta europea, empezando por el 0 y en sentido horario
    static readonly int[] ordenRueda =
    {
        0, 32, 15, 19, 4, 21, 2, 25, 17, 34, 6, 27, 13, 36, 11, 30, 8, 23, 10, 5, 24, 16, 33, 1, 20, 14, 31, 9, 22, 18, 29, 7, 28, 12, 35, 3, 26
    };

    static readonly int[] numerosRojos = { 1, 3, 5, 7, 9, 12, 14, 16, 18, 19, 21, 23, 25, 27, 30, 32, 34, 36 };

    const int ROJO = 0, VERDE = 1, NEGRO = 2;
    static readonly string[] nombres = { "rojo", "verde", "negro" };

    int colorElegido = -1;
    bool girando;

    void Start()
    {
        botonRojo.onClick.AddListener(() => Elegir(ROJO));
        botonVerde.onClick.AddListener(() => Elegir(VERDE));
        botonNegro.onClick.AddListener(() => Elegir(NEGRO));
        botonGirar.onClick.AddListener(Girar);

        foreach (var m in marcas) if (m != null) m.SetActive(false);
        textoMensaje.text = "Elige un color, elige una cantidad, y gira la ruleta!";
    }

    void Elegir(int color)
    {
        if (girando) return;
        colorElegido = color;
        for (int i = 0; i < marcas.Length; i++)
            if (marcas[i] != null) marcas[i].SetActive(i == color);
    }

    void Girar()
    {
        if (girando) return;

        if (colorElegido < 0)
        {
            textoMensaje.text = "Debes elegir un color para poder comenzar.";
            return;
        }

        int apuesta;
        if (!int.TryParse(inputApuesta.text, out apuesta) || apuesta < apuestaMinima)
        {
            textoMensaje.text = "Debes aposta por lo menos $" + apuestaMinima + " para poder jugar.";
            return;
        }
        if (apuesta > apuestaMaxima)
        {
            textoMensaje.text = "La apuesta maxima es $" + apuestaMaxima + ". Baja el numero para jugar.";
            return;
        }
        if (!DineroManager.instancia.Apostar(apuesta))
        {
            textoMensaje.text = "No te alcanza la plata. Tenes $" + DineroManager.instancia.Dinero + ".";
            return;
        }

        StartCoroutine(GirarRuleta(apuesta, colorElegido));
    }

    void LateUpdate()
    {
        if (textoDineroCasino != null && DineroManager.instancia != null)
            textoDineroCasino.text = "$" + DineroManager.instancia.Dinero;
    }

    IEnumerator GirarRuleta(int apuesta, int colorApostado)
    {
        girando = true;
        SetInteractable(false);
        textoMensaje.text = "Girando...";

        int numero = Random.Range(0, 37);
        int indice = System.Array.IndexOf(ordenRueda, numero);

        // Cada casillero ocupa 360/37 grados; llevamos el elegido arriba
        float objetivo = indice * (360f / 37f) + offsetGrados;
        float actual = Mathf.Repeat(rueda.localEulerAngles.z, 360f);
        float delta = Mathf.Repeat(objetivo - actual, 360f) + vueltas * 360f;

        float t = 0f;
        while (t < duracion)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duracion);
            k = 1f - Mathf.Pow(1f - k, 3f);   // arranca rápido y frena de a poco
            rueda.localEulerAngles = new Vector3(0f, 0f, actual + delta * k);
            yield return null;
        }
        rueda.localEulerAngles = new Vector3(0f, 0f, objetivo);

        int colorSalido = ColorDe(numero);
        string queSalio = "Salio el " + numero + " (" + nombres[colorSalido] + "). ";

        if (colorSalido == colorApostado)
        {
            int pago = apuesta * (colorApostado == VERDE ? pagoVerde : pagoColor);
            DineroManager.instancia.CobrarApuesta(pago);
            textoMensaje.text = queSalio + "Ganaste $" + (pago - apuesta) + "! Quieres intentar de nuevo?";
        }
        else
        {
            textoMensaje.text = queSalio + "Perdiste $" + apuesta + ". Quieres intentar de nuevo?";
        }

        girando = false;
        SetInteractable(true);
    }

    int ColorDe(int n)
    {
        if (n == 0) return VERDE;
        return System.Array.IndexOf(numerosRojos, n) >= 0 ? ROJO : NEGRO;
    }

    void SetInteractable(bool valor)
    {
        botonRojo.interactable = valor;
        botonVerde.interactable = valor;
        botonNegro.interactable = valor;
        botonGirar.interactable = valor;
        inputApuesta.interactable = valor;
    }
}