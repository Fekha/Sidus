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
                effectText ="+2 Kinetic \n +1 Explosive";
                break;
            case 1:
                effectText ="+2 Thermal \n +1 Kinetic";
                break;
            case 2:
                effectText ="+3 Explosive";
                break;
            case 3:
                effectText ="+1 Movement \n -1 Explosive Damage Recieved";
                break;
            case 4:
                effectText ="+5 Max HP \n -1 Explosive Damage Taken";
                break;
            case 5:
                effectText ="+2 Explosive \n +1 Thermal \n +1 Kinetic \n +1 Kinetic Damage Recieved";
                break;
            case 6:
                effectText = "+2 Kinetic \n +1 Explosive \n +1 Thermal \n +1 Explosive Damage Recieved";
                break;
            case 7:
                effectText = "+2 Thermal \n +1 Kinetic \n +1 Explosive \n +1 Thermal Damage Recieved";
                break;
            case 8:
                effectText ="+2 Explosive \n -1 Thermal Damage Recieved";
                break;
            case 9:
                effectText ="+2 Kinetic \n -1 Kinetic Damage Recieved";
                break;
            case 10:
                effectText = "+2 Thermal \n -1 Explosive Damage Recieved";
                break;
            case 11:
                effectText = "+2 Kinetic \n +2 Explosive \n +1 Thermal Damage Recieved";
                break;
            case 12:
                effectText = "+2 Thermal \n +2 Explosive \n +1 Kinetic Damage Recieved";
                break;
            case 13:
                effectText = "+4 Explosive \n +1 Kinetic Damage Recieved";
                break;
            case 14:
                effectText ="+1 Movement \n -1 Thermal Damage Recieved";
                break;
            case 15:
                effectText = "+6 Max HP \n -1 Explosive Damage Recieved";
                break;
            case 16:
                effectText = "+3 Explosive \n +2 Thermal \n +2 Kinetic Damage Recieved";
                break;
            case 17:
                effectText = "+3 Thermal \n +2 Kinetic \n  +2 Explosive Damage Recieved";
                break;
            case 18:
                effectText = "+3 Thermal \n +2 Explosive \n +2 Kinetic Damage Recieved";
                break;
            case 19:
                effectText = "+5 Explosive \n +2 Kinetic Damage Recieved";
                break;
            case 20:
                effectText = "+1 Kinetic \n -2 Explosive Damage Recieved";
                break;
            case 21:
                effectText = "+1 Thermal \n -2 Kinetic Damage Recieved";
                break;
            case 22:
                effectText = "+1 Explosive \n -2 Thermal Damage Recieved";
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