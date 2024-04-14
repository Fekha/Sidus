using System;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class Ship : Structure
{
    
    internal List<PathNode> path;
    public void InitializeShip(int _x, int _y, Station _station, string name, int _hp, int _range, int _shield, int _electricAttack, int _thermalAttack, int _voidAttack, int _level)
    {
        stationId = _station.stationId;
        resetMovementRange();
        _station.ships.Add(this);
        InitializeStructure(_x, _y, name, _hp, _range,_shield,_electricAttack,_thermalAttack,_voidAttack, _level);
    }
    internal void resetMovementRange()
    {
        range = maxRange;
        path = null;
    }

    internal void clearMovementRange()
    {
        range = 0;
    }

    internal int getMovementRange()
    {
        return range;
    }

    internal int getMaxMovementRange()
    {
        return maxRange;
    }

    internal void subtractMovement(int i)
    {
        range -= i;
    }
}