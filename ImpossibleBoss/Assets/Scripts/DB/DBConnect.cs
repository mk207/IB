using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;


[System.Serializable]
public class SelectSoloRankRequest
{
    public int bossType;
    public int bossDiff;
    public int offset;
    public int maxRows;
}

[System.Serializable]
public class SelectSoloRankResultDetail
{
    public string userName;
    public float dealPerSec;
    public float clearTime;
    public string createdAt;
}

[System.Serializable]
public class SelectSoloRankResponse
{
    public List<SelectSoloRankResultDetail> Body;
    public string ErrorReport;
    public int ResponseResults;
}

[System.Serializable]
public class InsertSoloRankRequest
{
    public string userName;
    public int bossType;
    public int bossDiff;
    public float dealPerSec;
    public float clearTime;
}

[System.Serializable]
public class InsertSoloRankResponse
{
    public string ErrorReport;
    public int ResponseResults;
}

public class DBConnect : MonoBehaviour
{
    public SelectSoloRankResponse SelectSoloRank(SelectSoloRankRequest body)
    {
        string bodyJson = JsonUtility.ToJson(body);
        var bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJson);

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://13.209.226.5:8090/s_solorank");
        request.Method = "POST";
        request.ContentType = "application/json";
        request.ContentLength = bodyRaw.Length;

        using (var stream = request.GetRequestStream())
        {
            stream.Write(bodyRaw, 0, bodyRaw.Length);
            stream.Flush();
            stream.Close();
        }
        try
        {
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();
            StreamReader reader = new StreamReader(response.GetResponseStream());
            string json = reader.ReadToEnd();

            SelectSoloRankResponse result = JsonUtility.FromJson<SelectSoloRankResponse>(json);

            return result;
        }
        catch
        {
            SelectSoloRankResponse result = null;
            return result;
        }
    }

    public static InsertSoloRankResponse InsertSoloRank(InsertSoloRankRequest body)
    {
        string bodyJson = JsonUtility.ToJson(body);
        var bodyRaw = System.Text.Encoding.UTF8.GetBytes(bodyJson);

        HttpWebRequest request = (HttpWebRequest)WebRequest.Create("http://13.209.226.5:8090/i_solorank");
        request.Method = "POST";
        request.ContentType = "application/json";
        request.ContentLength = bodyRaw.Length;

        using (var stream = request.GetRequestStream())
        {
            stream.Write(bodyRaw, 0, bodyRaw.Length);
            stream.Flush();
            stream.Close();
        }

        HttpWebResponse response = (HttpWebResponse)request.GetResponse();
        StreamReader reader = new StreamReader(response.GetResponseStream());
        string json = reader.ReadToEnd();

        InsertSoloRankResponse result = JsonUtility.FromJson<InsertSoloRankResponse>(json);

        return result;
    }
}
