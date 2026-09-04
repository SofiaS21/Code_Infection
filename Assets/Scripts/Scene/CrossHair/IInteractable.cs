using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IInteractable
{
    void OnFocus();
    void OnUnfocus();
    void Interact();
}
