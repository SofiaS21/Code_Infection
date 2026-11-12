using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EspejoAnimations : MonoBehaviour
{
    public Animator origen;
    Animator mio;
    Vector3 posLocal;
    Quaternion rotLocal;

    void Awake()
    {
        mio = GetComponent<Animator>();
        mio.applyRootMotion = false;
        posLocal = transform.localPosition;
        rotLocal = transform.localRotation;
    }

    void LateUpdate()
    {
        if (origen == null || mio == null) return;

        var estado = origen.GetCurrentAnimatorStateInfo(0);
        if (mio.HasState(0, estado.fullPathHash))
            mio.Play(estado.fullPathHash, 0, estado.normalizedTime);

        // La copia no se puede despegar del paciente real
        transform.localPosition = posLocal;
        transform.localRotation = rotLocal;
    }
}
