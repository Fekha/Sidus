using System;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class Ship : Structure
{
    public void InitializeShip(int _x, int _y, Station _station, string name, int _hp, int _range, int _shield, int _electricAttack, int _thermalAttack, int _voidAttack, int _level)
    {
        stationId = _station.stationId;
        _station.ships.Add(this);
        InitializeStructure(_x, _y, name, _hp, _range,_shield,_electricAttack,_thermalAttack,_voidAttack, _level);
    }
}