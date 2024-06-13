using StartaneousAPI.ServerModels;
using System;
using System.Collections;
using TMPro;
using UnityEngine;

public class PathNode : MonoBehaviour
{
    public Unit unitOnPath;
    public bool isRift = false;
    public int maxCredits = 0;
    public int minerals = 0;
    public int creditRegin = 0;
    public Coords coords;
    public int ownedById = -1;

    internal int gCost;
    internal PathNode parent;
    internal bool hasBeenMinedThisTurn = false;
    private TextMeshPro mineralText;
    public TextMeshPro coordsText;
    private SpriteRenderer asteriodSprite;
    private GameObject mineIcon;
    private Coords[] evenOffsets = { new Coords(1, 0), new Coords(1, -1), new Coords(0, -1), new Coords(-1, 0), new Coords(0, 1), new Coords(1, 1) }; // Clockwise from right
    private Coords[] oddOffsets = { new Coords(1, 0), new Coords(0, -1), new Coords(-1, -1), new Coords(-1, 0), new Coords(-1, 1), new Coords(0, 1) }; // Clockwise from right
    public bool isEvenCol { get { return coords.y % 2 == 0; } }
    public Coords[] offSet { get { return isEvenCol ? evenOffsets : oddOffsets; } }

    public bool isAsteroid { get { return minerals > 0; } }

    public void InitializeNode(int _x, int _y, int _minerals, int _maxCredits, int _creditRegin, bool _isRift)
    {
        isRift = _isRift;
        maxCredits = _maxCredits;
        minerals = _minerals;
        creditRegin = _creditRegin;
        coords = new Coords(_x, _y);
        GetUIComponents();
    }

    public void InitializeNode(ServerNode node)
    {
        isRift = node.IsRift;
        maxCredits = node.MaxCredits;
        minerals = node.Minerals;
        creditRegin = node.CreditRegin;
        coords = new Coords(node.Coords);
        ownedById = node.OwnedById;
        GetUIComponents();
    }

    private void GetUIComponents()
    {
        asteriodSprite = transform.Find("Asteroid").GetComponent<SpriteRenderer>();
        asteriodSprite.gameObject.SetActive(isAsteroid);
        mineIcon = transform.Find("Asteroid/Mine").gameObject;
        mineralText = transform.Find("Asteroid/Minerals").GetComponent<TextMeshPro>();
        coordsText = transform.Find("Coords").GetComponent<TextMeshPro>();
        coordsText.text = $"{coords.x},{coords.y}";
        SetNodeColor(ownedById);
        //coordsText.gameObject.SetActive(true); //Helpful for debugging
        GridManager.i.AllNodes.Add(this);
    }

    public ServerNode ToServerNode()
    {
        return new ServerNode()
        {
            IsRift = isRift,
            MaxCredits = maxCredits,
            Minerals = minerals,
            CreditRegin = creditRegin,
            Coords = coords.ToServerCoords(),
            OwnedById = ownedById,
        };
    }
    public void ReginCredits()
    {
        if (minerals < maxCredits && !hasBeenMinedThisTurn && isAsteroid)
        {
            minerals = Mathf.Min(minerals + creditRegin, maxCredits);
            mineralText.text = $"{minerals}";
            //StartCoroutine(GameManager.i.FloatingTextAnimation($"+{creditsRegin} Credit", transform, null));
        }
        hasBeenMinedThisTurn = false;
    }
    private IEnumerator MineAnimation()
    {
        ShowMineIcon(true);
        yield return new WaitForSeconds(1f);
        ShowMineIcon(false);
    }
    public int MineCredits(Unit unit, bool isQueuing)
    {
        var startingCredits = minerals;
        int minedAmount = (minerals - unit.miningLeft) >= 0 ? unit.miningLeft : startingCredits;
        if (!isQueuing && minedAmount > 0)
        {
            StartCoroutine(MineAnimation());
            hasBeenMinedThisTurn = true;
        }
        AwardCredits(unit, minedAmount, isQueuing);
        return minedAmount;
    }
    public void SetNodeColor(int _ownedById)
    {
        transform.Find("Node").GetComponent<SpriteRenderer>().material.color = GridManager.i.tileColors[_ownedById];
        ownedById = _ownedById;
    }
    internal void AwardCredits(Unit unit, int minedAmount, bool isQueuing)
    {
        minerals -= minedAmount;
        unit.miningLeft -= minedAmount;
        GameManager.i.Stations[unit.stationId].GainCredits(minedAmount, unit, isQueuing, false);
        mineralText.text = $"{minerals}";
        asteriodSprite.gameObject.SetActive(isAsteroid);
    }

    internal void ShowMineIcon(bool active)
    {
        mineIcon.SetActive(active);
    } 
    internal void ShowMineralText(bool active)
    {
        if (isAsteroid)
        {
            mineralText.text = $"{minerals}";
            mineralText.gameObject.SetActive(active);
        }
        else
        {
            mineralText.gameObject.SetActive(false);
        }
    }
}