using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Station : Structure
{
    internal List<Ship> ships = new List<Ship>();
    internal int stationId;
    //internal List<Outpost> outposts = new List<Outpost>();

    public void InitializeStation(int _x, int _y, int _hp)
    {
        InitializeStructure(_x, _y, _hp);
        stationId = GameManager.i.stations.Count;
        GameManager.i.stations.Add(this);
    }
}