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
    public Coords actualCoords;
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
    public bool isEvenCol { get { return actualCoords.y % 2 == 0; } }
    public Coords[] offSet { get { return isEvenCol ? evenOffsets : oddOffsets; } }

    public bool isAsteroid { get { return minerals > 0; } }
    private bool canDrag = false;
    private bool isDragging = false;
    private Vector3 offset;
    private Transform parentTransform;
    private Vector3 initialMousePosition;

    public float dragThreshold = 0.5f;
    void Start()
    {
        parentTransform = transform.parent;
    }
    public void InitializeNode(int _x, int _y, int _minerals, int _maxCredits, int _creditRegin, bool _isRift)
    {
        isRift = _isRift;
        maxCredits = _maxCredits;
        minerals = _minerals;
        creditRegin = _creditRegin;
        actualCoords = new Coords(_x, _y);
        GetUIComponents();
    }

    public void InitializeNode(ServerNode node)
    {
        isRift = node.IsRift;
        maxCredits = node.MaxCredits;
        minerals = node.Minerals;
        creditRegin = node.CreditRegin;
        actualCoords = new Coords(node.X, node.Y);
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
        coordsText.text = $"{actualCoords.x},{actualCoords.y}";
        SetNodeColor(ownedByGuid);
        //coordsText.gameObject.SetActive(true); //Helpful for debugging
        GridManager.i.AllNodes.Add(this);
    }
    void OnMouseDown()
    {
        if (!GameManager.i.isEndingTurn && Input.touchCount < 2)
        {
            // Calculate the offset between the mouse position and the GameObject position
            initialMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
            initialMousePosition.z = parentTransform.position.z; // Keep the Z position constant
            canDrag = !GameManager.i.isZooming;
        }
    }

    void OnMouseDrag()
    {
        Vector3 currentMousePosition = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        currentMousePosition.z = parentTransform.position.z; // Keep the Z position constant
        if (canDrag && !isDragging && !GameManager.i.isZooming)
        {
            // Calculate the distance between the initial mouse position and the current mouse position
            float distance = Vector3.Distance(initialMousePosition, currentMousePosition);
            // Start dragging if the distance exceeds the threshold
            if (distance >= dragThreshold)
            {
                GameManager.i.DeselectMovement();
                // Calculate the offset between the mouse position and the GameObject position
                offset = parentTransform.position - currentMousePosition;
                isDragging = true;
            }
        }
        if (isDragging && !GameManager.i.isZooming)
        {
            parentTransform.position = currentMousePosition + offset;
        }
    }

    void OnMouseUp()
    {
        canDrag = false;
        isDragging = false;
    }
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.gameObject.CompareTag("Wall"))
        {
            Vector3 newPostion = new Vector3();
            if (other.gameObject.name == "LeftWall")
            {
                newPostion = new Vector3(transform.position.x + 11.35f, transform.position.y, transform.position.z);
            }
            else if (other.gameObject.name == "RightWall")
            {
                newPostion = new Vector3(transform.position.x - 11.35f, transform.position.y, transform.position.z);
            }
            else if (other.gameObject.name == "TopWall")
            {
                newPostion = new Vector3(transform.position.x, transform.position.y - 9.69f, transform.position.z);
            }
            else if (other.gameObject.name == "BottomWall")
            {
                newPostion = new Vector3(transform.position.x, transform.position.y + 9.69f, transform.position.z);
            }
            transform.position = newPostion;
            if (unitOnPath != null)
                unitOnPath.transform.position = newPostion;
        }
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
            X = actualCoords.x,
            Y = actualCoords.y,
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