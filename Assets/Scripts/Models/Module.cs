using System;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class Module : MonoBehaviour
{
    internal int id;
    internal Sprite icon;
    internal String effectText;
    public Module(int _id)
    {
        id = _id;
        icon = Resources.Load<Sprite>($"Sprites/Modules/{_id}");
        switch (id)
        {
            case 0:
                effectText = "Ahoy";
                break;
            case 1:
                effectText = "Matey";
                break;
            case 2:
                effectText = "Test";
                break;
            case 3:
                effectText = "Wack";
                break;
            case 4:
                effectText = "Baby";
                break;
            case 5:
                effectText = "Shark";
                break;
            case 6:
                effectText = "Brooklyn";
                break;
            default:
                break;
        }
    }
}