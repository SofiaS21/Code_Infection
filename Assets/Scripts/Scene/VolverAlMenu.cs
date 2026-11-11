using UnityEngine;
using UnityEngine.SceneManagement;

public class VolverAlMenu : MonoBehaviour
{
    public void IrAlMenu()
    {
        Time.timeScale = 1f;
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        SceneManager.LoadScene("Inicio");
    }
}