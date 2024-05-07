using StartaneousAPI.Models;
using StarTaneousAPI.Models;
using System;
using System.Collections;
using System.Collections.Generic;
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
    public Toggle mineAfterMove;
    private TextMeshProUGUI waitingText;
    private TextMeshProUGUI playersText;
    private List<GameObject> openGamesObjects = new List<GameObject>();
    internal int MaxPlayers = 2;
    // Start is called before the first frame update
    void Start()
    {
        sql = new SqlManager();
        Globals.localStationGuid = Guid.NewGuid();
        waitingText = waitingPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>();
        playersText = createGamePanel.transform.Find("Value").GetComponent<TextMeshProUGUI>();
    }
    public void CreateGame()
    {
        if (mineAfterMove.isOn)
            Globals.GameSettings.Add(GameSettingType.MineAfterMove.ToString());
        var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(new NewGame(Globals.localStationGuid, MaxPlayers, Globals.GameSettings));
        StartCoroutine(sql.PostRoutine<NewGame>($"Game/Create?", stringToPost, SetMatchGuid));
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
        StartCoroutine(sql.GetRoutine<List<NewGame>>($"Game/Find", GetAllMatches));
    }
    public void JoinGame(Guid guid)
    {
        StartCoroutine(sql.GetRoutine<NewGame>($"Game/Join?ClientId={Globals.localStationGuid}&GameId={guid}", SetMatchGuid));
    }
    private void GetAllMatches(List<NewGame> newGames)
    {
        ClearOpenGames();
        foreach (var game in newGames)
        {
            var prefab = Instantiate(openGamePrefab, findContent);
            prefab.GetComponent<Button>().onClick.AddListener(() => JoinGame(game.GameId));
            prefab.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = game.GameId.ToString().Substring(0, 6);
            prefab.transform.Find("Players").GetComponent<TextMeshProUGUI>().text = $"{game.PlayerCount}/{game.MaxPlayers}";
            openGamesObjects.Add(prefab);
        }
    }
    private void ClearOpenGames()
    {
        while (openGamesObjects.Count > 0) { Destroy(openGamesObjects[0].gameObject); openGamesObjects.RemoveAt(0); }
    }

    private void SetMatchGuid(NewGame game)
    {
        if (game != null)
        {
            Globals.GameId = game.GameId;
            Globals.GameSettings = game.GameSettings;
            MaxPlayers = game.MaxPlayers;
            waitingPanel.SetActive(true);
            UpdateWaitingText(game.MaxPlayers - game.PlayerCount);
            StartCoroutine(GetStationIndex());
        }
    }
    public void UpdateWaitingText(int players)
    {
        var plural = players == 1 ? "" : "s";
        waitingText.text = $"Waiting for {players} more player{plural} to join game {Globals.GameId.ToString().Substring(0, 6)}....";
    }
    private IEnumerator GetStationIndex()
    {
        while (Globals.Players == null)
        {
            yield return StartCoroutine(sql.GetRoutine<Player[]>($"Game/HasGameStarted?GameId={Globals.GameId}&ClientId={Globals.localStationGuid}", SetStationGuids));
        }
    }

    private void SetStationGuids(Player[] players)
    {
        if (players != null)
        {
            if (players.Length == MaxPlayers) {
                for (int i = 0; i < players.Length; i++)
                {
                    if (players[i].StationGuid == Globals.localStationGuid)
                    {
                        Globals.localStationIndex = i;
                    }
                }
                Globals.Players = players;
                SceneManager.LoadScene(1);
            } else {
                UpdateWaitingText(MaxPlayers - players.Length);
            }
        }
    }
}
