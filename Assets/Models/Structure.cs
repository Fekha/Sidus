using TMPro;
using UnityEngine;

public class Structure : Node
{
    internal int maxHp;
    internal int hp;
    public void InitializeStructure(int _x, int _y, int _hp)
    {
        x = _x;
        y = _y;
        maxHp = _hp;
        hp = _hp;
        SetHPText();
        currentNode.OnPathNode = this;
        transform.position = currentNode.transform.position;
    }
    public void SetHPText()
    {
        //hpBar.text = hp + "/" + maxHp; 
    }
    public void TakeDamage(int damage)
    {
        hp -= damage;
        SetHPText();
    }
}