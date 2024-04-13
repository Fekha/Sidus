using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Ship : Structure
{
    internal int maxMovementRange;
    private int movementRange;
    internal List<PathNode> path;
    public void InitializeShip(int _x, int _y, int _movementRange, int _hp, Station _station)
    {
        InitializeStructure(_x, _y, _hp);

        maxMovementRange = _movementRange;
        movementRange = _movementRange;
        resetMovementRange();

        stationId = _station.stationId;
        _station.ships.Add(this);
    }
    internal void resetMovementRange()
    {
        movementRange = maxMovementRange;
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