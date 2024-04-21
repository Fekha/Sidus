using System;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class Module : MonoBehaviour
{
    internal int id;
    internal Sprite icon;
    public Module(int _id)
    {
        id = _id;
        icon = Resources.Load<Sprite>($"Sprites/Modules/{_id}");
    }
}