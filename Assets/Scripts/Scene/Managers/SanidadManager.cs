using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

public class SanidadManager : MonoBehaviour
{
    public static SanidadManager Instance;

    [Header("UI")]
    public TextMeshProUGUI textoSanidad;
    public Image iconoCerebro;
    public Sprite[] sprites;
    public GameObject panelGameOver;

    float sanidad = 90f;
    float drenaje;
    float tiempoPorMinuto;
    bool muerto;

    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        Time.timeScale = 1f;
        if (panelGameOver != null) panelGameOver.SetActive(false);
        ElegirDrenaje();
        ActualizarUI();
    }

    // Update is called once per frame
    void Update()
    {
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
        drenaje = Random.Range(3, 5) / 60f; //3 o 4 por minuto
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

    void GameOver()
    {
        muerto = true;
        Time.timeScale = 0f;
        panelGameOver.SetActive(true);
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
        SceneManager.LoadScene("Inicio");
    }

    public void RecibirDanoEnemigo() { CambiarSanidad(-Random.Range(5, 9)); }
    public void PacienteMuertoPorError() { CambiarSanidad(-Random.Range(10, 16)); }
    public void PacienteCurado() { CambiarSanidad(5); }

}
