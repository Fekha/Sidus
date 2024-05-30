using StartaneousAPI.ServerModels;
using System;
using UnityEngine;

public class Module
{
    internal Guid moduleGuid;
    internal int moduleId;
    internal Sprite icon;
    internal string effectText;
    internal Rarity rarity = Rarity.Common;
    internal int minBid = 3;
    internal int currentBid = 3;
    internal int turnsLeftOnMarket = 5;
    public Module(int _moduleId, Guid _moduleGuid)
    {
        SetModule(_moduleId, _moduleGuid);
    }
    public Module(ServerModule? x)
    {
        if (x != null)
        {
            SetModule(x.ModuleId, (Guid)x.ModuleGuid);
            minBid = x.MidBid;
            currentBid = x.PlayerBid;
            turnsLeftOnMarket = x.TurnsLeft;
        }
    }

    internal ServerModule ToServerModule()
    {
        return new ServerModule()
        {
            MidBid = minBid,
            ModuleGuid = moduleGuid,
            ModuleId = moduleId,
            PlayerBid = currentBid,
            TurnsLeft = turnsLeftOnMarket,
        };
    }

    private void SetModule(int _moduleId, Guid _moduleGuid)
    {
        moduleGuid = _moduleGuid;
        moduleId = _moduleId;
        icon = Resources.Load<Sprite>($"Sprites/Modules/{moduleId}");
        GameManager.i.AllModules.Add(this);
        switch (moduleId)
        {
            case 0:
                effectText = "+3 Kinetic Power \n +1 Thermal Damage Taken";
                break;
            case 1:
                effectText = "+3 Thermal Power \n +1 HP";
                break;
            case 2:
                effectText = "+3 Explosive Power \n -1 Thermal Damage Taken";
                break;
            case 3:
                effectText = "+1 Movement \n -1 Kinetic Power";
                break;
            case 4:
                effectText = "+1 Movement \n -1 Thermal Power";
                break;
            case 5:
                effectText = "+1 Movement \n -1 Explosive Power";
                break;
            case 6:
                effectText = "+2 Kinetic Power \n +1 Explosive Power";
                break;
            case 7:
                effectText = "+1 Kinetic Power \n +2 Thermal Power";
                break;
            case 8:
                effectText = "-1 Kinetic Power \n +4 Explosive Power ";
                break;
            case 9:
                effectText = "+3 Kinetic Power \n +1 Explosive Damage Taken";
                break;
            case 10:
                effectText = "+3 Explosive Power \n -1 Kinetic Damage Taken";
                break;
            case 11:
                effectText = "+1 Kinetic Power \n +1 Thermal Power \n +1 Explosive Power";
                break;
            case 12:
                effectText = "+2 Thermal Power \n +1 Explosive Power";
                break;
            case 13:
                effectText = "+2 Mining Power \n -2 Kinetic Damage Taken";
                break;
            case 14:
                effectText = "+2 Mining Power \n -2 Thermal Damage Taken";
                break;
            case 15:
                effectText = "+2 Mining Power \n -2 Explosive Damage Taken";
                break;
            case 16:
                effectText = "+3 Kinetic Power \n -2 Thermal Power \n +2 Explosive Power";
                break;
            case 17:
                effectText = "+2 Kinetic Power \n +3 Thermal Power \n -2 Explosive Power";
                break;
            case 18:
                effectText = "-2 Kinetic Power \n +4 Thermal Power \n +1 Explosive Power";
                break;
            case 19:
                effectText = "-2 Kinetic Power \n +4 Explosive Power \n +3 HP";
                break;
            case 20:
                effectText = "+1 Movement \n +2 Kinetic Damage Taken";
                break;
            case 21:
                effectText = "+1 Movement \n +2 Thermal Damage Taken";
                break;
            case 22:
                effectText = "+1 Movement \n +2 Explosive Damage Taken";
                break;
            case 23:
                effectText = "+1 Kinetic Power \n -2 Thermal Damage Taken \n -2 Explosive Damage Taken";
                break;
            case 24:
                effectText = "+1 Thermal Power \n -2 Kinetic Damage Taken \n -2 Explosive Damage Taken";
                break;
            case 25:
                effectText = "+1 Explosive Power \n -2 Kinetic Damage Taken \n -2 Thermal Damage Taken";
                break; 
            case 26:
                effectText = "+3 HP \n -2 Kinetic Damage Taken";
                break;
            case 27:
                effectText = "+3 HP \n -2 Thermal Damage Taken";
                break;
            case 28:
                effectText = "+3 HP \n -2 Explosive Damage Taken";
                break;
            case 29:
                effectText = "+3 HP \n +1 Kinetic Power";
                break;
            case 30:
                effectText = "+3 HP \n +1 Thermal Power";
                break;
            case 31:
                effectText = "+3 HP \n +1 Explosive Power";
                break; 
            case 32:
                effectText = "+1 credit per turn \n -2 Explosive Power";
                rarity = Rarity.Rare;
                break; 
            case 33:
                effectText = "+2 credits per turn \n -2 Mining Power";
                rarity = Rarity.Rare;
                break; 
            case 34:
                effectText = "+3 credits per turn \n -2 Movement";
                rarity = Rarity.Rare;
                break;
            case 35:
                effectText = "+2 Mining Power \n +1 Kinetic Power";
                break;
            case 36:
                effectText = "+2 Mining Power \n +1 Thermal Power";
                break;
            case 37:
                effectText = "+2 Mining Power \n +1 Explosive Power";
                break;
            case 38:
                effectText = "+5 Kinetic Power \n -2 Thermal Power \n +4 Explosive Damage Taken";
                break;
            case 39:
                effectText = "+5 Thermal Power \n -2 Explosive Power \n +3 Kinetic Damage Taken";
                break;
            case 40:
                effectText = "-2 Kinetic Power \n +5 Explosive Power \n +2 Thermal Damage Taken";
                break;
            case 41:
                effectText = "Support allies at full Power.";
                rarity = Rarity.Rare;
                break;
            case 42:
                effectText = "Damage dealt reduces max HP.";
                rarity = Rarity.Rare;
                break;
            case 43:
                effectText = "Double all repairing.";
                rarity = Rarity.Rare;
                break;
            case 44:
                effectText = "Repair 3 HP when entering combat.";
                rarity = Rarity.Rare;
                break;
            case 45:
                effectText = "Gain 1 Kinetic Power when entering combat.";
                rarity = Rarity.Rare;
                break;
            case 46:
                effectText = "Gain 1 Thermal Power when entering combat.";
                rarity = Rarity.Rare;
                break;
            case 47:
                effectText = "Gain 1 Explosive Power when entering combat.";
                rarity = Rarity.Rare;
                break;
            case 48:
                effectText = "Gain 1 Mining Power after destroying an asteroid.";
                rarity = Rarity.Rare;
                break;
            case 49:
                effectText = "Stats are hidden from enemies.";
                rarity = Rarity.Rare;
                break;
            case 50:
                effectText = "If destroyed in combat while defending, destroy the attacking fleet as well.";
                rarity = Rarity.Rare;
                break;
            case 51:
                effectText = "Gain 2 HP after destroying an asteroid.";
                rarity = Rarity.Rare;
                break;
            default:
                break;
        }
        //other ideas
        //shield piercer, ignores shield of specifc type
        //Meteor hopper, ignores obsticles
        //Move through structures
        //reorders, Explosive first etc
        //doubles, Explosive round twice etc
    }
}