
using System;

public class Fleet : Unit
{
    public void InitializeFleet(int _x, int _y, Station _station, string _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _structureId)
    {
        stationId = _station.stationId;
        _station.fleets.Add(this);
        _station.fleetCount++;
        unitName = $"{_color} Fleet {_station.fleetCount}";
        InitializeStructure(_x, _y, _color, _hp, _range,_electricAttack,_thermalAttack,_voidAttack, _structureId, 3, _station.facing);
    }
}