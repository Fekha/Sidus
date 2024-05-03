using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Structure : Node
{
    internal string structureName;
    internal string color;
    internal Guid structureGuid;
    internal int stationId;
    internal int maxHp;
    internal int hp;
    internal int maxRange;
    internal int range;
    internal AttackType shield;
    internal int electricAttack;
    internal int thermalAttack;
    internal int voidAttack;
    internal int level;
    internal List<Module> attachedModules = new List<Module>();
    internal int maxAttachedModules = 1; // 1+ station.level
    public void InitializeStructure(int _x, int _y, string _structureName, string _color, int _hp, int _range, AttackType _shield, int _electricAttack, int _thermalAttack, int _voidAttack, int _level, Guid _structureGuid)
    {
        structureGuid = _structureGuid;
        x = _x;
        y = _y;
        color = _color;
        maxHp = _hp;
        hp = _hp;
        maxRange = _range;
        range = _range;
        level = _level;
        shield = _shield;
        electricAttack = _electricAttack;
        thermalAttack = _thermalAttack;
        voidAttack = _voidAttack;
        structureName = _structureName;
        currentPathNode.structureOnPath = this;
        transform.position = currentPathNode.transform.position;
        resetMovementRange();
        SetNodeColor();
        GameManager.i.AllStructures.Add(this);
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
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
                electricAttack += (3 * modifer);
                voidAttack += (1 * modifer);
                if (modifer == 1) shield = AttackType.Thermal; 
                break;
            case 1:
                thermalAttack += (3 * modifer);
                electricAttack += (1 * modifer);
                if (modifer == 1) shield = AttackType.Electric;
                break;
            case 2:
                voidAttack += (4 * modifer);
                if (modifer == 1) shield = AttackType.Void;
                break;
            case 3:
                maxRange += (1 * modifer);
                range += (1 * modifer);
                shield = AttackType.Void;
                break;
            case 4:
                maxHp += (5 * modifer);
                hp += (5 * modifer);
                if (modifer == 1) shield = AttackType.Void;
                break;
            case 5:
                electricAttack += (1 * modifer);
                thermalAttack += (2 * modifer);
                voidAttack += (2 * modifer);
                if (modifer == 1) shield = AttackType.Electric;
                break;
            case 6:
                electricAttack += (2 * modifer);
                thermalAttack += (1 * modifer); 
                voidAttack += (2 * modifer);
                if (modifer == 1) shield = AttackType.Thermal;
                break;
            case 7:
                electricAttack += (2 * modifer);
                thermalAttack += (2 * modifer); 
                voidAttack += (2 * modifer);
                if (modifer == 1) shield = AttackType.Thermal;
                break;
            case 8:
                electricAttack += (1 * modifer);
                thermalAttack += (1 * modifer);
                voidAttack += (3 * modifer);
                if (modifer == 1) shield = AttackType.Thermal;
                break;
            case 9:
                electricAttack += (2 * modifer);
                thermalAttack += (1 * modifer);
                voidAttack += (2 * modifer);
                if (modifer == 1) shield = AttackType.Electric;
                break;
            case 10:
                electricAttack += (1 * modifer);
                thermalAttack += (3 * modifer);
                voidAttack += (2 * modifer);
                break;
            case 11:
                electricAttack += (3 * modifer);
                voidAttack += (2 * modifer);
                break;
            case 12:
                thermalAttack += (3 * modifer);
                voidAttack += (2 * modifer);
                break;
            case 13:
                voidAttack += (4 * modifer);
                break;
            case 14:
                maxRange += (1 * modifer);
                range += (1 * modifer);
                break;
            case 15:
                maxHp += (6 * modifer);
                hp += (6 * modifer);
                break;
            case 16:
                thermalAttack += (2 * modifer);
                voidAttack += (3 * modifer);
                break;
            case 17:
                electricAttack += (3 * modifer);
                thermalAttack += (1 * modifer);
                voidAttack += (1 * modifer);
                break;
            case 18:
                electricAttack += (1 * modifer);
                thermalAttack += (3 * modifer);
                voidAttack += (1 * modifer);
                break;
            case 19:
                electricAttack += (1 * modifer);
                thermalAttack += (1 * modifer);
                voidAttack += (3 * modifer);
                break;
            default:
                break;
        }
    }
}