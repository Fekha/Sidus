using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;

public class SqlManager
{
    string apiUrl;
    public SqlManager()
    {
//        apiUrl = "https://game.gravitas-games.com:7001/api/";
//#if UNITY_EDITOR
        apiUrl = "https://localhost:7001/api/";
//#endif
    }
    public IEnumerator GetRoutine<T>(string url, Action<T> callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl + url))
        {
            yield return request.SendWebRequest();
            if (request.result != UnityWebRequest.Result.Success)
            {
                Debug.Log(request.error);
            }
            else
            {
                Debug.Log(request.downloadHandler.text);
                if (callback != null)
                    callback(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(request.downloadHandler.text));
            } 
        }
    }
    public IEnumerator PostRoutine<T>(string url, string ToPost, Action<T> callback = null)
    {
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
                if (callback != null)
                    callback(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(request.downloadHandler.text));
            }
        }
    }
}