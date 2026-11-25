using System.Collections;
using TMPro;
using UnityEngine;

public class AvisoCuarto : MonoBehaviour
{
    public GameObject panel;
    public TMP_Text texto;
    public float tiempoVisible = 5f;
    public float duracionFade = 0.3f;

    private Coroutine actual;

    void Awake()
    {
        if (panel == gameObject || transform.IsChildOf(panel.transform))
            Debug.LogError("[AvisoCuarto] El campo Panel es este objeto o un PADRE de este objeto. Tiene que ser un HIJO.");

        panel.SetActive(false);
    }

    public void Mostrar(int numeroCuarto)
    {

        if (!gameObject.activeInHierarchy)
        {
            Debug.LogError("[AvisoCuarto] El objeto del script está apagado. Primer padre apagado: " + PrimerPadreApagado());
            return;
        }

        if (actual != null) StopCoroutine(actual);
        actual = StartCoroutine(Rutina(numeroCuarto));
    }

    IEnumerator Rutina(int numeroCuarto)
    {
        texto.text = "Cuarto " + numeroCuarto;

        yield return UITransiciones.FadeIn(panel, duracionFade, false);

        var cg = panel.GetComponent<CanvasGroup>();

        yield return new WaitForSeconds(tiempoVisible);
        yield return UITransiciones.FadeOut(panel, duracionFade);
    }

    string PrimerPadreApagado()
    {
        Transform t = transform;
        while (t != null)
        {
            if (!t.gameObject.activeSelf) return t.name;
            t = t.parent;
        }
        return "ninguno";
    }
}