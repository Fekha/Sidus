using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Unit : Node
{
    internal string unitName;
    internal string color;
    internal Guid unitGuid;
    internal int stationId;
    internal int maxHp;
    internal int hp;
    internal int maxRange;
    internal int range;
    internal int kineticAttack;
    internal int thermalAttack;
    internal int explosiveAttack;
    internal int kineticArmor;
    internal int thermalArmor;
    internal int explosiveArmor;
    internal int mining;
    internal int level;
    //internal bool hasMinedThisTurn = false;
    internal List<Module> attachedModules = new List<Module>();
    internal TextMeshPro hpText;
    internal GameObject selectIcon;
    internal GameObject inCombatIcon;
    internal int maxAttachedModules = 1; // 1+ station.level
    public void InitializeStructure(int _x, int _y, string _structureName, string _color, int _hp, int _range, int _electricAttack, int _thermalAttack, int _voidAttack, int _level, Guid _unitGuid, int _mining)
    {
        unitGuid = _unitGuid;
        x = _x;
        y = _y;
        color = _color;
        maxHp = _hp;
        hp = _hp;
        maxRange = _range;
        range = _range;
        level = _level;
        kineticAttack = _electricAttack;
        thermalAttack = _thermalAttack;
        explosiveAttack = _voidAttack;
        unitName = _structureName;
        mining = _mining;
        currentPathNode.structureOnPath = this;
        transform.position = currentPathNode.transform.position;
        resetMovementRange();
        SetNodeColor();
        hpText = transform.Find("HP").GetComponent<TextMeshPro>();
        selectIcon = transform.Find("Select").gameObject;
        inCombatIcon = transform.Find("InCombat").gameObject;
        GameManager.i.AllUnits.Add(this);
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
                kineticAttack += (2 * modifer);
                explosiveAttack += (1 * modifer);
                thermalArmor += (1 * modifer);
                break;
            case 1:
                thermalAttack += (2 * modifer);
                kineticAttack += (1 * modifer);
                explosiveArmor += (1 * modifer);
                break;
            case 2:
                explosiveAttack += (3 * modifer);
                kineticArmor += (1 * modifer);
                break;
            case 3:
                maxRange += (1 * modifer);
                range += (1 * modifer);
                explosiveArmor += (-1 * modifer);
                break;
            case 4:
                mining += (3 * modifer);
                explosiveArmor += (-1 * modifer);
                break;
            case 5:
                kineticAttack += (1 * modifer);
                thermalAttack += (1 * modifer);
                explosiveAttack += (2 * modifer);
                break;
            case 6:
                kineticAttack += (2 * modifer);
                thermalAttack += (1 * modifer); 
                explosiveAttack += (1 * modifer);
                break;
            case 7:
                kineticAttack += (1 * modifer);
                thermalAttack += (2 * modifer); 
                explosiveAttack += (1 * modifer);
                break;
            case 8:
                explosiveAttack += (2 * modifer);
                thermalArmor += (2 * modifer);
                break;
            case 9:
                kineticAttack += (2 * modifer);
                kineticArmor += (2 * modifer);
                break;
            case 10:
                thermalAttack += (2 * modifer);
                explosiveArmor += (2 * modifer);
                break;
            case 11:
                kineticAttack += (2 * modifer);
                explosiveAttack += (2 * modifer);
                break;
            case 12:
                thermalAttack += (2 * modifer);
                explosiveAttack += (2 * modifer);
                break;
            case 13:
                explosiveAttack += (4 * modifer);
                break;
            case 14:
                maxRange += (1 * modifer);
                range += (1 * modifer);
                kineticArmor += (-1 * modifer);
                break;
            case 15:
                mining += (3 * modifer);
                kineticArmor += (-1 * modifer);
                break;
            case 16:
                thermalAttack += (2 * modifer);
                explosiveAttack += (3 * modifer);
                kineticArmor += (-2 * modifer);
                break;
            case 17:
                kineticAttack += (2 * modifer);
                thermalAttack += (3 * modifer);
                explosiveArmor += (-2 * modifer);
                break;
            case 18:
                thermalAttack += (3 * modifer);
                explosiveAttack += (2 * modifer);
                kineticArmor += (-2 * modifer); 
                break;
            case 19:
                explosiveAttack += (5 * modifer);
                kineticArmor += (-2 * modifer); 
                break;
            case 20:
                kineticAttack += (1 * modifer);
                explosiveArmor += (2 * modifer);
                kineticArmor += (1 * modifer); 
                break;
            case 21:
                thermalAttack += (1 * modifer);
                kineticArmor += (2 * modifer); 
                explosiveArmor += (1 * modifer);
                break;
            case 22:
                explosiveAttack += (1 * modifer);
                kineticArmor += (2 * modifer); 
                thermalArmor += (1 * modifer);
                break;
            default:
                break;
        }
    }

    internal void ShowHPText(bool value)
    {
        hpText.text = $"{hp}";
        hpText.gameObject.SetActive(value);
    }
}