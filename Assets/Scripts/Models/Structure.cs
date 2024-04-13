using TMPro;
using UnityEngine;

public class Structure : Node
{
    internal int stationId;
    internal int maxHp;
    internal int hp;
    public void InitializeStructure(int _x, int _y, int _hp)
    {
        x = _x;
        y = _y;
        maxHp = _hp;
        hp = _hp;
        SetHPText();
        currentPathNode.nodeOnPath = this;
        transform.position = currentPathNode.transform.position;
        SetNodeColor();
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
    public void SetNodeColor()
    {
        currentPathNode.transform.Find("Node").GetComponent<SpriteRenderer>().material.color = stationId == 0 ? new Color(1, 0.7421383f, 0.7421383f, 1) : new Color(0.8731644f, 1, 0.572327f, 1);
    }
}