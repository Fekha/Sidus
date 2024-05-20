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
    internal int range;
    internal int kineticPower;
    internal int thermalPower;
    internal int explosivePower;
    internal int kineticArmor;
    internal int thermalArmor;
    internal int explosiveArmor;
    internal int mining;
    internal int level;
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
    public void InitializeStructure(int _x, int _y, string _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, int _level, Guid _unitGuid, int _mining, Direction _direction)
    {
        facing = _direction;
        unitGuid = _unitGuid;
        location = new Coords(_x, _y);
        color = _color;
        maxHP = _hp;
        HP = _hp;
        maxRange = _range;
        range = _range;
        level = _level;
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
        range = maxRange;
    }

    internal void clearMovementRange()
    {
        range = 0;
    }

    internal int getMovementRange()
    {
        return range;
    }

    internal int getMaxMovementRange()
    {
        return maxRange;
    }

    internal void subtractMovement(int i)
    {
        range -= i;
    }

    internal void EditModule(int id, int modifer = 1)
    {
        switch (id)
        {
            case 0:
                explosivePower += (2 * modifer);
                thermalArmor += (2 * modifer);
                break;
            case 1:
                thermalPower += (2 * modifer);
                explosiveArmor += (2 * modifer);
                break;
            case 2:
                explosivePower += (3 * modifer);
                kineticArmor += (-1 * modifer);
                break;
            case 3:
                maxRange += (1 * modifer);
                range += (1 * modifer);
                explosivePower += (-1 * modifer);
                break;
            case 4:
                mining += (3 * modifer);
                explosiveArmor += (1 * modifer);
                break;
            case 5:
                thermalPower += (1 * modifer);
                explosivePower += (2 * modifer);
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
                explosivePower += (4 * modifer);
                kineticPower += (1 * modifer);
                break;
            case 9:
                kineticPower += (3 * modifer);
                thermalArmor += (-1 * modifer);
                break;
            case 10:
                thermalPower += (3 * modifer);
                break;
            case 11:
                kineticPower += (1 * modifer);
                explosivePower += (2 * modifer);
                break;
            case 12:
                thermalPower += (1 * modifer);
                explosivePower += (2 * modifer);
                break;
            case 13:
                explosivePower += (3 * modifer);
                thermalArmor += (-1 * modifer);
                break;
            case 14:
                maxRange += (1 * modifer);
                range += (1 * modifer);
                kineticPower += (-1 * modifer);
                break;
            case 15:
                mining += (3 * modifer);
                kineticArmor += (1 * modifer);
                break;
            case 16:
                kineticPower += (3 * modifer);
                explosivePower += (2 * modifer);
                thermalPower += (-2 * modifer);
                break;
            case 17:
                kineticPower += (3 * modifer);
                thermalPower += (2 * modifer);
                explosivePower += (-2 * modifer);
                break;
            case 18:
                thermalPower += (2 * modifer);
                explosivePower += (3 * modifer);
                kineticPower += (-2 * modifer); 
                break;
            case 19:
                explosivePower += (5 * modifer);
                thermalPower += (-2 * modifer);
                kineticArmor += (-1 * modifer);
                break;
            case 20:
                maxRange += (1 * modifer);
                range += (1 * modifer);
                kineticArmor += (-2 * modifer); 
                break;
            case 21:
                maxRange += (1 * modifer);
                range += (1 * modifer);
                explosiveArmor += (-2 * modifer);
                break;
            case 22:
                maxRange += (1 * modifer);
                range += (1 * modifer);
                thermalArmor += (-2 * modifer);
                break;
            case 23:
                explosivePower += (1 * modifer);
                thermalArmor += (2 * modifer);
                kineticArmor += (2 * modifer);
                break;
            case 24:
                kineticPower += (1 * modifer);
                thermalArmor += (2 * modifer);
                explosiveArmor += (2 * modifer);
                break;
            case 25:
                thermalPower += (1 * modifer);
                explosiveArmor += (2 * modifer);
                kineticArmor += (2 * modifer);
                break;
            default:
                break;
        }
    }

    internal void ShowHPText(bool value)
    {
        HPText.text = $"{HP}";
        statText.text = $"{kineticPower}|{thermalPower}|{explosivePower}";
        HPText.gameObject.SetActive(value);
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