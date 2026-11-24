using TMPro;
using UnityEngine;

[RequireComponent(typeof(TMP_InputField))]
public class SoloNumeros : MonoBehaviour
{
    void Awake()
    {
        var input = GetComponent<TMP_InputField>();
        input.characterLimit = 5; 
        input.onValidateInput += (texto, indice, c) => (c >= '0' && c <= '9') ? c : '\0';
    }
}