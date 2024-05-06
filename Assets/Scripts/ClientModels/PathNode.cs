using UnityEngine;
using static GameManager;

public class PathNode : MonoBehaviour
{
    public bool isAsteroid;
    public int maxCredits = 0;
    public int currentCredits = 0;
    public int creditsRegin = 0;
    
    public int gCost;
    public int hCost;
    public int x;
    public int y;
    public int fCost { get { return gCost + hCost; } }
    public bool isEvenCol { get { return y % 2 == 0; } }
    public Unit structureOnPath;
    public PathNode parent;

    public int ownedById;
    public void InitializeNode(int _x, int _y, bool _isAsteroid, int _maxCredits, int _creditRegin)
    {
        isAsteroid = _isAsteroid;
        ownedById = -1;
        x = _x;
        y = _y;
        maxCredits = _maxCredits;
        currentCredits = _maxCredits;
        creditsRegin = _creditRegin;
    }

    public void ReginCredits()
    {
        if (currentCredits < maxCredits)
        {
            currentCredits = Mathf.Min(currentCredits + creditsRegin, maxCredits);
        }
    }

    public int MineCredits(int mineValue)
    {
        var totalCredits = currentCredits;
        var leftover = currentCredits - mineValue;
        if (leftover >= 0)
        {
            currentCredits = leftover;
            return mineValue;
        }
        else
        {
            currentCredits = 0;
            return totalCredits;
        }
    }
}