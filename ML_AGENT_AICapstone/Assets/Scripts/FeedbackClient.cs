using UnityEngine;
using UnityEngine.Networking;
using System.Collections;

public class FeedbackClient : MonoBehaviour
{
    [Header("서버 주소 (지금은 로컬)")]
    public string serverUrl = "http://localhost:5000/feedback";

    [System.Serializable]
    private class FeedbackWrapper
    {
        public string message;
    }

    public IEnumerator SendSnapshotAndGetFeedback(FocusSnapshot snapshot, System.Action<string> onResult)
    {
        // Snapshot -> JSON
        string json = JsonUtility.ToJson(snapshot);
        byte[] body = System.Text.Encoding.UTF8.GetBytes(json);

        using (UnityWebRequest www = new UnityWebRequest(serverUrl, "POST"))
        {
            www.uploadHandler = new UploadHandlerRaw(body);
            www.downloadHandler = new DownloadHandlerBuffer();
            www.SetRequestHeader("Content-Type", "application/json");

            yield return www.SendWebRequest();

            if (www.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Feedback error: " + www.error);
                onResult?.Invoke(null);
            }
            else
            {
                Debug.Log("서버 응답: " + www.downloadHandler.text);
                FeedbackWrapper wrapper = JsonUtility.FromJson<FeedbackWrapper>(www.downloadHandler.text);
                onResult?.Invoke(wrapper.message);
            }
        }
    }
}
