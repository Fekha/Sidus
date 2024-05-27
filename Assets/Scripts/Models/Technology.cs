using System;
using System.Reflection;
using UnityEngine;

public class Technology : MonoBehaviour
{
    public int researchId { get; set; }
    public int level { get; set; }
    public int neededAmount { get; set; }
    public int currentAmount { get; set; }
    public int simulatedAmount { get; set; }
    public string effectText { get; set; }

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
                effectText = $"+1 max station level. (Current max level: {1 + level})";
                neededAmount = 1 + level;
                break;
            case 1:
                effectText = $"+1 max fleet level. (Current max level: {1 + level})";
                neededAmount = 1 + level;
                break;
            case 2:
                effectText = $"+1 max number of fleets. (Current max fleets: {1 + level})";
                neededAmount = 2 + level;
                break;
            case 3:
                effectText = $"+2 HP for all units. (Current bonus: +{level})";
                neededAmount = 1 + level;
                break;
            case 4:
                effectText = $"+1 kinetic power for all units. (Current bonus: +{level})";
                neededAmount = 3 + level;
                break;
            case 5:
                effectText = $"+1 thermal power for all units. (Current bonus: +{level})";
                neededAmount = 2 + level;
                break;
            case 6:
                effectText = $"+1 explosive power for all units. (Current bonus: +{level})";
                neededAmount = 1 + level;
                break;
            case 7:
                effectText = $"+1 mining power for all units. (Current bonus: +{level})";
                neededAmount = 2 + level;
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
            switch (researchId)
            {
                case 0:
                    station.maxStationLevel++;
                    break;
                case 1:
                    station.maxFleetLevel++;
                    break;
                case 2:
                    station.maxNumberOfFleets++;
                    break;
                case 3:
                    station.researchHP();
                    break;
                case 4:
                    station.researchKinetic();
                    break;
                case 5:
                    station.researchThermal();
                    break;
                case 6:
                    station.researchExplosive();
                    break;
                case 7:
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