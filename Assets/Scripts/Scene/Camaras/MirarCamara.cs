using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MirarCamara : MonoBehaviour
{
    public bool activo = false;
    public float pesoCuerpo = 0.4f;
    public float pesoCabeza = 1f;
    public float velocidad = 4f;

    Animator anim;
    Camera[] camaras;
    Transform objetivo;
    float peso;
    float proximaBusqueda;

    void Awake()
    {
        anim = GetComponent<Animator>();
        camaras = FindObjectsOfType<Camera>(true);
    }

    void Update()
    {
        if (Time.time < proximaBusqueda) return;
        proximaBusqueda = Time.time + 0.25f;

        // Busca la cámara de vigilancia activa más cercana
        Transform mejor = null;
        float menor = float.MaxValue;
        foreach (var c in camaras)
        {
            if (c == null || c.targetTexture == null) continue;
            if (!c.enabled || !c.gameObject.activeInHierarchy) continue;

            float d = (c.transform.position - transform.position).sqrMagnitude;
            if (d < menor) { menor = d; mejor = c.transform; }
        }
        objetivo = mejor;
    }

    void OnAnimatorIK(int layerIndex)
    {
        if (anim == null) return;

        float meta = (activo && objetivo != null) ? 1f : 0f;
        peso = Mathf.MoveTowards(peso, meta, Time.deltaTime * velocidad);

        anim.SetLookAtWeight(peso, pesoCuerpo, pesoCabeza, 0f, 0.5f);
        if (objetivo != null)
            anim.SetLookAtPosition(objetivo.position);
    }
}
