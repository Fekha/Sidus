using UnityEngine;

public class Node : MonoBehaviour
{
    public PathNode currentNode {
        get { return GameManager.i.grid[x, y]; }
        set
        {
            x = value.x;
            y = value.y;
            GameManager.i.grid[x, y] = value;
        }
    }
    internal int x;
    internal int y;
    internal void Initialize(PathNode node)
    {
        x = node.x;
        y = node.y;
    }
}