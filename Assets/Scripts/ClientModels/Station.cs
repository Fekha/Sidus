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
    internal int credits = 10;
    internal bool defeated = false;

    public void InitializeStation(int _x, int _y, string _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, int _level, Guid _structureId)
    {
        stationId = GameManager.i.Stations.Count;
        GameManager.i.Stations.Add(this);
        InitializeStructure(_x, _y, _color + " Station", _color, _hp, _range, _electricAttack, _thermalAttack, _voidAttack, _level, _structureId, 5);
    }
}