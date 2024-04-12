using UnityEngine;
using static GameManager;

public class PathNode : MonoBehaviour
{
    public bool isObstacle;
    public Node OnPathNode;
    public int x;
    public int y;
    public int gCost;
    public int hCost;
    public bool isTaken { get { return isObstacle || OnPathNode != null; } }
    public int fCost { get { return gCost + hCost; } }
    public PathNode parent;
    public void InitializeNode(int x, int y, bool obstacle)
    {
        isObstacle = obstacle;
        this.x = x;
        this.y = y;
    }
}