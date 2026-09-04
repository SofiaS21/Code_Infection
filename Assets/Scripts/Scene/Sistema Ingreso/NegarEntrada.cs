using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NegarEntrada : MonoBehaviour
{

    public GameObject canvasE;
    public GameObject cam;
    private CanvasGroup canvasGroup;

    // Start is called before the first frame update
    void Start()
    {
        canvasGroup = canvasE.GetComponent<CanvasGroup>();

        canvasE.SetActive(false);
        canvasGroup.alpha = 0;
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void AcercarBoton()
    {
        canvasE.SetActive(true);
    }

    
    void LateUpdate()
    {
        transform.LookAt(transform.position + cam.transform.rotation * Vector3.forward,     cam.transform.rotation * Vector3.up);
    }
}
