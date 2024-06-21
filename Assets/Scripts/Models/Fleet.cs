
using Models;
using System;

public class Fleet : Unit
{
    public void InitializeFleet(int _x, int _y, Station _station, int _color, int _hp, int _range, int _mining, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _structureId)
    {
        playerColor = _station.playerColor;
        playerGuid = _station.playerGuid;
        _station.fleets.Add(this);
        _station.fleetCount++;
        unitName = $"{(PlayerColor)_color} Fleet {_station.fleetCount}";
        InitializeUnit(_x, _y, _color, _hp, _range, _electricAttack, _thermalAttack, _voidAttack, _structureId, _mining, _station.facing);
    }
}