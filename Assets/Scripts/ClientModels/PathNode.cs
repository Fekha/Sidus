using TMPro;
using UnityEngine;
using static GameManager;

public class PathNode : MonoBehaviour
{
    public bool isAsteroid;
    internal int maxCredits = 0;
    internal int currentCredits = 0;
    internal int creditsRegin = 0;
    internal bool hasBeenMinedThisTurn = false;
    public TextMeshPro mineralText;
    public int gCost;
    public int hCost;
    public int x;
    public int y;
    public int fCost { get { return gCost + hCost; } }
    public bool isEvenCol { get { return y % 2 == 0; } }
    public Unit structureOnPath;
    public PathNode parent;

    public int ownedById;
    public void InitializeNode(int _x, int _y, bool _isAsteroid, int _startCredits, int _maxCredits, int _creditRegin)
    {
        maxCredits = _maxCredits;
        currentCredits = _startCredits;
        creditsRegin = _creditRegin;
        isAsteroid = _isAsteroid;
        if (isAsteroid)
        {
            mineralText = transform.Find("Minerals").GetComponent<TextMeshPro>();
            mineralText.text = $"{currentCredits}";
        }
        ownedById = -1;
        x = _x;
        y = _y;
        
    }

    public void ReginCredits()
    {
        if (currentCredits < maxCredits && !hasBeenMinedThisTurn)
        {
            currentCredits = Mathf.Min(currentCredits + creditsRegin, maxCredits);
            mineralText.text = $"{currentCredits}";
        }
        hasBeenMinedThisTurn = false;
    }

    public int MineCredits(int mineValue)
    {
        var startingCredits = currentCredits;
        int amountMined = (currentCredits - mineValue) >= 0 ? mineValue : startingCredits;
        currentCredits -= amountMined;
        mineralText.text = $"{currentCredits}";
        hasBeenMinedThisTurn = true;
        return amountMined;
    }
}