using System;
using System.Reflection;
using UnityEngine;

public class Technology : MonoBehaviour
{
    public int researchId { get; set; }
    public int level { get; set; }
    public int neededAmount { get; set; }
    public int currentAmount { get; set; }
    public string effectText { get; set; }
    public string currentEffectText { get; set; }
    public string requirementText { get; set; }

    public Technology(int _researchId)
    {
        researchId = _researchId;
        UpdateValues(1);
    }

    private void UpdateValues(int modifier)
    {
        switch (researchId)
        {
            case 0:
                effectText = $"Increase max station level to {modifier + 1 + level}";
                currentEffectText = $"\n(Current max level: {modifier + level})";
                neededAmount = modifier + level;
                break;
            case 1:
                effectText = $"Increase max fleet level to {modifier + 1 + level}";
                currentEffectText = $"\n(Current max level: {modifier + level})";
                requirementText = "<b>Must increase max <u>station level</u> first</b>\n\n";
                neededAmount = modifier + level;
                break;
            case 2:
                effectText = $"Increase max number of fleets to {modifier + 1 + level}";
                currentEffectText = $"\n(Current max fleets: {modifier + level})";
                requirementText = "<b>Must increase max <u>station level</u> first</b>\n\n";
                neededAmount = modifier + level;
                break;
            case 3:
                effectText = $"+2 HP for all units";
                currentEffectText = $"\n(Current bonus: +{level - 1 + modifier})";
                requirementText = "<b>Must increase max <u>number of fleets</u> first<b>\n\n";
                neededAmount = modifier + level;
                break;
            case 4:
                effectText = $"+1 kinetic power for all units";
                currentEffectText = $"\n(Current bonus: +{level - 1 + modifier})";
                requirementText = "<b>Must increase max <u>fleet level</u> first</b>\n\n";
                neededAmount = modifier + level;
                break;
            case 5:
                effectText = $"+1 thermal power for all units";
                currentEffectText = $"\n(Current bonus: +{level - 1 + modifier})";
                requirementText = "<b>Must increase max <u>fleet level</u> first</b><b>\n\n";
                neededAmount = modifier + level;
                break;
            case 6:
                effectText = $"+1 explosive power for all units";
                currentEffectText = $"\n(Current bonus: +{level - 1 + modifier})";
                requirementText = "<b>Must increase max <u>fleet level</u> first</b>\n\n";
                neededAmount = modifier + level;
                break;
            case 7:
                effectText = $"+1 mining power for all units";
                currentEffectText = $"\n(Current bonus: +{level - 1 + modifier})";
                requirementText = "<b>Must increase max <u>number of fleets</u> first<b>\n\n";
                neededAmount = modifier + level;
                break;
            default:
                break;
        }
    }

    public void Research(Station station, int modifier)
    {
        currentAmount += modifier;
        if (currentAmount >= neededAmount || currentAmount < 0) {
            switch ((ResearchType)researchId)
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
}