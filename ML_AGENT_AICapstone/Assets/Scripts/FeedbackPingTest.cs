using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FeedbackPingTest : MonoBehaviour
{
    [System.Serializable]
    public class FocusSnapshot
    {
        public int roundIndex;
        public float successRate;
        public float avgReactionTime;
        public float targetLifeTime;
        public int streakSuccess;
        public int streakFail;
        public bool lastSuccess;
    }

    [System.Serializable]
    private class FeedbackWrapper
    {
        public string message;
    }

    public string serverUrl = "http://localhost:5000/feedback";

    void Start()
    {
        // 유니티 실행하면 바로 한 번 서버로 쏴보기
        FocusSnapshot snap = new FocusSnapshot
        {
            roundIndex = 3,
            successRate = 0.7f,
            avgReactionTime = 1.1f,
            targetLifeTime = 2.3f,
            streakSuccess = 2,
            streakFail = 1,
            lastSuccess = true
        };

        StartCoroutine(SendSnapshotAndLogResponse(snap));
    }

    IEnumerator SendSnapshotAndLogResponse(FocusSnapshot snapshot)
    {
        string json = JsonUtility.ToJson(snapshot);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            Debug.Log("?? 테스트 요청 전송 중...");
            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Feedback error: " + www.error);
            }
            else
            {
                Debug.Log("? 서버 응답(raw): " + www.downloadHandler.text);
                FeedbackWrapper wrapper = JsonUtility.FromJson<FeedbackWrapper>(www.downloadHandler.text);
                Debug.Log("? 파싱된 message: " + wrapper.message);
            }
        }
    }
}
