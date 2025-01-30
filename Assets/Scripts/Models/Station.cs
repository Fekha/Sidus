using Models;
using System;
using System.Collections.Generic;
using System.Linq;

public class Station : Unit
{
    internal List<Action> actions = new List<Action>();
    internal List<Bomber> fleets = new List<Bomber>();
    internal List<Bomb> bombs = new List<Bomb>();
    internal List<Module> modules = new List<Module>();
    internal List<Technology> technology = new List<Technology>();
    internal int maxActions;
    internal int credits;
    internal int fleetCount;
    internal int bonusKinetic;
    //internal int bonusThermal;
    internal int bonusExplosive;
    internal int bonusHP;
    internal int bonusMining;
    internal int score;

    public void InitializeStation(int _x, int _y, int _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _stationGuid, Direction _direction, Guid _fleetGuid, Guid _bombGuid, int _credits)
    {
        GameManager.i.Stations.Add(this);
        for (int i = 0; i < Constants.TechAmount; i++)
        {
            technology.Add(new Technology(i));
        }
        playerColor = (PlayerColor)GameManager.i.Stations.Count;
        playerGuid = _stationGuid;
        unitName = $"{(PlayerColor)_color} Station";
        unitType = UnitType.Station;
        credits = _credits;
        kineticDeployPower = 3;
        //thermalDeployPower = 4;
        explosiveDeployPower = 5;
        deployRange = 1;
        InitializeUnit(_x, _y, _color, _hp, _range, _electricAttack, _thermalAttack, _voidAttack, _stationGuid, 2, _direction, UnitType.Station);
        currentPathNode.SetNodeColor(playerGuid);
        var fleet = GridManager.i.Deploy(this, _fleetGuid, currentPathNode.actualCoords.AddCoords(currentPathNode.offSet[(int)facing]), UnitType.Bomber, _bombGuid);
        fleet.currentPathNode.SetNodeColor(playerGuid);
    }
    public void InitializeStation(GamePlayer player)
    {
        GameManager.i.Stations.Add(this);
        technology = player.Technology.Select(x => new Technology(x)).ToList();
        modules = GameManager.i.AllModules.Where(x => player.ModulesGuids.Contains(x.moduleGuid.ToString())).ToList();
        maxActions = player.MaxActions;
        credits = player.Credits;
        fleetCount = player.FleetCount;
        bonusKinetic = player.BonusKinetic;
        //bonusThermal = player.BonusThermal;
        bonusExplosive = player.BonusExplosive;
        bonusHP = player.BonusHP;
        bonusMining = player.BonusMining;
        score = player.Score;
        InitializeUnit(player.Units.FirstOrDefault(x=>x.UnitType == (int)UnitType.Station));
        foreach (var unit in player.Units)
        {
            if (unit.UnitType == (int)UnitType.Bomb)
            {
                var bombObj = Instantiate(GridManager.i.bombPrefab);
                bombObj.transform.SetParent(GridManager.i.characterParent);
                var bombNode = bombObj.AddComponent<Bomb>();
                bombNode.InitializeUnit(unit);
                bombs.Add(bombNode);
            }
            else if (unit.UnitType == (int)UnitType.Bomber)
            {
                var fleetObj = Instantiate(GridManager.i.unitPrefab);
                fleetObj.transform.SetParent(GridManager.i.characterParent);
                var fleetNode = fleetObj.AddComponent<Bomber>();
                fleetNode.InitializeUnit(unit);
                fleets.Add(fleetNode);
            }
        }
        actions = player.Actions.Select(x => new Action(x)).ToList();
    }
    internal void researchKinetic(int modifier)
    {
        bonusKinetic += modifier;
        kineticPower += modifier;
        fleets.ForEach(x => x.kineticPower += modifier);
    }
    //internal void researchThermal(int modifier)
    //{
    //    bonusThermal += modifier;
    //    thermalPower += modifier;
    //    fleets.ForEach(x => x.thermalPower += modifier);
    //}
    internal void researchExplosive(int modifier)
    {
        bonusExplosive += modifier;
        explosivePower += modifier;
        fleets.ForEach(x => x.explosivePower += modifier);
    }
    internal void researchHP(int modifier)
    {
        bonusHP+=(2 * modifier);
        IncreaseHP(2 * modifier);
        fleets.ForEach(x => x.IncreaseHP(2* modifier));
    }
    internal void researchMining(int modifier)
    {
        bonusMining += modifier;
        IncreaseMaxMining(1 * modifier);
        fleets.ForEach(x => x.IncreaseMaxMining(1 * modifier));
    }

    internal void AOERegen(int amount)
    {
        RegenHP(amount, false, true);
        var neighbors = GridManager.i.GetNeighbors(currentPathNode, false);
        foreach (var neighbor in neighbors)
        {
            if (neighbor.unitOnPath != null && neighbor.unitOnPath.playerGuid == playerGuid)
            {
                neighbor.unitOnPath.RegenHP(amount, false, true);
            }
        }
    }

    internal void GainCredits(int creditsGained, Unit unit, bool queued = false, bool stagger = true)
    {
        credits += creditsGained;
        if (creditsGained != 0 && !queued)
        {
            string plural = Math.Abs(creditsGained) == 1 ? "" : "s";
            string positive = creditsGained > 0 ? "+" : "";
            StartCoroutine(GameManager.i.FloatingTextAnimation($"{positive}{creditsGained} Credit{plural}", unit.transform, unit, stagger));
        }
    }

    internal List<ServerUnit> GetServerUnits()
    {
        var serverUnits = new List<ServerUnit>();
        serverUnits.Add(ToServerUnit());
        serverUnits.AddRange(fleets.Select(x => x.ToServerUnit()));
        serverUnits.AddRange(bombs.Select(x => x.ToServerUnit()));
        return serverUnits;
    }
}