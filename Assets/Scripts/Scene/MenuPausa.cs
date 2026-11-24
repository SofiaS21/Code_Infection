using UnityEngine;
using UnityEngine.SceneManagement;

[DefaultExecutionOrder(1000)]   
public class MenuPausa : MonoBehaviour
{
    [SerializeField] GameObject panelPausa;
    [SerializeField] string escenaMenu = "Inicio";

    [Header("Se desactivan al pausar")]
    [SerializeField] MonoBehaviour[] scriptsADesactivar;
    [SerializeField] GameObject[] objetosAOcultar;

    bool pausado;

    // Estado guardado antes de pausar
    bool[] scriptsEstabanActivos;
    bool[] objetosEstabanActivos;
    CursorLockMode lockPrevio;
    bool cursorVisiblePrevio;
    float timeScalePrevio;

    void Start()
    {
        panelPausa.SetActive(false);
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (pausado) Continuar();
            else Pausar();
        }
    }

    public void Pausar()
    {
        pausado = true;

        // Guardar cómo estaba todo
        lockPrevio = Cursor.lockState;
        cursorVisiblePrevio = Cursor.visible;
        timeScalePrevio = Time.timeScale;

        scriptsEstabanActivos = new bool[scriptsADesactivar.Length];
        for (int i = 0; i < scriptsADesactivar.Length; i++)
        {
            if (scriptsADesactivar[i] == null) continue;
            scriptsEstabanActivos[i] = scriptsADesactivar[i].enabled;
            scriptsADesactivar[i].enabled = false;
        }

        objetosEstabanActivos = new bool[objetosAOcultar.Length];
        for (int i = 0; i < objetosAOcultar.Length; i++)
        {
            if (objetosAOcultar[i] == null) continue;
            objetosEstabanActivos[i] = objetosAOcultar[i].activeSelf;
            objetosAOcultar[i].SetActive(false);
        }

        panelPausa.SetActive(true);
        Time.timeScale = 0f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Continuar()
    {
        pausado = false;
        panelPausa.SetActive(false);

        // Restaurar EXACTAMENTE lo que había antes
        for (int i = 0; i < scriptsADesactivar.Length; i++)
            if (scriptsADesactivar[i] != null)
                scriptsADesactivar[i].enabled = scriptsEstabanActivos[i];

        for (int i = 0; i < objetosAOcultar.Length; i++)
            if (objetosAOcultar[i] != null)
                objetosAOcultar[i].SetActive(objetosEstabanActivos[i]);

        Time.timeScale = timeScalePrevio;
        Cursor.lockState = lockPrevio;
        Cursor.visible = cursorVisiblePrevio;
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenu);
    }
}