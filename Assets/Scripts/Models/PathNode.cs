using UnityEngine;
using static GameManager;

public class PathNode : MonoBehaviour
{
    public bool isObstacle;
    public int x;
    public int y;
    public int gCost;
    public int hCost;
    public int fCost { get { return gCost + hCost; } }
    public Node nodeOnPath;
    public PathNode parent;
    public int ownedById;
    public void InitializeNode(int x, int y, bool obstacle)
    {
        isObstacle = obstacle;
        this.x = x;
        this.y = y;
        ownedById = -1;
    }
}