using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class ReactionGameManager : MonoBehaviour
{
    [Header("UI Refs")]
    public Button targetButton;
    public RectTransform targetRectTransform;

    [Header("Agent Ref")]
    public DifficultyAgent agent;

    [Header("Feedback / Luna UI")]
    public FeedbackClient feedbackClient;       // 피드백 서버
    public Text lunaText;                       // 루나 대사 출력용

    // === 난이도 범위 및 초기값 ===
    [Header("Difficulty Range")]
    public float minLifeTime = 1.0f;
    public float maxLifeTime = 3.0f;
    public float startLifeTime = 2.5f;

    // ML-Agent가 조절하는 유지 시간
    [HideInInspector] public float targetLifeTime = 1.0f;

    [Header("Spawn Area")]
    public Vector2 spawnAreaMin = new Vector2(-300f, -150f);
    public Vector2 spawnAreaMax = new Vector2(300f, 150f);

    private float appearTime;
    private bool isTargetActive = false;

    // 라운드 정보
    public bool roundJustFinished = false;
    public bool lastRoundSuccess = false;

    // 기록용
    public int maxHistory = 10;
    private List<float> reactionTimes = new List<float>();
    private int successCount = 0;
    private int failCount = 0;

    // === 피드백 관련 ===
    private float lastFeedbackTime = 0f;       // 마지막 피드백 시각
    public float feedbackInterval = 20f;       // 🔥 20초마다 피드백

    // 누적 정보
    private int totalRoundCount = 0;
    private int streakSuccess = 0;
    private int streakFail = 0;

    private float gameStartTime;

    void Start()
    {
        targetButton.onClick.AddListener(OnTargetClicked);
        targetButton.gameObject.SetActive(false);

        gameStartTime = Time.time;
        lastFeedbackTime = Time.time;  // 시작 후 20초 뒤 첫 피드백
    }

    void Update()
    {
        // 타겟 유지시간 초과 → 실패
        if (isTargetActive)
        {
            if (Time.time - appearTime > targetLifeTime)
            {
                FailRound();
            }
        }

        // 🔥 20초마다 한 번씩 피드백 요청
        if (Time.time - lastFeedbackTime >= feedbackInterval)
        {
            lastFeedbackTime = Time.time;
            RequestFeedbackNow();
        }
    }

    // === 라운드 시작 ===
    public void StartNewRound()
    {
        roundJustFinished = false;
        lastRoundSuccess = false;

        targetLifeTime = Mathf.Clamp(targetLifeTime, minLifeTime, maxLifeTime);

        ShowTarget();
        appearTime = Time.time;
        isTargetActive = true;
    }

    void ShowTarget()
    {
        float x = Random.Range(spawnAreaMin.x, spawnAreaMax.x);
        float y = Random.Range(spawnAreaMin.y, spawnAreaMax.y);
        targetRectTransform.anchoredPosition = new Vector2(x, y);

        targetButton.gameObject.SetActive(true);
    }

    // === 성공 처리 ===
    void OnTargetClicked()
    {
        if (!isTargetActive) return;

        isTargetActive = false;
        targetButton.gameObject.SetActive(false);

        float reactionTime = Time.time - appearTime;
        Debug.Log($"성공! 반응시간: {reactionTime * 1000f:F0} ms");

        successCount++;
        streakSuccess++;
        streakFail = 0;
        lastRoundSuccess = true;

        reactionTimes.Add(reactionTime);
        if (reactionTimes.Count > maxHistory)
            reactionTimes.RemoveAt(0);

        roundJustFinished = true;
        totalRoundCount++;

        if (agent != null)
            agent.OnRoundFinished(true);

        Debug.Log($"[ROUND END] success={lastRoundSuccess}, successRate={GetSuccessRate():F2}, avgReact={GetAverageReactionTime():F3}s, lifeTime={targetLifeTime:F3}");
    }

    // === 실패 처리 ===
    void FailRound()
    {
        if (!isTargetActive) return;

        isTargetActive = false;
        targetButton.gameObject.SetActive(false);

        Debug.Log("실패: 시간 초과");

        failCount++;
        streakFail++;
        streakSuccess = 0;
        lastRoundSuccess = false;

        roundJustFinished = true;
        totalRoundCount++;

        if (agent != null)
            agent.OnRoundFinished(false);

        Debug.Log($"[ROUND END] success={lastRoundSuccess}, successRate={GetSuccessRate():F2}, avgReact={GetAverageReactionTime():F3}s, lifeTime={targetLifeTime:F3}");
    }

    // 새로운 라운드 딜레이로 시작
    public IEnumerator StartNewRoundDelayed(float delay)
    {
        yield return new WaitForSeconds(delay);
        StartNewRound();
    }

    // === 통계 ===
    public float GetAverageReactionTime()
    {
        if (reactionTimes.Count == 0)
            return targetLifeTime;

        float sum = 0f;
        foreach (var t in reactionTimes) sum += t;
        return sum / reactionTimes.Count;
    }

    public float GetSuccessRate()
    {
        int total = successCount + failCount;
        if (total == 0) return 0f;
        return (float)successCount / total;
    }

    public void ResetStats()
    {
        reactionTimes.Clear();
        successCount = 0;
        failCount = 0;
        roundJustFinished = false;
        lastRoundSuccess = false;
        isTargetActive = false;
        targetButton.gameObject.SetActive(false);

        totalRoundCount = 0;
        streakSuccess = 0;
        streakFail = 0;

        gameStartTime = Time.time;
        lastFeedbackTime = Time.time;
    }

    public bool IsRoundFinished()
    {
        return !isTargetActive;
    }

    // === ML bot이 강제로 클릭할 때 ===
    public void ForceClickByBot()
    {
        OnTargetClicked();
    }

    // === 실제 LLM 피드백 요청 ===
    void RequestFeedbackNow()
    {
        Debug.Log("루나의 피드백 : ");

        if (feedbackClient == null)
        {
            Debug.LogWarning("FeedbackClient가 연결되어 있지 않아요.");
            return;
        }

        FocusSnapshot snap = new FocusSnapshot
        {
            roundIndex = totalRoundCount,
            successRate = GetSuccessRate(),
            avgReactionTime = GetAverageReactionTime(),
            targetLifeTime = targetLifeTime,
            streakSuccess = streakSuccess,
            streakFail = streakFail,
            lastSuccess = lastRoundSuccess
        };

        StartCoroutine(
            feedbackClient.SendSnapshotAndGetFeedback(snap, (msg) =>
            {
                if (!string.IsNullOrEmpty(msg))
                {
                    Debug.Log("루나 피드백: " + msg);

                    if (lunaText != null)
                        lunaText.text = msg;
                }
            })
        );
    }
}
