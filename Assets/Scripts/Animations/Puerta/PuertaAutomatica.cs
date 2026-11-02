using System.Collections;
using UnityEngine;

public class PuertaAutomatica : MonoBehaviour
{
    public Animator animPuerta;
    public float tiempoAntesDeCerrar = 2.5f;

    private const string parametroAbierta = "PuertaAbierta";
    private int pacientesCerca = 0;
    private Coroutine cierre;

    void OnTriggerEnter(Collider other)
    {
        if (other.GetComponentInParent<Pacientes>() == null) return;

        pacientesCerca++;

        // Si estaba por cerrarse, lo cancelo
        if (cierre != null)
        {
            StopCoroutine(cierre);
            cierre = null;
        }

        animPuerta.SetBool(parametroAbierta, true);
    }

    void OnTriggerExit(Collider other)
    {
        if (other.GetComponentInParent<Pacientes>() == null) return;

        pacientesCerca = Mathf.Max(0, pacientesCerca - 1);

        if (pacientesCerca == 0)
            cierre = StartCoroutine(CerrarConDelay());
    }

    IEnumerator CerrarConDelay()
    {
        yield return new WaitForSeconds(tiempoAntesDeCerrar);
        animPuerta.SetBool(parametroAbierta, false);
        cierre = null;
    }
}