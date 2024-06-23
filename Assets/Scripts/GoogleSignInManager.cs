using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System;
using System.Linq;
using UnityEngine;
using UnityEngine.SceneManagement;
using Models;

public class GoogleSignInManager : MonoBehaviour
{
    private SqlManager sql;
    public GameObject loadingPanel;
    public void Start()
    {
#if UNITY_EDITOR
        Globals.DebugMode = true;
        //Uncomment to run as exo
        //PlayerPrefs.SetString("AccountId", "108002382745613062728"); 
        //PlayerPrefs.Save();

        //Uncomment to run as feca
        PlayerPrefs.SetString("AccountId", "112550575566380218480");
        PlayerPrefs.Save();
#endif
        sql = new SqlManager();
        loadingPanel.SetActive(true);
        GetPlayerGuid();
    }
    // This function will be called when the Unity button is pressed
    public void OnSignInButtonPressed()
    {
        loadingPanel.SetActive(true);
        if (Globals.DebugMode)
        {
            OnGoogleSignIn("eyJhbGciOiJSUzI1NiIsImtpZCI6IjNkNTgwZjBhZjdhY2U2OThhMGNlZTdmMjMwYmNhNTk0ZGM2ZGJiNTUiLCJ0eXAiOiJKV1QifQ.eyJpc3MiOiJodHRwczovL2FjY291bnRzLmdvb2dsZS5jb20iLCJhenAiOiI0OTU3Mjc4ODk5NTEtc3RtNzczdGFhYzRiNW1ocDN1OGo2dWsxMnVhaGdnMXEuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJhdWQiOiI0OTU3Mjc4ODk5NTEtc3RtNzczdGFhYzRiNW1ocDN1OGo2dWsxMnVhaGdnMXEuYXBwcy5nb29nbGV1c2VyY29udGVudC5jb20iLCJzdWIiOiIxMDEwNzM3NDUxMjE3NzMzMzc2MDkiLCJlbWFpbCI6ImphbW1vcmFuanJAZ21haWwuY29tIiwiZW1haWxfdmVyaWZpZWQiOnRydWUsIm5iZiI6MTcxODc5NTY3MiwibmFtZSI6IkZlY2EiLCJwaWN0dXJlIjoiaHR0cHM6Ly9saDMuZ29vZ2xldXNlcmNvbnRlbnQuY29tL2EvQUNnOG9jSU9BeU90eEt5NzZlbHlzaHBKUXNJUXFfQVRPa2l3bVdoekVWVjNNaEVhY2t2bWpvNVk9czk2LWMiLCJnaXZlbl9uYW1lIjoiRmVjYSIsImlhdCI6MTcxODc5NTk3MiwiZXhwIjoxNzE4Nzk5NTcyLCJqdGkiOiJmMjE0YjkxMjE5YzllNTQ2OTUzMzRlNWI5ODE2YzEzNThmOGY0NTNmIn0.Weu3h-HnlX3wXUBeTXyUPndcmPnkZDV8W-LY69DAHMBkFQ8vr8GNsVgXAr2Qso1Bgl769Vh5Gz0g2XUYyhppnosq6Md_RQ11O7dXI8eTRocLFChdDp17g8mCNH3Ks09wHlahPdxUW1H5-kT6dn1fG4xctwHmXe7vZejpgG3yB5Hn8flffVjBGZLjtb64WgNLRqSBUunQckHDVI69jn7ohFI4BP03MnzOieyau3i0CSna8rIusyDK8FYHINgkb26EoFuuTq-RHNH6FrVDrDZrrpGVt5qOIpUoQrBQpO50-GAqVz2Mo-QjNOqNorEA78XOW6G0U5wX9vMMTQw28QhJIw");
        }
        else
        {
            Application.ExternalEval("initiateGoogleSignIn();");
        }
    }

    // This function will be called from JavaScript with the ID token
    public void OnGoogleSignIn(string idToken)
    {
        Debug.Log("Google ID Token: " + idToken);
        var user = DecodeIdToken(idToken);
        Debug.Log(user["name"]);
        Debug.Log(user["email"]);
        Debug.Log(user["sub"]);
        if (user.ContainsKey("sub") && user.ContainsKey("name")&& user.ContainsKey("email"))
        {
            Globals.Account = new Account()
            {
                AccountId = user["sub"],
                Username = user["name"],
                Email = user["email"],
            };
            try
            {
                var stringToPost = Newtonsoft.Json.JsonConvert.SerializeObject(Globals.Account);
                StartCoroutine(sql.PostRoutine<Guid>($"Login/CreateAccount", stringToPost, AccountCreated));
            }
            catch (Exception ex)
            {
                Debug.Log("Unable to create account");
                loadingPanel.SetActive(false);
            }
        }
        else
        {
            loadingPanel.SetActive(false);
        }
    }

    private void AccountCreated(Guid playerGuid)
    {
        if(playerGuid != Guid.Empty)
        {
            Globals.Account.PlayerGuid = playerGuid;
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

    private void GetPlayerGuid()
    {
        var accountId = PlayerPrefs.GetString("AccountId");
        if (!String.IsNullOrEmpty(accountId))
        {
            StartCoroutine(sql.GetRoutine<Account>($"Login/GetAccount?accountId={accountId}", SetAccount));
        }
        else
        {
            loadingPanel.SetActive(false);
        }
    }

    private void SetAccount(Account account)
    {
        if(account != null)
        {
            Globals.Account = account;
            SceneManager.LoadScene((int)Scene.Lobby);
        }
        else
        {
            Debug.Log("Account retrieve failed");
            loadingPanel.SetActive(false);
        }
    }

    private Dictionary<string, string> DecodeIdToken(string idToken)
    {
        string[] parts = idToken.Split('.');
        if (parts.Length != 3)
        {
            Debug.LogError("Invalid JWT token");
            return null;
        }

        // JWT consists of three parts: Header, Payload, Signature
        // We need to decode the Payload (the second part)
        string payload = parts[1];
        // Fix Base64 padding
        payload = payload.Replace('-', '+').Replace('_', '/');
        switch (payload.Length % 4)
        {
            case 2: payload += "=="; break;
            case 3: payload += "="; break;
        }

        string jsonPayload = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(payload));
        Debug.Log("Decoded Payload: " + jsonPayload);

        var payloadData = Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, string>>(jsonPayload);
        if (payloadData != null)
        {
            return payloadData;
        }

        Debug.LogError("No 'sub' claim found in the token");
        return null;
    }
}