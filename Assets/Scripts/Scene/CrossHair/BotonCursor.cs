using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;


public class BotonCursor : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [HideInInspector] public Texture2D cursorManito;
    [HideInInspector] public Vector2 hotspot;

    Button boton;

    void Awake() { boton = GetComponent<Button>(); }

    public void OnPointerEnter(PointerEventData e)
    {
        if (cursorManito == null) return;
        if (boton != null && !boton.interactable) return; 
        Cursor.SetCursor(cursorManito, hotspot, CursorMode.Auto);
    }

    public void OnPointerExit(PointerEventData e)
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }

    void OnDisable()
    {
        Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
    }
}
