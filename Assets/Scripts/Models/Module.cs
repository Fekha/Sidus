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
                effectText = "+4 Electric Attack \n On Attach: Change Shield to Thermal";
                break;
            case 1:
                effectText = "+4 Thermal Attack \n On Attach: Change Shield to Void";
                break;
            case 2:
                effectText = "+4 Void Attack \n On Attach: Change Shield to Electric";
                break;
            case 3:
                effectText = "+1 Movement \n On Attach: Change Shield to Thermal";
                break;
            case 4:
                effectText = "+3 Max HP \n On Attach: Change Shield to Electric";
                break;
            case 5:
                effectText = "+2 Void Attack \n +2 Thermal Attack \n +1 Electric Attack \n On Attach: Change Shield to Electric";
                break;
            case 6:
                effectText = "+2 Electric Attack \n +2 Void Attack \n +1 Thermal Attack \n On Attach: Change Shield to Thermal";
                break;
            case 7:
                effectText = "+2 Thermal Attack \n +2 Electric Attack \n +1 Void Attack \n On Attach: Change Shield to Void";
                break;
            case 8:
                effectText = "+3 Void Attack \n +1 Electric Attack \n +1 Thermal Attack \n On Attach: Change Shield to Thermal";
                break;
            case 9:
                effectText = "+3 Electric Attack \n +1 Thermal Attack \n +1 Void Attack \n On Attach: Change Shield to Void";
                break;
            case 10:
                effectText = "+3 Thermal Attack \n +1 Electric Attack \n +1 Void Attack \n On Attach: Change Shield to Electric";
                break;
            default:
                break;
        }
    }
}