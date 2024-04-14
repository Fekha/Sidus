using TMPro;
using UnityEngine;

public class Structure : Node
{
    internal string structureName;
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
    public void InitializeStructure(int _x, int _y, string _structureName, int _hp, int _range, int _shield, int _electricAttack, int _thermalAttack, int _voidAttack, int _level)
    {
        x = _x;
        y = _y;
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
        SetNodeColor();
    }

    public void TakeDamage(int damage)
    {
        hp -= damage;
    }
    public void SetNodeColor()
    {
        currentPathNode.transform.Find("Node").GetComponent<SpriteRenderer>().material.color = stationId == 0 ? new Color(1, 0.7421383f, 0.7421383f, 1) : new Color(0.8731644f, 1, 0.572327f, 1);
    }
}