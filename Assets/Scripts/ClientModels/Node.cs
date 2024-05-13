using UnityEngine;

public class Node : MonoBehaviour
{
    public PathNode currentPathNode {
        get { return GridManager.i.grid[coords.x, coords.y]; }
        set
        {
            coords.x = value.coords.x;
            coords.y = value.coords.y;
            GridManager.i.grid[coords.x, coords.y] = value;
        }
    }
    public Coords coords;
    internal void Initialize(PathNode node)
    {
        coords = node.coords;
    }
}