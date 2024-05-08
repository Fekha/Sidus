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
    public TextMeshPro coordsText;
    public int gCost;
    public int hCost;
    public int x;
    public int y;
    public int fCost { get { return gCost + hCost; } }
    private Coords[] evenOffsets = { new Coords(1, 0), new Coords(1, -1), new Coords(0, -1), new Coords(-1, 0), new Coords(0, 1), new Coords(1, 1) }; // Clockwise from right
    private Coords[] oddOffsets = { new Coords(1, 0), new Coords(0, -1), new Coords(-1, -1), new Coords(-1, 0), new Coords(-1, 1), new Coords(0, 1) }; // Clockwise from right
    public bool isEvenCol { get { return y % 2 == 0; } }
    public Coords[] offSet { get { return isEvenCol ? evenOffsets : oddOffsets; } }

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
        coordsText = transform.Find("Coords").GetComponent<TextMeshPro>();
        coordsText.text = $"{x},{y}";
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