using StartaneousAPI.ServerModels;
using System;
using System.Collections.Generic;

public class Station : Unit
{
    internal List<Action> actions = new List<Action>();
    internal List<Fleet> fleets = new List<Fleet>();
    internal List<Module> modules = new List<Module>();
    internal int maxActions = 2;
    internal int maxFleets = 1; // 1+ station.level
    internal int score = 0;
    internal int credits = 6;
    internal bool defeated = false;
    internal int fleetCount = 0;

    public void InitializeStation(int _x, int _y, string _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _structureId, Direction _direction)
    {
        stationId = GameManager.i.Stations.Count;
        unitName = $"{_color} Station";
        GameManager.i.Stations.Add(this);
        InitializeUnit(_x, _y, _color, _hp, _range, _electricAttack, _thermalAttack, _voidAttack, _structureId, 3, _direction);
    }
}