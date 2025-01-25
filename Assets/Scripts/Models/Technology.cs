using Models;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Windows;

public class Technology
{
    public TechnologyType researchId { get; set; }
    public int level { get; set; }
    public int currentAmount { get; set; }
    public int neededAmount { get; set; }
    public string effectText { get; set; }
    public string currentEffectText { get; set; }
    public string requirementText { get; set; }

    internal Technology(int _researchId)
    {
        researchId = (TechnologyType)_researchId;
        UpdateValues(1);
    }
    internal Technology(ServerTechnology tech)
    {
        researchId = (TechnologyType)tech.TechnologyId;
        level = tech.Level;
        currentAmount = tech.CurrentAmount;
        neededAmount = tech.NeededAmount;
        effectText = tech.EffectText;
        currentEffectText = tech.CurrentEffectText;
        requirementText = tech.RequirementText;
    }

    public ServerTechnology ToServerTechnology()
    {
        return new ServerTechnology()
        {
            GameGuid = Globals.GameMatch.GameGuid,
            TurnNumber = GameManager.i.TurnNumber,
            PlayerGuid = Globals.Account.PlayerGuid,
            TechnologyId = (int)researchId,
            Level = level,
            CurrentAmount = currentAmount,
            NeededAmount = neededAmount,
            EffectText = effectText,
            CurrentEffectText = currentEffectText,
            RequirementText = requirementText,
        };
    }
    private int GetNeededAmount(int level)
    {
        return 1;
        //increases whenever the input number is a triangular number, 1,2,2,3,3,3
        return ((int)Math.Floor((-1 + Math.Sqrt(1 + 8 * level-1)) / 2))+1;
    }

    private void UpdateValues(int modifier)
    {
        neededAmount = GetNeededAmount(modifier + level);
        switch (researchId)
        {
            case TechnologyType.ResearchStationLvl:
                effectText = $"Increase max station level to {modifier + 1 + level}";
                currentEffectText = $"\n(Current max level: {modifier + level})";
                break;
            case TechnologyType.ResearchFleetLvl:
                effectText = $"Increase max bomber level to {modifier + 1 + level}";
                currentEffectText = $"\n(Current max level: {modifier + level})";
                requirementText = "<b>Must research <u>max station level</u> first</b>\n\n";
                break;
            case TechnologyType.ResearchMaxFleets:
                effectText = $"Increase max number of bombers to {modifier + 1 + level}";
                currentEffectText = $"\n(Current max bombers: {modifier + level})";
                requirementText = "<b>Must research <u>max station level</u> first</b>\n\n";
                break;
            case TechnologyType.ResearchHP:
                effectText = $"+{Constants.HPGain} HP for all units";
                currentEffectText = $"\n(Current HP bonus: +{(level - 1 + modifier)*Constants.HPGain})";
                requirementText = "<b>Must research <u>power for all units</u> first</b>\n\n";
                break;
            case TechnologyType.ResearchKinetic:
                effectText = $"+1 kinetic power for all units";
                currentEffectText = $"\n(Current kinetic bonus: +{level - 1 + modifier})";
                break;
            case TechnologyType.ResearchThermal:
                effectText = $"+1 thermal power for all units";
                currentEffectText = $"\n(Current thermal bonus: +{level - 1 + modifier})";
                break;
            case TechnologyType.ResearchExplosive:
                effectText = $"+1 explosive power for all units";
                currentEffectText = $"\n(Current explosive bonus: +{level - 1 + modifier})";
                break;
            case TechnologyType.ResearchMining:
                effectText = $"+1 mining power for all units";
                currentEffectText = $"\n(Current mining bonus: +{(level - 1 + modifier)*2})";
                requirementText = "<b>Must research <u>power for all units</u> first</b>\n\n";
                break;
            default:
                break;    
        }
    }

    internal void Research(Station station, int modifier)
    {
        currentAmount += modifier;
        if (currentAmount >= neededAmount || currentAmount < 0) {
            switch (researchId)
            {
                case TechnologyType.ResearchHP:
                    station.researchHP(modifier);
                    break;
                case TechnologyType.ResearchKinetic:
                    station.researchKinetic(modifier);
                    break;
                case TechnologyType.ResearchThermal:
                    station.researchThermal(modifier);
                    break;
                case TechnologyType.ResearchExplosive:
                    station.researchExplosive(modifier);
                    break;
                case TechnologyType.ResearchMining:
                    station.researchMining(modifier);
                    break;
                default:
                    break;
            }
            if (modifier == 1)
            {
                level += modifier;
                currentAmount -= neededAmount;
                UpdateValues(1);
            }
            else if (modifier != 1)
            {
                UpdateValues(0);
                currentAmount = neededAmount-1;
                level += modifier;
            }
        }
    }

    internal bool GetCanQueueTech()
    {
        switch (researchId)
        {
            case TechnologyType.ResearchStationLvl:
                return level < 3;
            case TechnologyType.ResearchFleetLvl:
                return level < GameManager.i.MyStation.technology[(int)TechnologyType.ResearchStationLvl].level && level < 3;
            case TechnologyType.ResearchMaxFleets:
                return level < GameManager.i.MyStation.technology[(int)TechnologyType.ResearchStationLvl].level && level < 3;
            case TechnologyType.ResearchHP:
                return level < (GameManager.i.MyStation.technology[(int)TechnologyType.ResearchExplosive].level +
                    GameManager.i.MyStation.technology[(int)TechnologyType.ResearchKinetic].level +
                    GameManager.i.MyStation.technology[(int)TechnologyType.ResearchThermal].level);
            case TechnologyType.ResearchMining:
                return level < (GameManager.i.MyStation.technology[(int)TechnologyType.ResearchExplosive].level +
                    GameManager.i.MyStation.technology[(int)TechnologyType.ResearchKinetic].level +
                    GameManager.i.MyStation.technology[(int)TechnologyType.ResearchThermal].level);
            case TechnologyType.ResearchKinetic:
                return level <= GameManager.i.MyStation.technology[(int)TechnologyType.ResearchExplosive].level;
            case TechnologyType.ResearchThermal:
                return level < GameManager.i.MyStation.technology[(int)TechnologyType.ResearchKinetic].level;
            case TechnologyType.ResearchExplosive:
                return level < GameManager.i.MyStation.technology[(int)TechnologyType.ResearchThermal].level;
            default: return true;
        }
    }
}