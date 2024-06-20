using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToggleActiveState : MonoBehaviour
{
    void Start()
    {
        // Invoke the ToggleActiveState method after 2 seconds
        Invoke("ToggleActiveStateMethod", 2.0f);
    }

    void ToggleActiveStateMethod()
    {
        // Toggle the active state of the GameObject
        gameObject.SetActive(false);
    }
}
