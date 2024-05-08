using System;
using UnityEngine;

public class Module : MonoBehaviour
{
    internal Guid moduleGuid;
    internal int type;
    internal Sprite icon;
    internal String effectText;
    public Module(int _type, Guid _moduleGuid)
    {
        moduleGuid = _moduleGuid;
        type = _type;
        icon = Resources.Load<Sprite>($"Sprites/Modules/{_type}");
        switch (type)
        {
            case 0:
                effectText = "+2 Kinetic Power \n +1 Explosive Power \n +1 Thermal Resistance";
                break;
            case 1:
                effectText = "+2 Thermal Power \n +1 Kinetic Power \n +1 Explosive Resistance";
                break;
            case 2:
                effectText = "+3 Explosive Power \n +1 Kinetic Resistance";
                break;
            case 3:
                effectText ="+1 Movement \n -1 Explosive Resistance";
                break;
            case 4:
                effectText = "+3 Mining \n -1 Explosive Resistance";
                break;
            case 5:
                effectText ="+2 Explosive Power \n +1 Thermal Power \n +1  Kinetic Power";
                break;
            case 6:
                effectText = "+2 Kinetic Power \n +1 Explosive Power \n +1 Thermal Power";
                break;
            case 7:
                effectText = "+2 Thermal Power \n +1 Kinetic Power \n +1 Explosive Power";
                break;
            case 8:
                effectText ="+2 Explosive Power \n +2 Thermal Resistance";
                break;
            case 9:
                effectText ="+2 Kinetic Power \n +2 Kinetic Resistance";
                break;
            case 10:
                effectText = "+2 Thermal Power \n +2 Explosive Resistance";
                break;
            case 11:
                effectText = "+2 Kinetic Power \n +2 Explosive";
                break;
            case 12:
                effectText = "+2 Thermal Power \n +2 Explosive";
                break;
            case 13:
                effectText = "+4 Explosive";
                break;
            case 14:
                effectText = "+1 Movement \n -1 Kinetic Resistance";
                break;
            case 15:
                effectText = "+3 Mining \n -1 Kinetic Resistance";
                break;
            case 16:
                effectText = "+3 Explosive Power \n +2 Thermal Power \n -2 Kinetic Resistance";
                break;
            case 17:
                effectText = "+3 Thermal Power \n +2 Kinetic Power \n  -2 Explosive Resistance";
                break;
            case 18:
                effectText = "+3 Thermal Power \n +2 Explosive Power \n -2 Kinetic Resistance";
                break;
            case 19:
                effectText = "+5 Explosive Power \n -2 Kinetic Resistance";
                break;
            case 20:
                effectText = "+1 Kinetic Power \n +2 Explosive Resistance \n +1 Thermal Resistance";
                break;
            case 21:
                effectText = "+1 Thermal Power \n +2 Kinetic Resistance \n +1 Explosive Resistance";
                break;
            case 22:
                effectText = "+1 Explosive Power \n +2 Thermal Resistance \n +1 Kinetic Resistance";
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

        GameManager.i.AllModules.Add(this);
    }
}