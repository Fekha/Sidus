using UnityEngine;
using static GameManager;

public class PathNode : MonoBehaviour
{
    public bool isObstacle;
    public int gCost;
    public int hCost;
    public int x;
    public int y;
    public int fCost { get { return gCost + hCost; } }
    public Structure structureOnPath;
    public PathNode parent;
    public int ownedById;
    public void InitializeNode(int _x, int _y, bool obstacle)
    {
        isObstacle = obstacle;
        ownedById = -1;
        x = _x;
        y = _y;
    }
}