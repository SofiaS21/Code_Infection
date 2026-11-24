using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.PostProcessing;
using TMPro;

public class SanidadManager : MonoBehaviour
{
    public static SanidadManager Instance;

    [Header("UI")]
    public TextMeshProUGUI textoSanidad;
    public Image iconoCerebro;
    public Sprite[] sprites;
    public GameObject panelGameOver;

    [Header("Game Over")]
    public GameObject padreGameOver;
    public GameObject crosshair;
    public MonoBehaviour controladorJugador;

    [Header("Drenaje por tiempo (puntos por minuto)")]
    public int drenajeMin = 10;
    public int drenajeMax = 15;

    [Header("PostProcessing (el volumen de SANIDAD, no el del ataque)")]
    public PostProcessVolume volume;               // arrastrá acá tu Post-process Volume de sanidad
    public float sanidadInicioEfectos = 90f;       // desde acá empiezan a subir
    public float vignetteMax = 0.55f;
    public float ambientOcclusionMax = 2f;
    public float grainMax = 0.8f;
    public float chromaticMax = 1f;
    public float lensMax = 30f;

    [Header("Curva de los efectos")]
    [Tooltip("Más alto = los efectos fuertes aparecen más tarde (3 = el nivel fuerte llega cerca del 20%)")]
    public float curvaEfectos = 3f;
    [Tooltip("Intensidad que se nota cuando la sanidad está en 'Sanidad Inicio Efectos' (0 = nada)")]
    [Range(0f, 0.2f)] public float intensidadMinima = 0.03f;

    [Header("Pulso de susto (al matar, alucinaciones)")]
    [Tooltip("Segundos que tarda el pulso en llegar a su punto máximo")]
    public float subidaPulso = 1f;
    [Tooltip("Segundos que tarda en bajar desde el máximo. Subida + bajada = duración total (1 + 2 = 3 segundos)")]
    public float bajadaPulso = 2f;

    public Button botonVolverMenu;

    float sanidad = 90f;
    float drenaje;
    float tiempoPorMinuto;
    bool muerto;

    // Efectos post-processing
    Vignette vignette;
    AmbientOcclusion occlusion;
    Grain grain;
    ChromaticAberration chromatic;
    LensDistortion lens;
    float vignetteBase;
    float occlusionBase;
    float grainBase;
    float intensidadEfectos;   // 0 a 1 (suavizada)
    float pulso;               // 0 a 1, se suma a los efectos (sube y baja suave)
    float pulsoPico;
    float pulsoTiempo = 999f;
    float pulsoSubida;
    float pulsoBajada;
    float tiempoLog;           // solo para el log de diagnóstico de los efectos

    // Lo leen AlucinacionManager y ParpadeoLuces
    public float Sanidad { get { return sanidad; } }

    void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        Time.timeScale = 1f;
        if (panelGameOver != null) panelGameOver.SetActive(false);

        BuscarEfectos();
        ElegirDrenaje();
        ActualizarUI();

        if (botonVolverMenu != null)
        {
            botonVolverMenu.onClick.RemoveAllListeners();
            botonVolverMenu.onClick.AddListener(VolverAlMenu);
        }
    }

    void Update()
    {
        ActualizarEfectos();   // va antes del return para que siga funcionando al morir

        if (muerto) return;

        tiempoPorMinuto += Time.deltaTime;
        if (tiempoPorMinuto >= 60f)
        {
            tiempoPorMinuto = 0f;
            ElegirDrenaje();
        }

        CambiarSanidad(-drenaje * Time.deltaTime);
    }

    void ElegirDrenaje()
    {
        drenaje = Random.Range(drenajeMin, drenajeMax + 1) / 60f;   // puntos por minuto
    }

    void BuscarEfectos()
    {
        if (volume == null)
        {
            return;
        }

        volume.profile.TryGetSettings(out vignette);
        volume.profile.TryGetSettings(out occlusion);
        volume.profile.TryGetSettings(out grain);
        volume.profile.TryGetSettings(out chromatic);
        volume.profile.TryGetSettings(out lens);

        if (vignette != null) vignetteBase = vignette.intensity.value;
        if (occlusion != null) occlusionBase = occlusion.intensity.value;
        if (grain != null) grainBase = grain.intensity.value;
    }


    public void PulsoSusto(float fuerza, float subida = -1f, float bajada = -1f)
    {
        fuerza = Mathf.Clamp01(fuerza);
        if (fuerza <= pulso) return;   // ya hay un pulso más fuerte en curso

        pulsoPico = fuerza;
        pulsoSubida = Mathf.Max(0.01f, subida >= 0f ? subida : subidaPulso);
        pulsoBajada = Mathf.Max(0.01f, bajada >= 0f ? bajada : bajadaPulso);
        pulsoTiempo = 0f;
    }

    void ActualizarPulso()
    {
        pulsoTiempo += Time.unscaledDeltaTime;

        if (pulsoTiempo < pulsoSubida)
            pulso = pulsoPico * Mathf.SmoothStep(0f, 1f, pulsoTiempo / pulsoSubida);
        else if (pulsoTiempo < pulsoSubida + pulsoBajada)
            pulso = pulsoPico * (1f - Mathf.SmoothStep(0f, 1f, (pulsoTiempo - pulsoSubida) / pulsoBajada));
        else
            pulso = 0f;
    }

    void ActualizarEfectos()
    {

        float t = Mathf.InverseLerp(sanidadInicioEfectos, 0f, sanidad);
        float objetivo = Mathf.Lerp(intensidadMinima, 1f, Mathf.Pow(t, curvaEfectos));

        intensidadEfectos = Mathf.Lerp(intensidadEfectos, objetivo, Time.unscaledDeltaTime * 3f);

        ActualizarPulso();
        float f = Mathf.Clamp01(intensidadEfectos + pulso);

        if (vignette != null)
        {
            vignette.enabled.Override(true);
            vignette.intensity.Override(Mathf.Lerp(vignetteBase, vignetteMax, f));
        }
        if (occlusion != null)
        {
            occlusion.enabled.Override(true);
            occlusion.intensity.Override(Mathf.Lerp(occlusionBase, ambientOcclusionMax, f));
        }
        if (grain != null)
        {
            grain.enabled.Override(true);
            grain.intensity.Override(Mathf.Lerp(grainBase, grainMax, f));
        }
        if (chromatic != null)
        {
            chromatic.enabled.Override(true);
            chromatic.intensity.Override(chromaticMax * f);
        }
        if (lens != null)
        {
            lens.enabled.Override(true);
            lens.intensity.Override(lensMax * f);
        }

        tiempoLog += Time.unscaledDeltaTime;
        if (tiempoLog >= 2f)
        {
            tiempoLog = 0f;
        }
    }

    void ActualizarUI()
    {
        int valor = Mathf.CeilToInt(sanidad);
        textoSanidad.text = valor + "%";

        int i = 4;
        if (valor >= 81) i = 0;
        else if (valor >= 61) i = 1;
        else if (valor >= 41) i = 2;
        else if (valor >= 21) i = 3;

        iconoCerebro.sprite = sprites[i];
    }

    public void PerderPorRechazos()
    {
        GameOver();
    }

    void GameOver()
    {
        if (muerto) return;
        muerto = true;

        if (padreGameOver != null) padreGameOver.SetActive(true);
        panelGameOver.SetActive(true);

        if (crosshair != null) crosshair.SetActive(false);
        if (controladorJugador != null) controladorJugador.enabled = false;

        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void CambiarSanidad(float cantidad)
    {
        if (muerto) return;

        sanidad = Mathf.Clamp(sanidad + cantidad, 0f, 100f);
        ActualizarUI();

        if (sanidad <= 0f) GameOver();
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Inicio");
    }

    public void PacienteCurado() { CambiarSanidad(15); }            // curar a un normal
    public void PacienteMuertoPorError() { CambiarSanidad(-10); }  // normal muerto por medicación incorrecta
    public void AnomaliaMuerta() { CambiarSanidad(5); }            // anomalía muerta por medicación incorrecta
    public void AtaqueAnomalia() { CambiarSanidad(-15); }          // una anomalía te ataca
    public void RechazarAnomalia() { CambiarSanidad(10); }         // rechazar una anomalía en el counter
    public void AceptarPaciente() { CambiarSanidad(5); }           // aceptar a cualquiera en el counter
    public void BonusFinDeDia() { CambiarSanidad(20); }
}