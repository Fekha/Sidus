using Models;
using System;
using System.Reflection;
using UnityEngine;
using UnityEngine.Windows;

public class Technology
{
    public ResearchType researchId { get; set; }
    public int level { get; set; }
    public int currentAmount { get; set; }
    public int neededAmount { get; set; }
    public string effectText { get; set; }
    public string currentEffectText { get; set; }
    public string requirementText { get; set; }

    internal Technology(int _researchId)
    {
        researchId = (ResearchType)_researchId;
        UpdateValues(1);
    }
    internal Technology(ServerTechnology tech)
    {
        researchId = (ResearchType)tech.TechnologyId;
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
            case ResearchType.ResearchStationLvl:
                effectText = $"Increase max station level to {modifier + 1 + level}";
                currentEffectText = $"\n(Current max level: {modifier + level})";
                break;
            case ResearchType.ResearchFleetLvl:
                effectText = $"Increase max fleet level to {modifier + 1 + level}";
                currentEffectText = $"\n(Current max level: {modifier + level})";
                requirementText = "<b>Must research <u>max station level</u> first</b>\n\n";
                break;
            case ResearchType.ResearchMaxFleets:
                effectText = $"Increase max number of fleets to {modifier + 1 + level}";
                currentEffectText = $"\n(Current max fleets: {modifier + level})";
                requirementText = "<b>Must research <u>max fleet level</u> first</b>\n\n";
                break;
            case ResearchType.ResearchHP:
                effectText = $"+2 max HP for all units";
                currentEffectText = $"\n(Current bonus: +{level - 1 + modifier})";
                requirementText = "<b>Must research <u>max fleet level</u> first</b>\n\n";
                break;
            case ResearchType.ResearchKinetic:
                effectText = $"+1 kinetic power for all units";
                currentEffectText = $"\n(Current bonus: +{level - 1 + modifier})";
                requirementText = "<b>Must research <u>+1 thermal power for all units</u> first</b>\n\n";
                break;
            case ResearchType.ResearchThermal:
                effectText = $"+1 thermal power for all units";
                currentEffectText = $"\n(Current bonus: +{level - 1 + modifier})";
                requirementText = "<b>Must research <u>+1 explosive power for all units</u> first</b>\n\n";
                break;
            case ResearchType.ResearchExplosive:
                effectText = $"+1 explosive power for all units";
                currentEffectText = $"\n(Current bonus: +{level - 1 + modifier})";
                break;
            case ResearchType.ResearchMining:
                effectText = $"+1 mining power for all units";
                currentEffectText = $"\n(Current bonus: +{level - 1 + modifier})";
                requirementText = "<b>Must research <u>+2 max HP for all units</u> first</b>\n\n";
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
                case ResearchType.ResearchHP:
                    station.researchHP(modifier);
                    break;
                case ResearchType.ResearchKinetic:
                    station.researchKinetic(modifier);
                    break;
                case ResearchType.ResearchThermal:
                    station.researchThermal(modifier);
                    break;
                case ResearchType.ResearchExplosive:
                    station.researchExplosive(modifier);
                    break;
                case ResearchType.ResearchMining:
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
            case ResearchType.ResearchFleetLvl:
                return level < GameManager.i.MyStation.technology[(int)ResearchType.ResearchStationLvl].level;
            case ResearchType.ResearchMaxFleets:
                return level < GameManager.i.MyStation.technology[(int)ResearchType.ResearchFleetLvl].level;
            //case ResearchType.ResearchHP:
            //    canQueue = tech.level < MyStation.technology[(int)ResearchType.ResearchFleetLvl].level;
            //    break;
            case ResearchType.ResearchMining:
                return level < GameManager.i.MyStation.technology[(int)ResearchType.ResearchHP].level;
            case ResearchType.ResearchKinetic:
                return level <= GameManager.i.MyStation.technology[(int)ResearchType.ResearchThermal].level;
            case ResearchType.ResearchThermal:
                return level <= GameManager.i.MyStation.technology[(int)ResearchType.ResearchExplosive].level;
            //case ResearchType.ResearchExplosive:
            //    canQueue = tech.level <= MyStation.technology[(int)ResearchType.ResearchThermal].level;
            //    break;
            default: return true;
        }
    }
}