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
    public Transform findContent;
    public GameObject openGamePrefab;
    private TextMeshProUGUI waitingText;
    private List<GameObject> openGamesObjects = new List<GameObject>();
    // Start is called before the first frame update
    void Start()
    {
        sql = new SqlManager();
        Globals.localStationGuid = Guid.NewGuid();
        waitingText = waitingPanel.transform.Find("Text").GetComponent<TextMeshProUGUI>();
    }
    public void CreateGame()
    {
        StartCoroutine(sql.GetRoutine<Guid?>($"Game/Create?ClientId={Globals.localStationGuid}", SetMatchGuid));
    }
    public void ViewOpenGames(bool active)
    {
        if (active)
        {
            FindGames();
        }
        joinGamePanel.SetActive(active);
    }
    public void FindGames()
    {
        StartCoroutine(sql.GetRoutine<List<Guid>>($"Game/Find", GetAllMatches));
    }
    public void JoinGame(Guid guid)
    {
        StartCoroutine(sql.GetRoutine<Guid?>($"Game/Join?ClientId={Globals.localStationGuid}&GameId={guid}", SetMatchGuid));
    }
    private void GetAllMatches(List<Guid> list)
    {
        ClearOpenGames();
        foreach (var guid in list)
        {
            var prefab = Instantiate(openGamePrefab, findContent);
            prefab.GetComponent<Button>().onClick.AddListener(() => JoinGame(guid));
            prefab.transform.Find("Text").GetComponent<TextMeshProUGUI>().text = guid.ToString().Substring(0, 6);
            openGamesObjects.Add(prefab);
        }
    }
    private void ClearOpenGames()
    {
        while (openGamesObjects.Count > 0) { Destroy(openGamesObjects[0].gameObject); openGamesObjects.RemoveAt(0); }
    }

    
    private void SetMatchGuid(Guid? gameId)
    {
        if (gameId != null)
        {
            Globals.GameId = (Guid)gameId;
            waitingPanel.SetActive(true);
            waitingText.text = $"Waiting for another player to join game {Globals.GameId.ToString().Substring(0, 6)}....";
            StartCoroutine(GetStationIndex());
        }
    }

    private IEnumerator GetStationIndex()
    {
        while (Globals.Players == null)
        {
            yield return StartCoroutine(sql.GetRoutine<Player[]?>($"Game/HasGameStarted?GameId={Globals.GameId}&ClientId={Globals.localStationGuid}", SetStationGuids));
        }
    }

    private void SetStationGuids(Player[]? players)
    {
        if (players != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].StationId == Globals.localStationGuid)
                {
                    Globals.myStationIndex = i;
                    Globals.localClient = players[i];
                }
                else
                {
                    Globals.enemyClient = players[i];
                }
            }
            Globals.Players = players;
            SceneManager.LoadScene(1);
        }
    }
}
