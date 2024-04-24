using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;

public class SqlController
{
    string apiUrl;
    public SqlController()
    {
        apiUrl = "https://game.gravitas-games.com:7001/api/";
#if UNITY_EDITOR
        apiUrl = "https://localhost:7220/api/";
#endif
    }
    public IEnumerator GetRoutine<T>(string url, Action<T> callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl + url))
        {
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
            yield return request.SendWebRequest();
            if (callback != null)
                callback(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(request.downloadHandler.text));
        }
    }
    public IEnumerator PostRoutine<T>(string url, string ToPost, Action<T> callback = null)
    {
        WWWForm form = new WWWForm();
        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl + url, ToPost, "application/json"))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
            }
            if (callback != null)
                callback(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(request.downloadHandler.text));

        }
    }
}