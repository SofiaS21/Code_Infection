using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuPausa : MonoBehaviour
{
    [SerializeField] GameObject panelPausa;
    [SerializeField] string escenaMenu = "Inicio";

    [Header("Se desactivan al pausar")]
    [SerializeField] MonoBehaviour[] scriptsADesactivar; // FPS controller, interacción, etc.

    bool pausado;

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
        panelPausa.SetActive(true);
        Time.timeScale = 0f;

        foreach (var s in scriptsADesactivar) if (s != null) s.enabled = false;

        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
    }

    public void Continuar()
    {
        pausado = false;
        panelPausa.SetActive(false);
        Time.timeScale = 1f;

        foreach (var s in scriptsADesactivar) if (s != null) s.enabled = true;

        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
    }

    public void VolverAlMenu()
    {
        Time.timeScale = 1f;
        SceneManager.LoadScene(escenaMenu);
    }
}