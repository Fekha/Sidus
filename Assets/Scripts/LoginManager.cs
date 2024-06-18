using Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoginManager : MonoBehaviour
{
    private SqlManager sql;
    public GameObject waitingPanel;
    public GameObject joinGamePanel;
    public GameObject activeGamePanel;
    public GameObject createGamePanel;
    public GameObject loadingPanel;
    public Transform findContent;
    public Transform activeContent;
    public GameObject openGamePrefab;
    public Toggle toggle1;
    public Toggle teamToggle;
    private TextMeshProUGUI waitingText;
    private TextMeshProUGUI playersText;
    private List<GameObject> openGamesObjects = new List<GameObject>();
    private int MaxPlayers = 2;
    private List<string> GameSettings = new List<string>();
    // Start is called before the first frame update
    void Start()
    {
        sql = new SqlManager();
        Globals.HasBeenToLobby = true;
        waitingText = waitingPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        playersText = createGamePanel.transform.Find("Value").GetComponent<TextMeshProUGUI>();
        GetPlayerGuid();
    }

    private void GetPlayerGuid()
    {
        bool isUnityEditor = false;
#if UNITY_EDITOR
        isUnityEditor = true;
#endif
        try
        {
            Guid.TryParse(PlayerPrefs.GetString("ClientGuid"), out Globals.clientGuid);
            if (Globals.clientGuid == Guid.Empty)
            {
                Globals.clientGuid = Guid.NewGuid();
                PlayerPrefs.SetString("ClientGuid", Globals.clientGuid.ToString());
                PlayerPrefs.Save();
            }
        }
        catch (Exception ex)
        {
            Debug.Log("Unable to connect to cache");
        }
    }

    public void CreateGame(bool cpuGame)
    {
        loadingPanel.SetActive(true);
        Globals.IsCPUGame = cpuGame;
        if (teamToggle.isOn && MaxPlayers == 4 && !Globals.IsCPUGame)
            GameSettings.Add(GameSettingType.Teams.ToString());
        if (toggle1.isOn)
            GameSettings.Add(GameSettingType.TakeoverCosts2.ToString());
        if (Globals.IsCPUGame)
            MaxPlayers = 1;
        var gameGuid = Guid.NewGuid();
        var players = new List<GamePlayer>();
        players.Add(GetNewPlayer(gameGuid,0));
        var gameMatch = new GameMatch()
        {
            GameGuid = gameGuid,
            MaxPlayers = MaxPlayers,
            NumberOfModules = 51,
            GameSettings = String.Join(",", GameSettings),
            GameTurns = new List<GameTurn>()
            {
                new GameTurn() {
                    GameGuid = gameGuid,
                    ModulesForMarket = "",
                    MarketModuleGuids = "",
                    TurnNumber = 0,
                    AllModules = new List<ServerModule>(),
                    Players = players,
                    TurnIsOver = true
                }
            },
            HealthCheck = DateTime.Now 
        };
        var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(gameMatch);
        StartCoroutine(sql.PostRoutine<GameMatch>($"Game/CreateGame", stringToPost, SetMatchGuid));
    }
    public void ViewOpenGames(bool active)
    {
        if (active)
        {
            FindGames();
        }
        joinGamePanel.SetActive(active);
    }
    public void ViewActiveGames(bool active)
    {
        if (active)
        {
            FindActiveGames();
        }
        activeGamePanel.SetActive(active);
    }
    public void ViewGameCreation(bool active)
    {
        createGamePanel.SetActive(active);
    }
    public void EditMaxPlayers(bool plus)
    {
        if (plus && MaxPlayers < 4)
        {
            MaxPlayers++;
        }
        else if (!plus && MaxPlayers > 2)
        {
            MaxPlayers--;
        }
        if (MaxPlayers == 4)
        {
            teamToggle.interactable = true;
            teamToggle.gameObject.SetActive(true);
        }
        else
        {
            teamToggle.gameObject.SetActive(false);
            teamToggle.interactable = false;
            teamToggle.isOn = false;
        }
        playersText.text = $"{MaxPlayers}";
    }
    public void FindGames()
    {
        StartCoroutine(sql.GetRoutine<List<GameMatch>>($"Game/FindGames", GetAllMatches));
    }
    public void FindActiveGames()
    {
        StartCoroutine(sql.GetRoutine<List<GameMatch>>($"Game/FindGames?playerGuid={Globals.clientGuid}", GetActiveMatches));
    }
    public void JoinActiveGame(GameMatch gameMatch)
    {
        var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(gameMatch);
        StartCoroutine(sql.PostRoutine<GameMatch>($"Game/JoinGame", stringToPost, SetMatchGuid));
    } 
    public void JoinGame(GameMatch gameMatch)
    {
        gameMatch.GameTurns[0].Players.Add(GetNewPlayer(gameMatch.GameGuid,1));
        var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(gameMatch);
        StartCoroutine(sql.PostRoutine<GameMatch>($"Game/JoinGame", stringToPost, SetMatchGuid));
    }
    private GamePlayer GetNewPlayer(Guid gameGuid, int playerId)
    {
        return new GamePlayer()
        {
            GameGuid = gameGuid,
            TurnNumber = 0,
            PlayerId = playerId,
            PlayerGuid = Globals.clientGuid,
            Units = new List<ServerUnit>()
            {
                new ServerUnit()
                {
                    UnitGuid = Globals.clientGuid,
                    GameGuid = gameGuid,
                    TurnNumber = 0,
                    PlayerId = playerId,
                    IsStation = true,
                },
                new ServerUnit()
                {
                    UnitGuid = Guid.NewGuid(),
                    GameGuid = gameGuid,
                    TurnNumber = 0,
                    PlayerId = playerId,
                    IsStation = false,
                },
            },
            Credits = 3,
        };
    }

    private void GetAllMatches(List<GameMatch> _newGames)
    {
        ClearOpenGames();
        var newGames = _newGames.Where(x => !x.GameTurns[0].Players.Any(y => y.PlayerGuid == Globals.clientGuid));
        foreach (var newGame in newGames)
        {
            var game = newGame;
            var prefab = Instantiate(openGamePrefab, findContent);
            prefab.GetComponent<Button>().onClick.AddListener(() => JoinGame(game));
            prefab.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = game.GameGuid.ToString().Substring(0, 6);
            prefab.transform.Find("Players").GetComponent<TextMeshProUGUI>().text = $"{game.GameTurns[0].Players.Count(x=>x!=null)}/{game.MaxPlayers}";
            openGamesObjects.Add(prefab);
        }
    }
    private void GetActiveMatches(List<GameMatch> newGames)
    {
        ClearOpenGames();
        foreach (var game in newGames)
        {
            var prefab = Instantiate(openGamePrefab, activeContent);
            prefab.GetComponent<Button>().onClick.AddListener(() => JoinActiveGame(game));
            prefab.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = game.GameGuid.ToString().Substring(0, 6);
            prefab.transform.Find("Players").GetComponent<TextMeshProUGUI>().text = $"Turn #{game.GameTurns.Count-1}";
            var players = game.GameTurns.Last().Players;
            prefab.transform.Find("Ready").gameObject.SetActive(players.Count() == game.MaxPlayers || !players.Any(x => x?.PlayerGuid == Globals.clientGuid));
            openGamesObjects.Add(prefab);
        }
    }
    private void ClearOpenGames()
    {
        while (openGamesObjects.Count > 0) { Destroy(openGamesObjects[0].gameObject); openGamesObjects.RemoveAt(0); }
    }

    private void SetMatchGuid(GameMatch game)
    {
        if (game != null)
        {
            Globals.GameMatch = game;
            MaxPlayers = game.MaxPlayers;
            waitingPanel.SetActive(true);
            loadingPanel.SetActive(false);
            createGamePanel.SetActive(false);
            joinGamePanel.SetActive(false);
            activeGamePanel.SetActive(false);
            UpdateGameStatus(game.GameTurns[0]);
            StartCoroutine(CheckIfGameHasStarted());
        }
    }
    public void UpdateWaitingText()
    {
        var playerNeeded = PlayersNeeded();
        var plural = playerNeeded == 1 ? "" : "s";
        waitingText.text = $"Waiting for {playerNeeded} more player{plural} to join game {Globals.GameMatch.GameGuid.ToString().Substring(0, 6)}....";
    }
    private IEnumerator CheckIfGameHasStarted()
    {
        while (PlayersNeeded() != 0)
        {
            yield return StartCoroutine(sql.GetRoutine<GameTurn>($"Game/HasTakenTurn?gameGuid={Globals.GameMatch.GameGuid}&turnNumber=0", UpdateGameStatus));
        }
    }

    private int PlayersNeeded()
    {
        return Globals.GameMatch.MaxPlayers - Globals.GameMatch.GameTurns[0].Players.Count();
    }

    private void UpdateGameStatus(GameTurn gameTurn)
    {
        Globals.GameMatch.GameTurns[0] = gameTurn;
        if (PlayersNeeded() == 0) {
            Globals.localStationIndex = Globals.GameMatch.GameTurns[0].Players.FirstOrDefault(x => x.PlayerGuid == Globals.clientGuid).PlayerId;
            Globals.Teams = Globals.GameMatch.GameSettings.Contains(GameSettingType.Teams.ToString()) ? 2 : Globals.IsCPUGame ? 4 : Globals.GameMatch.MaxPlayers;
            SceneManager.LoadScene((int)Scene.Game);
        } else {
            UpdateWaitingText();
        }
    }
}
