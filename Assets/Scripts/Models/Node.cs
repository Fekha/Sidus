using UnityEngine;

public class Node : MonoBehaviour
{
    public PathNode currentPathNode {
        get { return GridManager.i.grid[location.x, location.y]; }
        set
        {
            location.x = value.actualCoords.x;
            location.y = value.actualCoords.y;
            GridManager.i.grid[location.x, location.y] = value;
        }
    }
    public Coords location;
    internal void Initialize(PathNode node)
    {
        location = node.actualCoords;
    }
}