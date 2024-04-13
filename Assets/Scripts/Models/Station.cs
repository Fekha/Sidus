using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Station : Structure
{
    internal List<Action> actions = new List<Action>();
    internal List<Ship> ships = new List<Ship>();
    
    //internal List<Outpost> outposts = new List<Outpost>();

    public void InitializeStation(int _x, int _y, int _hp)
    {
        stationId = GameManager.i.stations.Count;
        GameManager.i.stations.Add(this);
        InitializeStructure(_x, _y, _hp);
    }
}