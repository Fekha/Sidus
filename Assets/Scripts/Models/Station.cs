using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Station : Structure
{
    internal List<Action> actions = new List<Action>();
    internal List<Ship> ships = new List<Ship>();
    internal List<bool> modules = new List<bool>();

    //internal List<Outpost> outposts = new List<Outpost>();

    public void InitializeStation(int _x, int _y, string name, int _hp, int _range, int _shield, int _electricAttack, int _thermalAttack, int _voidAttack, int _level)
    {
        stationId = GameManager.i.stations.Count;
        GameManager.i.stations.Add(this);
        InitializeStructure(_x, _y, name, _hp, _range, _shield, _electricAttack, _thermalAttack, _voidAttack, _level);
    }
}