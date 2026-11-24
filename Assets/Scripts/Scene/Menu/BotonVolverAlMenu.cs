using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;

public class BotonVolverMenu : MonoBehaviour, IPointerClickHandler
{
    public string nombreEscena = "Inicio";

    public void OnPointerClick(PointerEventData eventData)
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene(nombreEscena);
    }
}