using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Models;
using TMPro;
using UnityEngine.UI;

public class GoogleSignInManager : MonoBehaviour
{
    public static GoogleSignInManager i;
    private SqlManager sql;
    public GameObject loadingPanel;
    public TMP_InputField username;
    public GameObject clientOutOfSyncPanel;
    public TMP_Text customAlertText;
    public GameObject customAlertPanel;

    public void Start()
    {
        i = this;
#if UNITY_EDITOR
        Globals.DebugMode = true;
        PlayerPrefs.SetString("AccountId", "101073745121773337609");
        PlayerPrefs.Save();
#endif
        sql = new SqlManager();
        loadingPanel.SetActive(true);
        GetPlayerGuid();
    }
    // This function will be called when the Unity button is pressed
    public void OnSignInButtonPressed()
    {
        if (username.text.Length > 2)
        {
            loadingPanel.SetActive(true);
            StartCoroutine(sql.GetRoutine<Account?>($"Login/CheckAccountExists?username={username.text.ToUpper()}&clientVersion={Constants.ClientVersion}", SetAccount));
        }
        else
        {
            ShowCustomAlertPanel("Username must be at least 3 characters");
        }
    }

    private void AccountCreated(Account player, string clientOutOfSync)
    {
        if (!String.IsNullOrEmpty(clientOutOfSync))
        {
            clientOutOfSyncPanel.SetActive(true);
            clientOutOfSyncPanel.transform.Find("ClientVersion").GetComponent<TextMeshProUGUI>().text = clientOutOfSync;
        }
        else
        {
            if (player != null)
            {
                Globals.Account = player;
                PlayerPrefs.SetString("AccountId", Globals.Account.AccountId);
                PlayerPrefs.Save();
                SceneManager.LoadScene((int)Scene.Lobby);
            }
            else
            {
                Debug.Log("Account creation failed");
                loadingPanel.SetActive(false);
            }
        }
    }

    private void GetPlayerGuid()
    {
        var accountId = PlayerPrefs.GetString("AccountId");
        if (!String.IsNullOrEmpty(accountId))
        {
            StartCoroutine(sql.GetRoutine<Account>($"Login/GetAccount?accountId={accountId}&clientVersion={Constants.ClientVersion}", SetAccount));
        }
        else
        {
            loadingPanel.SetActive(false);
        }
    }

    private void SetAccount(Account account, string clientOutOfSync)
    {
        if (!String.IsNullOrEmpty(clientOutOfSync))
        {
            clientOutOfSyncPanel.SetActive(true);
            clientOutOfSyncPanel.transform.Find("ClientVersion").GetComponent<TextMeshProUGUI>().text = clientOutOfSync;
        }
        else
        {
            if (account != null)
            {
                Globals.Account = account;
                account.Username = account.Username.Split(' ')[0];
                SceneManager.LoadScene((int)Scene.Lobby);
            }
            else
            {
                Globals.Account = new Account()
                {
                    AccountId = Guid.NewGuid().ToString(),
                    Username = username.text,
                    Email = "test@yahoo.com",
                    Rating = 1500,
                    Wins = 0,
                    NotifiyByEmail = false
                };
                var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(Globals.Account);
                StartCoroutine(sql.PostRoutine<Account>($"Login/CreateAccount?clientVersion={Constants.ClientVersion}", stringToPost, AccountCreated));
                loadingPanel.SetActive(false);
            }
        }
    }

    internal void Type(string letter)
    {
        if (letter == "backspace")
        {
            if (username.text.Length > 0)
            {
                username.text = username.text.Substring(0, username.text.Length - 1);
            }
        }
        else
        {
            username.text += letter.ToUpper();
        }
    }

    public void ShowCustomAlertPanel(string message)
    {
        customAlertText.text = message;
        customAlertPanel.SetActive(true);
        Debug.Log(message);
    }
}