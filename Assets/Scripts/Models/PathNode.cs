using System;
using TMPro;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public bool isAsteroid { get { return currentCredits > 0; } }
    internal int maxCredits = 0;
    internal int currentCredits = 0;
    internal int creditsRegin = 0;
    internal bool hasBeenMinedThisTurn = false;
    private TextMeshPro mineralText;
    public TextMeshPro coordsText;
    private SpriteRenderer asteriodSprite;
    private GameObject mineIcon;
    public int gCost;
    public int hCost;
    public Coords coords;
    public int fCost { get { return gCost + hCost; } }
    private Coords[] evenOffsets = { new Coords(1, 0), new Coords(1, -1), new Coords(0, -1), new Coords(-1, 0), new Coords(0, 1), new Coords(1, 1) }; // Clockwise from right
    private Coords[] oddOffsets = { new Coords(1, 0), new Coords(0, -1), new Coords(-1, -1), new Coords(-1, 0), new Coords(-1, 1), new Coords(0, 1) }; // Clockwise from right
    public bool isEvenCol { get { return coords.y % 2 == 0; } }
    public Coords[] offSet { get { return isEvenCol ? evenOffsets : oddOffsets; } }

    public Unit structureOnPath;
    public PathNode parent;

    public int ownedById;
    public void InitializeNode(int _x, int _y, int _startCredits, int _maxCredits, int _creditRegin)
    {
        maxCredits = _maxCredits;
        currentCredits = _startCredits;
        creditsRegin = _creditRegin;
        asteriodSprite = transform.Find("Asteroid").GetComponent<SpriteRenderer>();
        asteriodSprite.gameObject.SetActive(isAsteroid);
        mineIcon = transform.Find("Asteroid/Mine").gameObject;
        mineralText = transform.Find("Asteroid/Minerals").GetComponent<TextMeshPro>();
        ownedById = -1;
        coords = new Coords(_x, _y);
        coordsText = transform.Find("Coords").GetComponent<TextMeshPro>();
        coordsText.text = $"{coords.x},{coords.y}";
//#if UNITY_EDITOR
//        coordsText.gameObject.SetActive(true);
//#endif
        GridManager.i.AllNodes.Add(this);
    }

    public void ReginCredits()
    {
        if (currentCredits < maxCredits && !hasBeenMinedThisTurn && isAsteroid)
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
        asteriodSprite.gameObject.SetActive(isAsteroid);
        return amountMined;
    }

    internal void ShowMineIcon(bool active)
    {
        mineIcon.SetActive(active);
    } 
    internal void ShowMineralText(bool active)
    {
        if (isAsteroid)
        {
            mineralText.text = $"{currentCredits}";
            mineralText.gameObject.SetActive(active);
        }
        else
        {
            mineralText.gameObject.SetActive(false);
        }
    }
}