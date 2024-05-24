using StartaneousAPI.ServerModels;
using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements.Experimental;

public class Unit : Node
{
    internal string unitName;
    internal string color;
    internal Guid unitGuid;
    internal int stationId;
    internal int maxHP;
    internal int HP;
    internal int maxRange;
    internal int movement;
    internal int kineticPower;
    internal int thermalPower;
    internal int explosivePower;
    internal int kineticArmor;
    internal int thermalArmor;
    internal int explosiveArmor;
    internal int mining;
    internal int level = 1;
    internal int globalCreditGain = 0;
    internal bool hasMoved = false;
    //internal bool hasMinedThisTurn = false;
    internal List<Module> attachedModules = new List<Module>();
    internal TextMeshPro HPText;
    internal TextMeshPro statText;
    internal GameObject selectIcon;
    internal GameObject inCombatIcon;
    internal Direction facing;
    internal Transform unitImage;
    internal int maxAttachedModules = 1; // 1+ station.level
    public void InitializeUnit(int _x, int _y, string _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, Guid _unitGuid, int _mining, Direction _direction)
    {
        facing = _direction;
        unitGuid = _unitGuid;
        location = new Coords(_x, _y);
        color = _color;
        maxHP = _hp;
        HP = _hp;
        maxRange = _range;
        movement = _range;
        kineticPower = _electricAttack;
        thermalPower = _thermalAttack;
        explosivePower = _voidAttack;
        mining = _mining;
        currentPathNode.structureOnPath = this;
        transform.position = currentPathNode.transform.position;
        resetMovementRange();
        SetNodeColor();
        HPText = transform.Find("HP").GetComponent<TextMeshPro>();
        statText = transform.Find("HP/Stats").GetComponent<TextMeshPro>();
        selectIcon = transform.Find("Select").gameObject;
        inCombatIcon = transform.Find("InCombat").gameObject;
        unitImage = this.transform.Find("Unit");
        unitImage.rotation = Quaternion.Euler(transform.rotation.x, transform.rotation.y, 270 - (facing == Direction.TopRight ? -60 : (int)facing * 60));
        GameManager.i.AllUnits.Add(this);
    }
    public void RegenHP(int regen)
    {
        HP += regen;
        HP = Mathf.Min(maxHP, HP);
    }
    public void TakeDamage(int damage)
    {
        HP -= damage;
        HP = Mathf.Max(0, HP);
    }
    public void SetNodeColor()
    {
        currentPathNode.transform.Find("Node").GetComponent<SpriteRenderer>().material.color = GridManager.i.tileColors[stationId];
        currentPathNode.ownedById = stationId;
    }
    internal void resetMovementRange()
    {
        movement = maxRange;
    }

    internal void clearMovementRange()
    {
        movement = 0;
    }

    internal int getMovementRange()
    {
        return movement;
    }

    internal int getMaxMovementRange()
    {
        return maxRange;
    }

    internal void subtractMovement(int i)
    {
        movement -= i;
    }
    internal void ShowHPText(bool value)
    {
        HPText.text = $"{HP}";
        statText.text = $"{kineticPower}|{thermalPower}|{explosivePower}";
        HPText.gameObject.SetActive(value);
    }
    internal void EditModule(int id, int modifer = 1)
    {
        switch (id)
        {
            case 0:
                kineticPower += (3 * modifer);
                thermalArmor += (-1 * modifer);
                break;
            case 1:
                thermalPower += (3 * modifer);
                maxHP += (1 * modifer);
                HP += (1 * modifer);
                break;
            case 2:
                explosivePower += (3 * modifer);
                kineticArmor += (1 * modifer);
                break;
            case 3:
                maxRange += (1 * modifer);
                movement += (1 * modifer);
                kineticPower += (-1 * modifer);
                break;
            case 4:
                maxRange += (1 * modifer);
                movement += (1 * modifer);
                thermalPower += (-1 * modifer);
                break;
            case 5:
                maxRange += (1 * modifer);
                movement += (1 * modifer);
                explosivePower += (-1 * modifer);
                break;
            case 6:
                kineticPower += (2 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 7:
                kineticPower += (1 * modifer);
                thermalPower += (2 * modifer);
                break;
            case 8:
                kineticPower += (-1 * modifer);
                explosivePower += (4 * modifer);
                break;
            case 9:
                kineticPower += (3 * modifer);
                explosiveArmor += (-1 * modifer);
                break;
            case 10:
                explosivePower += (3 * modifer);
                kineticArmor += (1 * modifer);
                break;
            case 11:
                kineticPower += (1 * modifer);
                thermalPower += (1 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 12:
                thermalPower += (2 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 13:
                mining += (3 * modifer);
                kineticArmor += (2 * modifer);
                break;
            case 14:
                mining += (3 * modifer);
                thermalArmor += (2 * modifer);
                break;
            case 15:
                mining += (3 * modifer);
                explosiveArmor += (2 * modifer);
                break;
            case 16:
                kineticPower += (3 * modifer);
                thermalPower += (-2 * modifer);
                explosivePower += (2 * modifer);
                break;
            case 17:
                kineticPower += (2 * modifer);
                thermalPower += (3 * modifer);
                explosivePower += (-2 * modifer);
                break;
            case 18:
                kineticPower += (-2 * modifer);
                thermalPower += (4 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 19:
                thermalPower += (-2 * modifer);
                explosivePower += (4 * modifer);
                maxHP += (3 * modifer);
                HP += (3 * modifer);
                break;
            case 20:
                maxRange += (1 * modifer);
                movement += (1 * modifer);
                kineticArmor += (-2 * modifer); 
                break;
            case 21:
                maxRange += (1 * modifer);
                movement += (1 * modifer);
                thermalArmor += (-2 * modifer);
                break;
            case 22:
                maxRange += (1 * modifer);
                movement += (1 * modifer);
                explosiveArmor += (-2 * modifer);
                break;
            case 23:
                kineticPower += (1 * modifer);
                thermalArmor += (2 * modifer);
                explosiveArmor += (2 * modifer);
                break;
            case 24:
                thermalPower += (1 * modifer);
                explosiveArmor += (2 * modifer);
                kineticArmor += (2 * modifer);
                break;
            case 25:
                explosivePower += (1 * modifer);
                kineticArmor += (2 * modifer);
                thermalArmor += (2 * modifer);
                break;
            case 26:
                maxHP += (3 * modifer);
                HP += (3 * modifer);
                kineticArmor += (-2 * modifer);
                break;
            case 27:
                maxHP += (3 * modifer);
                HP += (3 * modifer);
                thermalArmor += (-2 * modifer);
                break;
            case 28:
                maxHP += (3 * modifer);
                HP += (3 * modifer);
                explosiveArmor += (-2 * modifer);
                break;
            case 29:
                maxHP += (3 * modifer);
                HP += (3 * modifer);
                kineticPower += (1 * modifer);
                break;
            case 30:
                maxHP += (3 * modifer);
                HP += (3 * modifer);
                thermalPower += (1 * modifer);
                break;
            case 31:
                maxHP += (3 * modifer);
                HP += (3 * modifer);
                explosivePower += (1 * modifer);
                break;  
            case 32:
                globalCreditGain += (1 * modifer);
                explosivePower += (-2 * modifer);
                break; 
            case 33:
                globalCreditGain += (2 * modifer);
                mining += (-2 * modifer);
                break; 
            case 34:
                globalCreditGain += (3 * modifer);
                movement += (-2 * modifer);
                break;
            case 35:
                mining += (3 * modifer);
                kineticPower += (1 * modifer);
                break; 
            case 36:
                mining += (3 * modifer);
                thermalPower += (1 * modifer);
                break; 
            case 37:
                mining += (3 * modifer);
                explosivePower += (1 * modifer);
                break;
            case 38:
                kineticPower += (5 * modifer);
                thermalPower += (-2 * modifer);
                explosiveArmor += (-4 * modifer);
                break;
            case 39:
                thermalPower += (5 * modifer);
                explosivePower += (-2 * modifer);
                kineticArmor += (-3 * modifer);
                break;
            case 40:
                kineticPower += (-2 * modifer);
                explosivePower += (5 * modifer);
                thermalArmor += (-2 * modifer);
                break;
            default:
                break;
        }
    }

    internal ServerUnit ToServerUnit()
    {
        return new ServerUnit()
        {
            UnitGuid = unitGuid,
            Facing = facing,
            Location = location.ToServerCoords()
        };
    }
}