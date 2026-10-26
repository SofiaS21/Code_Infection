using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BotonCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public Texture2D cursorManito;
    [HideInInspector] public Vector2 hotspot;

    public float escalaHover = 1.05f;
    public float velocidadHover = 12f;

    Button boton;
    Vector3 escalaOriginal;
    Vector3 escalaObjetivo;

    void Awake()
    {
        boton = GetComponent<Button>();
        escalaOriginal = transform.localScale;
        escalaObjetivo = escalaOriginal;
    }

    void Update()
    {
        transform.localScale = Vector3.Lerp(transform.localScale, escalaObjetivo, velocidadHover * Time.unscaledDeltaTime);
    }

    public void OnPointerEnter(PointerEventData e)
    {
        if (cursorManito == null) return;

        escalaObjetivo = escalaOriginal * escalaHover;

        if (boton != null && !boton.interactable) { Debug.Log("Botón no interactable"); return; }
        Cursor.SetCursor(cursorManito, hotspot, CursorMode.ForceSoftware);
    }

    public void OnPointerExit(PointerEventData e)
    {
        escalaObjetivo = escalaOriginal;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnDisable()
    {
        transform.localScale = escalaOriginal;
        escalaObjetivo = escalaOriginal;
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
