using Models;
using System;
using System.Linq;
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
            turnsLeftOnMarket = x.TurnsLeft;
        }
    }

    internal ServerModule ToServerModule()
    {
        return new ServerModule()
        {
            GameGuid = Globals.GameMatch.GameGuid,
            TurnNumber = GameManager.i.TurnNumber,
            MidBid = minBid,
            ModuleGuid = moduleGuid,
            ModuleId = moduleId,
            TurnsLeft = turnsLeftOnMarket,
        };
    }
    private void SetModule(int _moduleId, Guid _moduleGuid)
    {
        moduleGuid = _moduleGuid;
        moduleId = _moduleId;
        icon = Resources.Load<Sprite>($"Sprites/Modules/{moduleId}");
        if(!GameManager.i.AllModules.Any(x=>x.moduleGuid == _moduleGuid))
            GameManager.i.AllModules.Add(this);
        switch (moduleId)
        {
            case 0:
                effectText = "+5 Kinetic Power\n+1 Thermal Damage Taken";
                break;
            case 1: //Roughly balancing everything on this module
                effectText = "+5 Thermal Power";
                break;
            case 2:
                effectText = "+5 Explosive Power\n-1 Thermal Damage Taken";
                break;
            case 3:
                effectText = "+1 Movement\n+1 Kinetic Power";
                break;
            case 4:
                effectText = "+1 Movement\n+1 Thermal Power";
                break;
            case 5:
                effectText = "+1 Movement\n+1 Explosive Power";
                break;
            case 6:
                effectText = "+2 Kinetic Power\n+3 Explosive Power";
                break;
            case 7:
                effectText = "+2 Kinetic Power\n+3 Thermal Power";
                break;
            case 8:
                effectText = "+6 Kinetic Power\n-1 Explosive Power\n+2 Thermal Damage Taken";
                break;
            case 9:
                effectText = "-1 Kinetic Power\n+6 Thermal Power\n+1 Explosive Damage Taken";
                break;
            case 10:
                effectText = "-1 Thermal Power\n+6 Explosive Power";
                break;
            case 11:
                effectText = "+1 Kinetic Power\n+2 Thermal Power\n+2 Explosive Power";
                break;
            case 12:
                effectText = "+3 Thermal Power\n+2 Explosive Power";
                break;
            case 13:
                effectText = "+2 Mining Power\n+2 Kinetic Power\n-2 Kinetic Damage Taken";
                break;
            case 14:
                effectText = "+2 Mining Power\n+2 Thermal Power\n-2 Thermal Damage Taken";
                break;
            case 15:
                effectText = "+2 Mining Power\n+2 Explosive Power\n-2 Explosive Damage Taken";
                break;
            case 16:
                effectText = "+3 Kinetic Power\n-2 Thermal Power\n+4 Explosive Power";
                break;
            case 17:
                effectText = "+3 Kinetic Power\n+4 Thermal Power\n-2 Explosive Power";
                break;
            case 18:
                effectText = "-2 Kinetic Power\n+3 Thermal Power\n+4 Explosive Power";
                break;
            case 19:
                effectText = "-2 Kinetic Power\n+5 Explosive Power\n+5 Max HP";
                break;
            case 20:
                effectText = "+1 Movement\n-2 Kinetic Damage Taken";
                break;
            case 21:
                effectText = "+1 Movement\n-2 Thermal Damage Taken";
                break;
            case 22:
                effectText = "+1 Movement\n-2 Explosive Damage Taken";
                break;
            case 23:
                effectText = "+3 Kinetic Power\n-2 Thermal Damage Taken\n-1 Explosive Damage Taken";
                break;
            case 24:
                effectText = "+3 Thermal Power\n-2 Kinetic Damage Taken\n-1 Explosive Damage Taken";
                break;
            case 25:
                effectText = "+3 Explosive Power\n-2 Kinetic Damage Taken\n-1 Thermal Damage Taken";
                break; 
            case 26:
                effectText = "+5 Max HP\n+3 Thermal Power\n-1 Kinetic Damage Taken";
                break;
            case 27:
                effectText = "+5 Max HP\n+3 Explosive Power\n-2 Kinetic Damage Taken";
                break;
            case 28:
                effectText = "+5 Max HP\n+3 Kinetic Power";
                break;
            case 29:
                effectText = "+5 Max HP\n+1 Kinetic Power\n+1 Explosive Deploy Power";
                break;
            case 30:
                effectText = "+5 Max HP\n+1 Thermal Power\n+1 Thermal Deploy Power";
                break;
            case 31:
                effectText = "+5 Max HP\n+1 Explosive Power\n+1 Kinetic Deploy Power";
                break; 
            case 32:
                effectText = "+1 credit per turn";
                rarity = Rarity.Rare;
                break; 
            case 33:
                effectText = "+2 credits per turn\n-2 Mining Power";
                rarity = Rarity.Rare;
                break; 
            case 34:
                effectText = "+4 credits per turn\n-1 Movement";
                rarity = Rarity.Rare;
                break;
            case 35:
                effectText = "+3 Mining Power\n+2 Kinetic Power\n-1 Thermal Power";
                break;
            case 36:
                effectText = "+3 Mining Power\n+1 Thermal Power";
                break;
            case 37:
                effectText = "+3 Mining Power\n+2 Explosive Power\n-1 Kinetic Power";
                break;
            case 38:
                effectText = "+7 Kinetic Power\n-2 Thermal Power\n+2 Thermal Damage Taken";
                break;
            case 39:
                effectText = "+7 Thermal Power\n-2 Explosive Power\n+1 Explosive Damage Taken";
                break;
            case 40:
                effectText = "-2 Thermal Power\n+7 Explosive Power";
                break;
            case 41:
                effectText = "Provide full Kinetic Power amount while supporting.";
                rarity = Rarity.Rare;
                break;
            case 42:
                effectText = "Damage dealt reduces Max HP\n+3 Thermal Power";
                rarity = Rarity.Rare;
                break;
            case 43:
                effectText = "Double all repairing\n+4 Explosive Power";
                rarity = Rarity.Rare;
                break;
            case 44:
                effectText = "Repair 3 HP when entering combat\n+3 Explosive Power";
                rarity = Rarity.Rare;
                break;
            case 45:
                effectText = "Gain 2 Kinetic Power when entering combat";
                rarity = Rarity.Rare;
                break;
            case 46:
                effectText = "Gain 2 Thermal Power when entering combat";
                rarity = Rarity.Rare;
                break;
            case 47:
                effectText = "Gain 2 Explosive Power when entering combat";
                rarity = Rarity.Rare;
                break;
            case 48:
                effectText = "Gain 3 Credits after destroying an asteroid\n+2 Mining Power";
                rarity = Rarity.Rare;
                break;
            case 49:
                effectText = "Stats are hidden from enemies\n+2 Kinetic Power";
                rarity = Rarity.Rare;
                break;
            case 50:
                effectText = "If destroyed while defending\ndestroy the attacking fleet\nand this module as well";
                rarity = Rarity.Rare;
                break;
            case 51:
                effectText = "Gain 5 Max HP after destroying an asteroid\n+1 Mining Power";
                rarity = Rarity.Rare;
                break;
            case 52:
                effectText = "4 Kinetic Power\n+1 Explosive Power\n+1 Thermal Damage Taken";
                break;
            case 53:
                effectText = "+4 Thermal Power\n+1 Explosive Power";
                break;
            case 54:
                effectText = "+1 Kinetic Power\n+4 Explosive Power\n-1 Thermal Damage Taken";
                break;
            case 55:
                effectText = "+3 Kinetic Power\n-1 Thermal Damage Taken\n-2 Explosive Damage Taken";
                break;
            case 56:
                effectText = "+3 Thermal Power\n-1 Kinetic Damage Taken\n-2 Explosive Damage Taken";
                break;
            case 57:
                effectText = "+3 Explosive Power\n-1 Kinetic Damage Taken\n-2 Thermal Damage Taken"; 
                break;
            case 58:
                effectText = "Provide full Thermal Power amount while supporting.";
                rarity = Rarity.Rare;
                break;
            case 59:
                effectText = "Provide full Explosive Power amount while supporting.";
                rarity = Rarity.Rare;
                break;
            case 60:
                effectText = "Stats are hidden from enemies\n+2 Thermal Power";
                rarity = Rarity.Rare;
                break;
            case 61:
                effectText = "Stats are hidden from enemies\n+2 Explosive Power";
                rarity = Rarity.Rare;
                break;
            case 62:
                effectText = "+5 Kinetic Power\n+1 Explosive Damage Taken";
                break;
            case 63:
                effectText = "+5 Explosive Power\n-1 Thermal Damage Taken";
                break;
            case 64:
                effectText = "+6 Kinetic Power\n-1 Thermal Power\n+2 Explosive Damage Taken";
                break;
            case 65:
                effectText = "+6 Thermal Power\n-1 Explosive Power\n+1 Kinetic Damage Taken";
                break;
            case 66:
                effectText = "-1 Kinetic Power\n+6 Explosive Power";
                break;
            case 67:
                effectText = "+1 Deploy Range\n-1 Kinetic Deploy Power";
                break;
            case 68:
                effectText = "+1 Deploy Range\n-1 Thermal Deploy Power";
                break;
            case 69:
                effectText = "+1 Deploy Range\n-1 Explosive Deploy Power";
                break;
            case 70:
                effectText = "+1 Explosive Power\n+2 Kinetic Deploy Power\n+1 Thermal Damage Taken";
                break;
            case 71:
                effectText = "+1 Explosive Power\n+2 Thermal Deploy Power";
                break;
            case 72:
                effectText = "+1 Kinetic Power\n+2 Explosive Deploy Power\n-1 Thermal Damage Taken";
                break;
            case 73:
                effectText = "Damage dealt reduces Max HP\n+3 Kinetic Power";
                rarity = Rarity.Rare;
                break;
            case 74:
                effectText = "Damage dealt reduces Max HP\n+3 Explosive Power";
                rarity = Rarity.Rare;
                break;
            case 75:
                effectText = "+5 Max HP\n+1 Kinetic Deploy Power\n-2 Explosive Damage Taken";
                break;
            case 76:
                effectText = "+5 Max HP\n+1 Thermal Deploy Power\n-2 Kinetic Damage Taken";
                break;
            case 77:
                effectText = "+5 Max HP\n+1 Explosive Deploy Power\n-2 Thermal Damage Taken";
                break;
            case 78:
                effectText = "+6 Kinetic Power\n-1 Thermal Deploy Power\n+1 Thermal Damage Taken";
                break;
            case 79:
                effectText = "+6 Thermal Power\n-1 Explosive Deploy Power\n+1 Explosive Damage Taken";
                break;
            case 80:
                effectText = "+6 Explosive Power\n-1 Kinetic Deploy Power\n+1 Kinetic Damage Taken";
                break;
            case 81:
                effectText = "+3 Kinetic Deploy Power\n-1 Thermal Deploy Power\n-1 Thermal Damage Taken";
                break;
            case 82:
                effectText = "+3 Thermal Deploy Power\n-1 Explosive Deploy Power\n-1 Explosive Damage Taken";
                break;
            case 83:
                effectText = "+3 Explosive Deploy Power\n-1 Kinetic Deploy Power\n-1 Kinetic Damage Taken";
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