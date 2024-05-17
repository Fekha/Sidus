using StartaneousAPI.ServerModels;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public GameObject createGamePanel;
    public Transform findContent;
    public GameObject openGamePrefab;
    public Toggle toggle1;
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
        Globals.localStationGuid = Guid.NewGuid();
        waitingText = waitingPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        playersText = createGamePanel.transform.Find("Value").GetComponent<TextMeshProUGUI>();
    }
    public void CreateGame(bool cpuGame)
    {
        Globals.IsCPUGame = cpuGame;
        if (toggle1.isOn)
            GameSettings.Add(GameSettingType.TakeoverCosts2.ToString());
        if (Globals.IsCPUGame)
            MaxPlayers = 1;
        var gameGuid = Guid.NewGuid();
        var players = new Player[MaxPlayers];
        players[0] = GetNewPlayer();
        var gameMatch = new GameMatch()
        {
            GameGuid = gameGuid,
            MaxPlayers = MaxPlayers,
            NumberOfModules = 26,
            GameSettings = GameSettings,
            GameTurns = new List<GameTurn>()
            {
                new GameTurn() {
                    GameGuid = gameGuid,
                    MarketModules = new List<ServerModule>(),
                    TurnNumber = 0,
                    Players = players
                }
            }
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
        playersText.text = $"{MaxPlayers}";
    }
    public void FindGames()
    {
        StartCoroutine(sql.GetRoutine<List<GameMatch>>($"Game/FindGames", GetAllMatches));
    }
    public void JoinGame(GameMatch gameMatch)
    {
        gameMatch.GameTurns[0].Players[1] = GetNewPlayer();
        var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(gameMatch);
        StartCoroutine(sql.PostRoutine<GameMatch>($"Game/JoinGame", stringToPost, SetMatchGuid));
    }

    private Player GetNewPlayer()
    {
        return new Player()
        {
            Station = new ServerUnit()
            {
                UnitGuid = Globals.localStationGuid,
            },
            Fleets = new List<ServerUnit>()
            {
                new ServerUnit()
                {
                    UnitGuid = Guid.NewGuid(),
                },
            },
            Credits = 5,
        };
    }

    private void GetAllMatches(List<GameMatch> newGames)
    {
        ClearOpenGames();
        foreach (var game in newGames)
        {
            var prefab = Instantiate(openGamePrefab, findContent);
            prefab.GetComponent<Button>().onClick.AddListener(() => JoinGame(game));
            prefab.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = game.GameGuid.ToString().Substring(0, 6);
            prefab.transform.Find("Players").GetComponent<TextMeshProUGUI>().text = $"{game.GameTurns[0].Players.Count(x=>x!=null)}/{game.MaxPlayers}";
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
            UpdateGameStatus(game);
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
            yield return StartCoroutine(sql.GetRoutine<GameMatch>($"Game/HasGameStarted?GameGuid={Globals.GameMatch.GameGuid}", UpdateGameStatus));
        }
    }

    private int PlayersNeeded()
    {
        return Globals.GameMatch.MaxPlayers - (Globals.GameMatch?.GameTurns?[0]?.Players?.Count(x => x != null) ?? 0);
    }

    private void UpdateGameStatus(GameMatch gameMatch)
    {
        Globals.GameMatch = gameMatch;
        if (PlayersNeeded() == 0) {
            for (int i = 0; i < Globals.GameMatch.GameTurns[0].Players.Length; i++)
            {
                if (Globals.GameMatch.GameTurns[0].Players[i].Station.UnitGuid == Globals.localStationGuid)
                {
                    Globals.localStationIndex = i;
                }
            }  
            SceneManager.LoadScene((int)Scene.Game);
        } else {
            UpdateWaitingText();
        }
    }
}
