using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoginManager : MonoBehaviour
{
    private SqlManager sql;
    public GameObject waitingPanel;
    private bool gameReady;
    // Start is called before the first frame update
    void Start()
    {
        sql = new SqlManager();
        Globals.ClientId = Guid.NewGuid();
    }

    public void JoinGame()
    {
        StartCoroutine(sql.GetRoutine<Guid>($"Game/Join?ClientId={Globals.ClientId}", SetMatchGuid));
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
        while (Globals.myStationIndex == null)
        {
            yield return StartCoroutine(sql.GetRoutine<int?>($"Game/HasGameStarted?GameId={Globals.GameId}&ClientId={Globals.ClientId}", SetStationIndex));
        }
    }

    private void SetStationIndex(int? index)
    {
        if (index != null)
        {
            Globals.myStationIndex = index;
            SceneManager.LoadScene(1);
        }
    }
}
