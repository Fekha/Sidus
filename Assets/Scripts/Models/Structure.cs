using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class Structure : Node
{
    internal string structureName;
    internal string color;
    internal Guid structureId;
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
    internal List<PathNode> path;
    internal List<Module> attachedModules = new List<Module>();
    internal int maxAttachedModules = 0; // 1+ station.level
    public void InitializeStructure(int _x, int _y, string _structureName, string _color, int _hp, int _range, int _shield, int _electricAttack, int _thermalAttack, int _voidAttack, int _level)
    {
        structureId = Guid.NewGuid();
        x = _x;
        y = _y;
        color = _color;
        maxHp = _hp;
        hp = _hp;
        maxRange = _range;
        range = _range;
        level = _level;
        shield = (AttackType)_shield;
        electricAttack = _electricAttack;
        thermalAttack = _thermalAttack;
        voidAttack = _voidAttack;
        structureName = _structureName;
        currentPathNode.nodeOnPath = this;
        transform.position = currentPathNode.transform.position;
        resetMovementRange();
        SetNodeColor();
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
    }
    public void SetNodeColor()
    {
        currentPathNode.transform.Find("Node").GetComponent<SpriteRenderer>().material.color = stationId == 0 ? new Color(1, 0.7421383f, 0.7421383f, 1) : new Color(0.8731644f, 1, 0.572327f, 1);
        currentPathNode.ownedById = stationId;
    }
    internal void resetMovementRange()
    {
        range = maxRange;
        path = null;
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
}