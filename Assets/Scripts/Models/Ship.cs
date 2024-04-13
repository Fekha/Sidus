using System;
using System.Collections.Generic;
using TMPro;
using Unity.Android.Gradle.Manifest;
using UnityEngine;

public class Ship : Structure
{
    internal int maxMovementRange;
    private int movementRange;
    internal List<PathNode> path;
    public void InitializeShip(int _x, int _y, int _movementRange, int _hp, Station _station)
    {
        stationId = _station.stationId;
        maxMovementRange = _movementRange;
        movementRange = _movementRange;
        resetMovementRange();
        _station.ships.Add(this);
        InitializeStructure(_x, _y, _hp);
    }
    internal void resetMovementRange()
    {
        movementRange = maxMovementRange;
        path = null;
    }

    internal void clearMovementRange()
    {
        movementRange = 0;
    }

    internal int getMovementRange()
    {
        return movementRange;
    }

    internal int getMaxMovementRange()
    {
        return maxMovementRange;
    }

    internal void subtractMovement(int i)
    {
        movementRange -= i;
    }
}