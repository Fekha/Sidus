using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SqlManager
{
    string apiUrl;
    public SqlManager()
    {
        apiUrl = "https://game.gravitas-games.com:7002/api/";
//#if UNITY_EDITOR
//        apiUrl = "https://localhost:7002/api/";
//#endif
    }
    public IEnumerator GetRoutine<T>(string url, Action<T,string> callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl + url))
        {
            yield return request.SendWebRequest();
            DoCallback<T>(request, callback, $"{apiUrl + url}");
        }
    }
    public IEnumerator PostRoutine<T>(string url, string ToPost, Action<T,string> callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Post(apiUrl + url, ToPost, "application/json"))
        {
            yield return request.SendWebRequest();
            DoCallback<T>(request, callback, $"{apiUrl + url}\n{ToPost}");
        }
    }
    private void DoCallback<T>(UnityWebRequest request, Action<T, string> callback, string requestString)
    {
        var clientOutOfSync = request.result != UnityWebRequest.Result.Success && request.error.Contains("400 Bad Request");
        if (request.result == UnityWebRequest.Result.Success || clientOutOfSync)
        {
            Debug.Log($"{requestString}\n{request.downloadHandler.text}");
            if (callback != null)
            {
                if (clientOutOfSync)
                    callback(default, $"Please refresh broswer.\n{request.downloadHandler.text}.");
                else
                    callback(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(request.downloadHandler.text), "");
            }
        }
        else
        {
            Debug.Log($"{requestString}\n{request.error}");
        }
    }
}