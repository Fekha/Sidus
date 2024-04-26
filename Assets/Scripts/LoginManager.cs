using StarTaneousAPI.Models;
using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    private SqlManager sql;
    public GameObject waitingPanel;
    // Start is called before the first frame update
    void Start()
    {
        sql = new SqlManager();
        Globals.localStationId = Guid.NewGuid();
    }

    public void JoinGame()
    {
        StartCoroutine(sql.GetRoutine<Guid>($"Game/Join?ClientId={Globals.localStationId}", SetMatchGuid));
    }

    private void SetMatchGuid(Guid gameId)
    {
        if (gameId != null)
        {
            Globals.GameId = gameId;
            waitingPanel.SetActive(true);
            StartCoroutine(GetStationIndex());
        }
    }

    private IEnumerator GetStationIndex()
    {
        while (Globals.Players == null)
        {
            yield return StartCoroutine(sql.GetRoutine<Player[]?>($"Game/HasGameStarted?GameId={Globals.GameId}&ClientId={Globals.localStationId}", SetStationGuids));
        }
    }

    private void SetStationGuids(Player[]? players)
    {
        if (players != null)
        {
            for (int i = 0; i < players.Length; i++)
            {
                if (players[i].StationId == Globals.localStationId)
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
