using System;
using UnityEngine;

public class Module : MonoBehaviour
{
    internal Guid id;
    internal int type;
    internal Sprite icon;
    internal String effectText;
    public Module(int _type)
    {
        id = Guid.NewGuid();
        type = _type;
        icon = Resources.Load<Sprite>($"Sprites/Modules/{_type}");
        switch (type)
        {
            case 0:
                effectText = "+4 Electric Attack \n On Attach: Change Shield to Thermal";
                break;
            case 1:
                effectText = "+4 Thermal Attack \n On Attach: Change Shield to Void";
                break;
            case 2:
                effectText = "+4 Void Attack \n On Attach: Change Shield to Electric";
                break;
            case 3:
                effectText = "+1 Movement \n On Attach: Change Shield to Thermal";
                break;
            case 4:
                effectText = "+5 Max HP \n On Attach: Change Shield to Electric";
                break;
            case 5:
                effectText = "+2 Void Attack \n +2 Thermal Attack \n +1 Electric Attack \n On Attach: Change Shield to Electric";
                break;
            case 6:
                effectText = "+2 Electric Attack \n +2 Void Attack \n +1 Thermal Attack \n On Attach: Change Shield to Thermal";
                break;
            case 7:
                effectText = "+2 Thermal Attack \n +2 Electric Attack \n +1 Void Attack \n On Attach: Change Shield to Void";
                break;
            case 8:
                effectText = "+3 Void Attack \n +1 Electric Attack \n +1 Thermal Attack \n On Attach: Change Shield to Thermal";
                break;
            case 9:
                effectText = "+3 Electric Attack \n +1 Thermal Attack \n +1 Void Attack \n On Attach: Change Shield to Void";
                break;
            case 10:
                effectText = "+3 Thermal Attack \n +1 Electric Attack \n +1 Void Attack \n On Attach: Change Shield to Electric";
                break;
            default:
                break;
        }
        //other ideas
        //shield piercer, ignores shield of specifc type
        //Meteor hopper, ignores obsticles
        //Move through structures
        //reorder attacks, void first etc
        //double attacks, void round twice etc

        GameManager.i.AllModules.Add(this);
    }
}