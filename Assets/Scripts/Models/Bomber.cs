
using Models;
using System;

public class Bomber : Unit
{
    public void InitializeFleet(int _x, int _y, Station _station, int _color, int _hp, int _range, int _mining, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _unitGuid, Guid? _bombGuid)
    {
        playerColor = _station.playerColor;
        playerGuid = _station.playerGuid;
        _station.fleets.Add(this);
        _station.fleetCount++;
        unitName = $"{(PlayerColor)_color} Bomber {_station.fleetCount}";
        kineticDeployPower = 1;
        //thermalDeployPower = 1;
        explosiveDeployPower = 1;
        deployRange = 2;
        unitType = UnitType.Bomber;
        InitializeUnit(_x, _y, _color, _hp, _range, _electricAttack, _thermalAttack, _voidAttack, _unitGuid, _mining, _station.facing, UnitType.Bomber);
        currentPathNode.SetNodeColor(playerGuid);
        //if (_bombGuid != null)
        //{
        //    var bomb = GridManager.i.Deploy(this, (Guid)_bombGuid, _station.currentPathNode.actualCoords.AddCoords(currentPathNode.offSet[(int)(_station.facing+1)%6]), UnitType.Bomb,null,true);
        //    bomb.currentPathNode.SetNodeColor(playerGuid);
        //}
    }
}