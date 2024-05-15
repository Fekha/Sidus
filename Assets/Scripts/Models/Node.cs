using UnityEngine;

public class Node : MonoBehaviour
{
    public PathNode currentPathNode {
        get { return GridManager.i.grid[location.x, location.y]; }
        set
        {
            location.x = value.coords.x;
            location.y = value.coords.y;
            GridManager.i.grid[location.x, location.y] = value;
        }
    }
    public Coords location;
    internal void Initialize(PathNode node)
    {
        location = node.coords;
    }
}