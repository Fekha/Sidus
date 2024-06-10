using StartaneousAPI.ServerModels;
using System;
using System.Collections.Generic;
using static System.Collections.Specialized.BitVector32;

public class Station : Unit
{
    internal List<Action> actions = new List<Action>();
    internal List<Fleet> fleets = new List<Fleet>();
    internal List<Module> modules = new List<Module>();
    internal List<Technology> technology = new List<Technology>();
    internal int maxActions = 2;
    internal int credits = 6;
    internal bool defeated = false;
    internal int fleetCount = 0;
    internal int bonusKinetic = 0;
    internal int bonusThermal = 0;
    internal int bonusExplosive = 0;
    internal int bonusHP = 0;
    internal int bonusMining = 0;
    internal int score = 0;

    public void InitializeStation(int _x, int _y, string _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _structureId, Direction _direction)
    {
        for (int i = 0; i <= Constants.TechAmount; i++)
        {
            technology.Add(new Technology(i));
        }
        stationId = GameManager.i.Stations.Count;
        unitName = $"{_color} Station";
        GameManager.i.Stations.Add(this);
        InitializeUnit(_x, _y, _color, _hp, _range, _electricAttack, _thermalAttack, _voidAttack, _structureId, 2, _direction);
    }

    internal void researchKinetic(int modifier)
    {
        bonusKinetic += modifier;
        kineticPower += modifier;
        fleets.ForEach(x => x.kineticPower += modifier);
    }
    internal void researchThermal(int modifier)
    {
        bonusThermal += modifier;
        thermalPower += modifier;
        fleets.ForEach(x => x.thermalPower += modifier);
    }
    internal void researchExplosive(int modifier)
    {
        bonusExplosive += modifier;
        explosivePower += modifier;
        fleets.ForEach(x => x.explosivePower += modifier);
    }
    internal void researchHP(int modifier)
    {
        bonusHP+=(2 * modifier);
        IncreaseMaxHP(2 * modifier);
        fleets.ForEach(x => x.IncreaseMaxHP(2* modifier));
    }
    internal void researchMining(int modifier)
    {
        bonusExplosive += modifier;
        IncreaseMaxMining(1 * modifier);
        fleets.ForEach(x => IncreaseMaxMining(1 * modifier));
    }

    internal void AOERegen(int amount)
    {
        RegenHP(amount);
        var neighbors = GridManager.i.GetNeighbors(currentPathNode, false);
        foreach (var neighbor in neighbors)
        {
            if (neighbor.unitOnPath != null && neighbor.unitOnPath.stationId == stationId)
            {
                neighbor.unitOnPath.RegenHP(amount);
            }
        }
    }
}