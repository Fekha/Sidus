using TMPro;
using UnityEngine;

public class Ship : Structure
{
    internal int maxMovementRange;
    internal int movementRange;
    internal int stationId;
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
}