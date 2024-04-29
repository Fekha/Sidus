
using System;

public class Ship : Structure
{
    public void InitializeShip(int _x, int _y, Station _station, string _color, int _hp, int _range, int _shield, int _electricAttack, int _thermalAttack, int _voidAttack, int _level, Guid _structureId)
    {
        stationId = _station.stationId;
        _station.ships.Add(this);
        InitializeStructure(_x, _y, _color + " Fleet", _color, _hp, _range,_shield,_electricAttack,_thermalAttack,_voidAttack, _level, _structureId);
    }
}