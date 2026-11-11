using System.Collections;
using System.Collections.Generic;
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

    [Header("PostProcessing")]
    public PostProcessVolume volume;               // arrastrá acá tu Post-process Volume
    public float sanidadInicioEfectos = 1f;       // desde acá empiezan a subir
    public float vignetteMax = 0.55f;
    public float ambientOcclusionMax = 2f;
    public float grainMax = 0.8f;
    public float chromaticMax = 1f;
    public float lensMax = 30f;

    public Button botonVolverMenu;
    public void RechazarAnomalia() { CambiarSanidad(10); }
    public void AceptarPaciente() { CambiarSanidad(5); }

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
            Debug.LogWarning("SanidadManager: falta arrastrar el Post-process Volume.");
            return;
        }

        volume.profile.TryGetSettings(out vignette);
        volume.profile.TryGetSettings(out occlusion);
        volume.profile.TryGetSettings(out grain);
        volume.profile.TryGetSettings(out chromatic);
        volume.profile.TryGetSettings(out lens);

        if (vignette == null) Debug.LogWarning("SanidadManager: el profile no tiene Vignette.");
        if (occlusion == null) Debug.LogWarning("SanidadManager: el profile no tiene Ambient Occlusion.");
        if (grain == null) Debug.LogWarning("SanidadManager: el profile no tiene Grain.");
        if (chromatic == null) Debug.LogWarning("SanidadManager: el profile no tiene Chromatic Aberration.");
        if (lens == null) Debug.LogWarning("SanidadManager: el profile no tiene Lens Distortion.");

        if (vignette != null) vignetteBase = vignette.intensity.value;
        if (occlusion != null) occlusionBase = occlusion.intensity.value;
        if (grain != null) grainBase = grain.intensity.value;
    }

    void ActualizarEfectos()
    {
        // 0 cuando sanidad >= sanidadInicioEfectos, 1 cuando sanidad = 0
        float objetivo = Mathf.InverseLerp(sanidadInicioEfectos, 0f, sanidad);

        // suavizado para que no salte de golpe
        intensidadEfectos = Mathf.Lerp(intensidadEfectos, objetivo, Time.unscaledDeltaTime * 3f);

        if (vignette != null)
        {
            vignette.enabled.Override(true);
            vignette.intensity.Override(Mathf.Lerp(vignetteBase, vignetteMax, intensidadEfectos));
        }
        if (occlusion != null)
        {
            occlusion.enabled.Override(true);
            occlusion.intensity.Override(Mathf.Lerp(occlusionBase, ambientOcclusionMax, intensidadEfectos));
        }
        if (grain != null)
        {
            grain.enabled.Override(true);
            grain.intensity.Override(Mathf.Lerp(grainBase, grainMax, intensidadEfectos));
        }
        if (chromatic != null)
        {
            chromatic.enabled.Override(true);
            chromatic.intensity.Override(chromaticMax * intensidadEfectos);
        }
        if (lens != null)
        {
            lens.enabled.Override(true);
            lens.intensity.Override(lensMax * intensidadEfectos);
        }
    }

    // ---------- UI ----------
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

        // Activa el padre primero, si no el panel no se ve
        if (padreGameOver != null) padreGameOver.SetActive(true);
        panelGameOver.SetActive(true);

        // Apaga el crosshair y congela al jugador
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
        Debug.Log("BOTON APRETADO. Escena a cargar: Inicio");
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Inicio");
    }

    public void PacienteMuertoPorError() { CambiarSanidad(-10); }
    public void PacienteCurado() { CambiarSanidad(10); }
    public void AnomaliaMuerta() { CambiarSanidad(5); }
    public void AtaqueAnomalia() { CambiarSanidad(-15); }
}
