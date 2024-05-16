using StartaneousAPI.ServerModels;
using System;
using System.Security.Cryptography;
using UnityEngine;
using UnityEngine.UIElements;

public class Module : MonoBehaviour
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
                effectText = "+2 Explosive Power \n +1 Kinetic Power \n -2 Thermal Damage Taken";
                break;
            case 1:
                effectText = "+2 Thermal Power \n +1 Kinetic Power \n -2 Explosive Damage Taken";
                break;
            case 2:
                effectText = "+4 Explosive Power \n +1 Kinetic Damage Taken";
                break;
            case 3:
                effectText = "+1 Movement \n +3 Explosive Damage Taken";
                break;
            case 4:
                effectText = "+3 Mining \n -1 Explosive Power";
                break;
            case 5:
                effectText = "+2 Explosive Power \n +1 Thermal Power \n +1 Kinetic Power";
                break;
            case 6:
                effectText = "+2 Kinetic Power \n +1 Explosive Power \n +1 Thermal Power";
                break;
            case 7:
                effectText = "+2 Thermal Power \n +1 Kinetic Power \n +1 Explosive Power";
                break;
            case 8:
                effectText = "+3 Explosive Power \n -2 Kinetic Damage Taken";
                break;
            case 9:
                effectText = "+3 Kinetic Power \n -2 Thermal Damage Taken";
                break;
            case 10:
                effectText = "+3 Thermal Power \n -2 Explosive Damage Taken";
                break;
            case 11:
                effectText = "+2 Kinetic Power \n +2 Explosive";
                break;
            case 12:
                effectText = "+2 Thermal Power \n +2 Explosive";
                break;
            case 13:
                effectText = "+4 Explosive \n +1 Thermal Damage Taken";
                break;
            case 14:
                effectText = "+1 Movement \n -3 Kinetic Power";
                break;
            case 15:
                effectText = "+3 Mining \n -1 Kinetic Power";
                break;
            case 16:
                effectText = "+3 Kinetic Power \n +3 Explosive Power \n -2 Thermal Power";
                break;
            case 17:
                effectText = "+3 Thermal Power \n +3 Kinetic Power \n  -2 Explosive Power";
                break;
            case 18:
                effectText = "+3 Thermal Power \n +3 Explosive Power \n -2 Kinetic Power";
                break;
            case 19:
                effectText = "+6 Explosive Power \n -2 Thermal Power \n -1 Kinetic Armor";
                break;
            case 20:
                effectText = "+1 Movement \n -2 Explosive Power \n +2 Thermal Damage Taken";
                break;
            case 21:
                effectText = "+1 Movement \n -2 Kinetic Power \n +2 Explosive Damage Taken";
                break;
            case 22:
                effectText = "+1 Movement \n -2 Thermal Power \n +2 Kinetic Damage Taken";
                break;
            case 23:
                effectText = "+2 Explosive Power \n -2 Thermal Damage Taken \n -2 Kinetic Damage Taken";
                break;
            case 24:
                effectText = "+2 Kinetic Power \n -2 Explosive Damage Taken \n -2 Thermal Damage Taken";
                break;
            case 25:
                effectText = "+2 Thermal Power \n -2 Explosive Damage Taken \n -2 Kinetic Damage Taken";
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