using System;
using System.Reflection;
using UnityEngine;

public class Technology : MonoBehaviour
{
    public int researchId { get; set; }
    public int level { get; set; }
    public int simulatedLevel { get { return level + ((simulatedAmount >= neededAmount) ? 1 : 0); } }
    public int neededAmount { get; set; }
    public int currentAmount { get; set; }
    public int simulatedAmount { get; set; }
    public string effectText { get; set; }
    public string currentEffectText { get; set; }
    public string requirementText { get; set; }

    public Technology(int _researchId)
    {
        researchId = _researchId;
        UpdateValues();
    }

    private void UpdateValues()
    {
        switch (researchId)
        {
            case 0:
                effectText = $"+1 max station level";
                currentEffectText = $"\n(Current max level: {1 + level})";
                neededAmount = 1 + level;
                break;
            case 1:
                effectText = $"+1 max fleet level";
                currentEffectText = $"\n(Current max level: {1 + level})";
                requirementText = "<b>Must research max <u>station level</u> first</b>\n\n";
                neededAmount = 1 + level;
                break;
            case 2:
                effectText = $"+1 max number of fleets";
                currentEffectText = $"\n(Current max fleets: {1 + level})";
                requirementText = "<b>Must research max <u>station level</u> first</b>\n\n";
                neededAmount = 1 + level;
                break;
            case 3:
                effectText = $"+2 HP for all units";
                currentEffectText = $"\n(Current bonus: +{level})";
                requirementText = "<b>Must research max <u>fleet level</u> first</b>\n\n";
                neededAmount = 1 + level;
                break;
            case 4:
                effectText = $"+1 kinetic power for all units";
                currentEffectText = $"\n(Current bonus: +{level})";
                neededAmount = 1 + level;
                break;
            case 5:
                effectText = $"+1 thermal power for all units";
                currentEffectText = $"\n(Current bonus: +{level})";
                neededAmount = 1 + level;
                break;
            case 6:
                effectText = $"+1 explosive power for all units";
                currentEffectText = $"\n(Current bonus: +{level})";
                neededAmount = 1 + level;
                break;
            case 7:
                effectText = $"+1 mining power for all units";
                currentEffectText = $"\n(Current bonus: +{level})";
                requirementText = "<b>Must research max <u>fleet level</u> first</b>\n\n";
                neededAmount = 1 + level;
                break;
            default:
                break;
        }
    }

    public void Research(Station station)
    {
        currentAmount++;
        if (currentAmount >= neededAmount) {
            currentAmount -= neededAmount;
            level++;
            switch ((ResearchType)researchId)
            {
                case ResearchType.ResearchHP:
                    station.researchHP();
                    break;
                case ResearchType.ResearchKinetic:
                    station.researchKinetic();
                    break;
                case ResearchType.ResearchThermal:
                    station.researchThermal();
                    break;
                case ResearchType.ResearchExplosive:
                    station.researchExplosive();
                    break;
                case ResearchType.ResearchMining:
                    station.researchMining();
                    break;
                default:
                    break;
            }
            UpdateValues();
        }
        simulatedAmount = currentAmount;
    }
}