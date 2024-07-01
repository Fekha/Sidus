using Models;
using System;
using System.Collections;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UIElements;

public class PathNode : MonoBehaviour
{
    public Unit unitOnPath;
    public bool isRift = false;
    public int maxCredits = 0;
    public int minerals = 0;
    public int creditRegin = 0;
    public Coords coords;
    public Guid ownedByGuid;

    internal int gCost;
    internal PathNode parent;
    internal bool hasBeenMinedThisTurn = false;
    private TextMeshPro mineralText;
    public TextMeshPro coordsText;
    private SpriteRenderer asteriodSprite;
    private SpriteRenderer minedAsteriodSprite;
    private GameObject mineIcon;
    private Coords[] evenOffsets = { new Coords(1, 0), new Coords(1, -1), new Coords(0, -1), new Coords(-1, 0), new Coords(0, 1), new Coords(1, 1) }; // Clockwise from right
    private Coords[] oddOffsets = { new Coords(1, 0), new Coords(0, -1), new Coords(-1, -1), new Coords(-1, 0), new Coords(-1, 1), new Coords(0, 1) }; // Clockwise from right
    public bool isEvenCol { get { return coords.y % 2 == 0; } }
    public Coords[] offSet { get { return isEvenCol ? evenOffsets : oddOffsets; } }

    public bool isAsteroid { get { return minerals > 0; } }
    private bool isDragging = false;
    private Vector3 offset;
    private Transform parentTransform;
    private Vector3 originalPosition;

    void Start()
    {
        parentTransform = transform.parent;
        originalPosition = transform.parent.position;
    }

    void OnMouseDown()
    {
        // Calculate the offset between the mouse position and the GameObject position
        Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        mousePosition.z = parentTransform.position.z; // Keep the Z position constant
        offset = parentTransform.position - mousePosition;
        //isDragging = true; //Uncomment out when ready to drag
    }

    void OnMouseDrag()
    {
        if (isDragging)
        {
            // Get the current mouse position
            Vector3 mousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            mousePosition.z = parentTransform.position.z; // Keep the Z position constant
            // Set the new position of the parent GameObject
            parentTransform.position = mousePosition + offset;
        }
    }

    void OnMouseUp()
    {
        isDragging = false;
        parentTransform.position = originalPosition;
    }
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
        coords = new Coords(node.X,node.Y);
        ownedByGuid = node.OwnedByGuid;
        GetUIComponents();
    }

    private void GetUIComponents()
    {
        minedAsteriodSprite = transform.Find("MinedAsteroid").GetComponent<SpriteRenderer>();
        minedAsteriodSprite.gameObject.SetActive(false);
        asteriodSprite = transform.Find("Asteroid").GetComponent<SpriteRenderer>();
        asteriodSprite.gameObject.SetActive(isAsteroid);
        mineIcon = transform.Find("Asteroid/Mine").gameObject;
        mineralText = transform.Find("Asteroid/Minerals").GetComponent<TextMeshPro>();
        coordsText = transform.Find("Coords").GetComponent<TextMeshPro>();
        coordsText.text = $"{coords.x},{coords.y}";
        SetNodeColor(ownedByGuid);
        //coordsText.gameObject.SetActive(true); //Helpful for debugging
        GridManager.i.AllNodes.Add(this);
    }

    public ServerNode ToServerNode()
    {
        return new ServerNode()
        {
            GameGuid = Globals.GameMatch.GameGuid,
            TurnNumber = GameManager.i.TurnNumber,
            IsRift = isRift,
            MaxCredits = maxCredits,
            Minerals = minerals,
            CreditRegin = creditRegin,
            X = coords.x,
            Y = coords.y,
            OwnedByGuid = ownedByGuid,
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
    public void SetNodeColor(Guid _ownedByGuid)
    {
        ownedByGuid = _ownedByGuid;
        if (ownedByGuid != Guid.Empty)
        {
            int? playerColor = Globals.GameMatch.GameTurns.FirstOrDefault().Players.FirstOrDefault(x => x.PlayerGuid == ownedByGuid)?.PlayerColor;
            //For Practice Games
            if (playerColor == null) {
                playerColor = (int)GameManager.i.GetStationByGuid(ownedByGuid).playerColor;
            }
            transform.Find("Node").GetComponent<SpriteRenderer>().material.color = GridManager.i.tileColors[(int)playerColor];
        }
    }
    internal void AwardCredits(Unit unit, int minedAmount, bool isQueuing)
    {
        minerals -= minedAmount;
        unit.miningLeft -= minedAmount;
        GameManager.i.GetStationByGuid(unit.playerGuid).GainCredits(minedAmount, unit, isQueuing, false);
        mineralText.text = $"{minerals}";
        asteriodSprite.gameObject.SetActive(isAsteroid);
        minedAsteriodSprite.gameObject.SetActive(!isAsteroid && isQueuing);
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