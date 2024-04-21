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
                effectText = "+3 Electric Attack";
                break;
            case 1:
                effectText = "+3 Thermal Attack";
                break;
            case 2:
                effectText = "+3 Void Attack";
                break;
            case 3:
                effectText = "+1 Movement";
                break;
            case 4:
                effectText = "+2 Max HP";
                break;
            case 5:
                effectText = "On Attach: Change Shield to Electric";
                break;
            case 6:
                effectText = "On Attach: Change Shield to Thermal";
                break;
            default:
                break;
        }
    }
}