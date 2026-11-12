using UnityEngine;

public class VisionCamara : MonoBehaviour
{
    public GameObject modeloNormal;      // hijo con el modelo normal
    public GameObject modeloInfectado;   // hijo con el modelo infectado

    public void Activar(bool esAnomaliaDeCamara)
    {
        if (!esAnomaliaDeCamara)
        {
            // Paciente normal: solo el modelo normal, visible para todos
            if (modeloInfectado != null) modeloInfectado.SetActive(false);
            return;
        }

        if (modeloNormal != null)
            PonerCapa(modeloNormal, LayerMask.NameToLayer("ModeloNormal"));
        if (modeloInfectado != null)
        {
            modeloInfectado.SetActive(true);
            PonerCapa(modeloInfectado, LayerMask.NameToLayer("ModeloInfectado"));
        }
    }

    void PonerCapa(GameObject obj, int capa)
    {
        obj.layer = capa;
        foreach (Transform hijo in obj.transform)
            PonerCapa(hijo.gameObject, capa);
    }
}