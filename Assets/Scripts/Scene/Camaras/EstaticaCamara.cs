using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Genera estatica de television (ruido) sobre la imagen de las camaras,
/// al estilo FNAF: se ve mas o menos clara con el tiempo, con algun
/// "glitch" ocasional, pero nunca tapa del todo la imagen de la camara.
///
/// COMO USARLO:
/// 1) Dentro de tu panel de camaras (el mismo padre de "ImageCamara"),
///    crea un nuevo objeto: click derecho > UI > Raw Image. Llamalo
///    "EstaticaCamaras".
/// 2) Ponelo como ULTIMO hijo dentro de ese padre (para que se dibuje
///    encima de la imagen de la camara). Estira su RectTransform para
///    que cubra el mismo rectangulo que "ImageCamara".
/// 3) Agregale este script (Add Component > Estatica Camaras).
/// 4) Arrastra ese mismo Raw Image al campo "Imagen Estatica" de abajo.
/// 5) Jugá con "alphaMinimo" y "alphaMaximo" para decidir que tan visible
///    queres que sea la estatica (por defecto es sutil).
/// </summary>
[RequireComponent(typeof(RawImage))]
public class EstaticaCamara : MonoBehaviour
{
    [Header("Referencias")]
    [Tooltip("El Raw Image que va a mostrar la estatica. Si lo dejas vacio, usa el de este mismo objeto.")]
    public RawImage imagenEstatica;

    [Header("Que tan visible es la estatica")]
    [Tooltip("Transparencia minima de la estatica (0 = invisible).")]
    [Range(0f, 1f)] public float alphaMinimo = 0.04f;
    [Tooltip("Transparencia maxima de la estatica en su momento mas fuerte. Mantenelo bajo para no tapar la camara.")]
    [Range(0f, 1f)] public float alphaMaximo = 0.28f;
    [Tooltip("Que tan rapido sube y baja la transparencia (mas alto = parpadeo mas rapido).")]
    public float velocidadFlicker = 2.5f;

    [Header("Glitches ocasionales")]
    [Tooltip("Cada cuanto tiempo (aprox, en segundos) puede aparecer un pico de estatica mas fuerte.")]
    public float tiempoEntreGlitches = 4f;
    [Tooltip("Duracion de cada pico de estatica.")]
    public float duracionGlitch = 0.25f;
    [Tooltip("Transparencia que alcanza la estatica durante un glitch.")]
    [Range(0f, 1f)] public float alphaGlitch = 0.55f;

    [Header("Textura de ruido (generada sola, no tocar)")]
    [Tooltip("Resolucion de la textura de ruido. Bajo = mas 'retro' y mas barato en rendimiento.")]
    public int resolucionRuido = 64;
    [Tooltip("Cada cuantos frames se regenera el ruido (mas alto = mas barato, menos fluido).")]
    public int frameCadaCuantoActualiza = 2;

    private Texture2D textura;
    private Color32[] pixeles;
    private int contadorFrames;
    private float semillaFlicker;
    private float tiempoProximoGlitch;
    private float glitchRestante;

    void Awake()
    {
        if (imagenEstatica == null)
            imagenEstatica = GetComponent<RawImage>();

        CrearTexturaDeRuido();
        semillaFlicker = Random.Range(0f, 1000f);
        ProgramarProximoGlitch();
    }

    void OnEnable()
    {
        // Arrancamos siempre con un valor bajo al abrir las camaras.
        ProgramarProximoGlitch();
    }

    void Update()
    {
        ActualizarRuidoVisual();
        ActualizarTransparencia();
    }

    void CrearTexturaDeRuido()
    {
        textura = new Texture2D(resolucionRuido, resolucionRuido, TextureFormat.RGBA32, false);
        textura.filterMode = FilterMode.Point; // pixelado, look "retro TV"
        textura.wrapMode = TextureWrapMode.Repeat;
        pixeles = new Color32[resolucionRuido * resolucionRuido];
        RellenarRuido();
        imagenEstatica.texture = textura;
    }

    void RellenarRuido()
    {
        for (int i = 0; i < pixeles.Length; i++)
        {
            byte gris = (byte)Random.Range(0, 256);
            pixeles[i] = new Color32(gris, gris, gris, 255);
        }
        textura.SetPixels32(pixeles);
        textura.Apply(false);
    }

    void ActualizarRuidoVisual()
    {
        contadorFrames++;
        if (contadorFrames >= Mathf.Max(1, frameCadaCuantoActualiza))
        {
            contadorFrames = 0;
            RellenarRuido();
        }
    }

    void ActualizarTransparencia()
    {
        float alphaObjetivo;

        if (glitchRestante > 0f)
        {
            // Estamos en medio de un glitch: estatica bien fuerte por un instante.
            glitchRestante -= Time.deltaTime;
            alphaObjetivo = alphaGlitch;
        }
        else
        {
            // Flicker suave y continuo entre minimo y maximo (Perlin = sin saltos bruscos).
            float ruido = Mathf.PerlinNoise(semillaFlicker, Time.time * velocidadFlicker);
            alphaObjetivo = Mathf.Lerp(alphaMinimo, alphaMaximo, ruido);

            tiempoProximoGlitch -= Time.deltaTime;
            if (tiempoProximoGlitch <= 0f)
            {
                glitchRestante = duracionGlitch;
                ProgramarProximoGlitch();
            }
        }

        Color c = imagenEstatica.color;
        c.a = Mathf.Lerp(c.a, alphaObjetivo, Time.deltaTime * 10f);
        imagenEstatica.color = c;
    }

    void ProgramarProximoGlitch()
    {
        tiempoProximoGlitch = tiempoEntreGlitches * Random.Range(0.5f, 1.5f);
    }

    void OnDestroy()
    {
        if (textura != null)
            Destroy(textura);
    }
}