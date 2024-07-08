
using Models;
using System;

public class Bomb : Unit
{
    public void InitializeBomb(int _x, int _y, Station _station, int _color, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _unitGuid)
    {
        playerColor = _station.playerColor;
        playerGuid = _station.playerGuid;
        _station.bombs.Add(this);
        unitName = $"{(PlayerColor)_color} Bomb";
        unitType = UnitType.Bomb;
        InitializeUnit(_x, _y, _color, 0, 0, _electricAttack, _thermalAttack, _voidAttack, _unitGuid, 0, _station.facing, UnitType.Bomb);
    }
}