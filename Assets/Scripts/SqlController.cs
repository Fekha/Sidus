using System;
using System.Collections;
using System.Collections.Generic;
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
    public IEnumerator RequestRoutine<T>(string url, Action<T> callback = null)
    {
        using (UnityWebRequest request = UnityWebRequest.Get(apiUrl + url))
        {
            request.SetRequestHeader("Access-Control-Allow-Origin", "*");
            yield return request.SendWebRequest();
            if (callback != null)
                callback(Newtonsoft.Json.JsonConvert.DeserializeObject<T>(request.downloadHandler.text));
        }
    }

    //public IEnumerator PostRoutine(string url, dynamic ToPost, Action<string> callback = null)
    //{
    //    var json = JsonUtility.ToJson(ToPost); //Newtonsoft.Json.JsonConvert.SerializeObject(ToPost);
    //    byte[] postData = System.Text.Encoding.UTF8.GetBytes(json);
    //    using (UnityWebRequest request = UnityWebRequest.Put(apiUrl + url, postData))
    //    {
    //        //request.SetRequestHeader("Access-Control-Allow-Origin", "*");
    //        request.SetRequestHeader("Content-Type", "application/json");
    //        request.method = "POST";
    //        //request.chunkedTransfer = false;
    //        yield return request.SendWebRequest();

    //        var data = request.downloadHandler.text;

    //        if (callback != null)
    //            callback(data);
    //    }
    //}
}