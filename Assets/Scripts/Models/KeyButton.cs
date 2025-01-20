using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class KeyButton : MonoBehaviour
{
    private string letter;
    private Button button;

    public void Start()
    {
        letter = gameObject.name;
        button = gameObject.GetComponent<Button>();
        button.onClick.AddListener(() => onClick());
    }
    public void onClick()
    {
        GoogleSignInManager.i.Type(letter);
    }
}
