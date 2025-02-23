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
    public Transform accountPanel;
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
    public GameObject clientOutOfSyncPanel;
    internal bool goingToNextScene = false;
    // Start is called before the first frame update
    void Start()
    {
        sql = new SqlManager();
        Globals.HasBeenToLobby = true;
        waitingText = waitingPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        playersText = createGamePanel.transform.Find("Value").GetComponent<TextMeshProUGUI>();
        if (Globals.Account == null)
        {
            SceneManager.LoadScene((int)Scene.Login);
        }
        else
        {
            accountPanel.Find("Elo").GetComponent<TextMeshProUGUI>().text = $"2 Player Rating: {(int)Globals.Account.Rating}";
            //accountPanel.Find("Wins").GetComponent<TextMeshProUGUI>().text = $"Wins: {Globals.Account.Wins}";
            accountPanel.Find("Username").GetComponent<TextMeshProUGUI>().text = Globals.Account.Username;
            accountPanel.Find("Email").GetComponent<Toggle>().isOn = (bool)Globals.Account.NotifiyByEmail;
        }
    }
    public void ToggleEmailNotification()
    {
        try
        {
            Globals.Account.NotifiyByEmail = accountPanel.Find("Email").GetComponent<Toggle>().isOn;
            var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(Globals.Account);
            StartCoroutine(sql.PostRoutine<Account>($"Login/UpdateAccount?clientVersion={Constants.ClientVersion}", stringToPost, AccountCreated));
        }
        catch (Exception ex)
        {
            Debug.Log("Unable to create account");
            loadingPanel.SetActive(false);
        }
    }

    private void AccountCreated(Account account, string clientOutOfSync)
    {
        if (!String.IsNullOrEmpty(clientOutOfSync))
        {
            clientOutOfSyncPanel.SetActive(true);
            clientOutOfSyncPanel.transform.Find("ClientVersion").GetComponent<TextMeshProUGUI>().text = clientOutOfSync;
        }
    }

    public void Logout()
    {
        PlayerPrefs.SetString("AccountId", "");
        PlayerPrefs.Save();
        SceneManager.LoadScene((int)Scene.Login);
    }
    public void ExitScene()
    {
        goingToNextScene = false;
        loadingPanel.SetActive(false);
        waitingPanel.SetActive(false);
        createGamePanel.SetActive(false);
        joinGamePanel.SetActive(false);
        activeGamePanel.SetActive(false);
    }
    public void CreateGame(bool cpuGame)
    {
        var gameGuid = Guid.NewGuid();
        var players = new List<GamePlayer>();
        GameSettings.Clear();
        if (teamToggle.isOn && MaxPlayers == 4 && !cpuGame)
            GameSettings.Add(GameSettingType.Teams.ToString());
        if (toggle1.isOn)
            GameSettings.Add(GameSettingType.TakeoverCosts2.ToString());
        if (cpuGame)
        {
            GameSettings.Add(GameSettingType.PracticeGame.ToString());
            MaxPlayers = 4;
            for (int i = 1; i < MaxPlayers; i++)
            {
                players.Add(GetNewPlayer(gameGuid, i, Constants.CPUGuids[i-1]));
            }
        }
        GameSettings.Add(((GameSettingType)(UnityEngine.Random.Range((int)GameSettingType.Map1, (int)GameSettingType.Map3+1))).ToString());
        players.Add(GetNewPlayer(gameGuid,0));
        var gameMatch = new GameMatch()
        {
            GameGuid = gameGuid,
            MaxPlayers = MaxPlayers,
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
        StartCoroutine(sql.PostRoutine<GameMatch>($"Game/CreateGame?clientVersion={Constants.ClientVersion}", stringToPost, SetMatchGuid));
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
        MaxPlayers = 2;
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
        StartCoroutine(sql.GetRoutine<List<GameMatch>>($"Game/FindGames?clientVersion={Constants.ClientVersion}", GetAllMatches));
    }
    public void FindActiveGames()
    {
        StartCoroutine(sql.GetRoutine<List<GameMatch>>($"Game/FindGames?clientVersion={Constants.ClientVersion}&playerGuid={Globals.Account.PlayerGuid}", GetActiveMatches));
    }
    public void JoinActiveGame(GameMatch gameMatch)
    {
        goingToNextScene = true;
        loadingPanel.SetActive(true);
        var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(gameMatch);
        StartCoroutine(sql.PostRoutine<GameMatch>($"Game/JoinGame?clientVersion={Constants.ClientVersion}", stringToPost, SetMatchGuid));
    }
    public void JoinGame(GameMatch gameMatch)
    {
        loadingPanel.SetActive(true);
        gameMatch.GameTurns.FirstOrDefault().Players.Add(GetNewPlayer(gameMatch.GameGuid, gameMatch.GameTurns.FirstOrDefault().Players.Count()));
        var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(gameMatch);
        StartCoroutine(sql.PostRoutine<GameMatch>($"Game/JoinGame?clientVersion={Constants.ClientVersion}", stringToPost, SetMatchGuid));
    }
    private GamePlayer GetNewPlayer(Guid gameGuid, int playerId, Guid? playerGuid = null)
    {
        if(playerGuid == null) 
            playerGuid = Globals.Account.PlayerGuid;
        return new GamePlayer()
        {
            GameGuid = gameGuid,
            TurnNumber = 0,
            PlayerColor = playerId,
            PlayerName = Globals.Account.Username.Split(' ')[0],
            PlayerGuid = (Guid)playerGuid,
            Units = new List<ServerUnit>()
            {
                new ServerUnit()
                {
                    PlayerGuid = Globals.Account.PlayerGuid,
                    UnitGuid = Globals.Account.PlayerGuid,
                    GameGuid = gameGuid,
                    TurnNumber = 0,
                    PlayerColor = playerId,
                    UnitType = (int)UnitType.Station,
                },
                new ServerUnit()
                {
                    PlayerGuid = Globals.Account.PlayerGuid,
                    UnitGuid = Guid.NewGuid(),
                    GameGuid = gameGuid,
                    TurnNumber = 0,
                    PlayerColor = playerId,
                    UnitType = (int)UnitType.Bomber,
                },
                new ServerUnit()
                {
                    PlayerGuid = Globals.Account.PlayerGuid,
                    UnitGuid = Guid.NewGuid(),
                    GameGuid = gameGuid,
                    TurnNumber = 0,
                    PlayerColor = playerId,
                    UnitType = (int)UnitType.Bomb,
                },
            },
            Credits = Constants.StartingCredits,
        };
    }

    private void GetAllMatches(List<GameMatch> _newGames, string clientOutOfSync)
    {
        if (!String.IsNullOrEmpty(clientOutOfSync))
        {
            clientOutOfSyncPanel.SetActive(true);
            clientOutOfSyncPanel.transform.Find("ClientVersion").GetComponent<TextMeshProUGUI>().text = clientOutOfSync;
        }
        else
        {
            ClearOpenGames();
            var newGames = _newGames.Where(x => !x.GameTurns.FirstOrDefault().Players.Any(y => y.PlayerGuid == Globals.Account.PlayerGuid));
            foreach (var newGame in newGames)
            {
                var game = newGame;
                var prefab = Instantiate(openGamePrefab, findContent);
                prefab.GetComponent<Button>().onClick.AddListener(() => JoinGame(game));
                prefab.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = $"Game Id: {game.GameGuid.ToString().Substring(0, 6)}";
                prefab.transform.Find("PlayerText").GetComponent<TextMeshProUGUI>().text = $"Created By: {game.GameTurns.FirstOrDefault().Players.FirstOrDefault(x=>x.PlayerColor == 0)?.PlayerName}";
                prefab.transform.Find("Players").GetComponent<TextMeshProUGUI>().text = $"{game.GameTurns.FirstOrDefault().Players.Count(x => x != null)}/{game.MaxPlayers}";
                openGamesObjects.Add(prefab);
            }
        }
    }
    private void GetActiveMatches(List<GameMatch> newGames, string clientOutOfSync)
    {
        if (!String.IsNullOrEmpty(clientOutOfSync))
        {
            clientOutOfSyncPanel.SetActive(true);
            clientOutOfSyncPanel.transform.Find("ClientVersion").GetComponent<TextMeshProUGUI>().text = clientOutOfSync;
        }
        else
        {
            ClearOpenGames();
            foreach (var newGame in newGames.Where(x => !x.GameSettings.Contains("PracticeGame")))
            {
                var game = newGame;
                var prefab = Instantiate(openGamePrefab, activeContent);
                prefab.GetComponent<Button>().onClick.AddListener(() => JoinActiveGame(game));
                var playersNames = game.GameTurns.FirstOrDefault().Players.Where(x => x.PlayerGuid != Globals.Account.PlayerGuid).OrderBy(x => x.PlayerColor).Select(x => x.PlayerName);
                prefab.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = $"Game Id: {game.GameGuid.ToString().Substring(0, 6)}";
                if (playersNames.Count() == 0)
                {
                    prefab.transform.Find("PlayerText").GetComponent<TextMeshProUGUI>().text = "Waiting for players";
                }
                else
                {
                    prefab.transform.Find("PlayerText").GetComponent<TextMeshProUGUI>().text = $"Vs: {String.Join(", ", playersNames)}";
                }
                prefab.transform.Find("Players").GetComponent<TextMeshProUGUI>().text = $"Turn #{(game.GameTurns.OrderBy(x => x.TurnNumber).LastOrDefault(x => x.Players.Count() == game.MaxPlayers)?.TurnNumber ?? -1) + 1}";
                var players = game.GameTurns.OrderBy(x => x.TurnNumber).LastOrDefault().Players;
                prefab.transform.Find("Ready").gameObject.SetActive(players.Count() == game.MaxPlayers || !players.Any(x => x?.PlayerGuid == Globals.Account.PlayerGuid));
                openGamesObjects.Add(prefab);
            }
        }
    }
    private void ClearOpenGames()
    {
        while (openGamesObjects.Count > 0) { Destroy(openGamesObjects[0].gameObject); openGamesObjects.RemoveAt(0); }
    }

    private void SetMatchGuid(GameMatch game, string clientOutOfSync)
    {
        if (!String.IsNullOrEmpty(clientOutOfSync))
        {
            clientOutOfSyncPanel.SetActive(true);
            clientOutOfSyncPanel.transform.Find("ClientVersion").GetComponent<TextMeshProUGUI>().text = clientOutOfSync;
        }
        else if (game != null)
        {
            Globals.GameMatch = game;
            MaxPlayers = game.MaxPlayers;
            waitingPanel.SetActive(true);
            createGamePanel.SetActive(false);
            joinGamePanel.SetActive(false);
            activeGamePanel.SetActive(false);
            UpdateGameStatus(game.GameTurns.FirstOrDefault(),"");
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
            yield return StartCoroutine(sql.GetRoutine<GameTurn>($"Game/GetTurns?gameGuid={Globals.GameMatch.GameGuid}&turnNumber=0&searchType={(int)SearchType.lobbySearch}&startPlayers={Globals.GameMatch.GameTurns.FirstOrDefault(x => x.TurnNumber == 0)?.Players?.Count() ?? 0}&clientVersion={Constants.ClientVersion}", UpdateGameStatus));
        }
    }

    private int PlayersNeeded()
    {
        return Globals.GameMatch.MaxPlayers - Globals.GameMatch.GameTurns.FirstOrDefault().Players.Count();
    }

    private void UpdateGameStatus(GameTurn newTurn, string clientOutOfSync)
    {
        if (!String.IsNullOrEmpty(clientOutOfSync))
        {
            clientOutOfSyncPanel.SetActive(true);
            clientOutOfSyncPanel.transform.Find("ClientVersion").GetComponent<TextMeshProUGUI>().text = clientOutOfSync;
        }
        else
        {
            Globals.GameMatch.GameTurns[0] = newTurn;
            if (PlayersNeeded() == 0)
            {
                Globals.localStationColor = Globals.GameMatch.GameTurns.FirstOrDefault().Players.FirstOrDefault(x => x.PlayerGuid == Globals.Account.PlayerGuid).PlayerColor;
                Globals.Teams = Globals.GameMatch.GameSettings.Contains(GameSettingType.Teams.ToString()) ? 2 : Globals.GameMatch.MaxPlayers == 1 ? 4 : Globals.GameMatch.MaxPlayers;
                PlayerPrefs.SetString("GameGuid", newTurn.GameGuid.ToString());
                PlayerPrefs.Save();
                SceneManager.LoadScene((int)Scene.Game);
            }
            else
            {
                loadingPanel.SetActive(goingToNextScene);
                UpdateWaitingText();
            }
        }
    }
}
