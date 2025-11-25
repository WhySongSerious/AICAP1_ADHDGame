using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Actuators;

/// <summary>
/// DifficultyAgent에 붙여서
/// ONNX 정책 출력(actionValue)과 난이도 변화 로그를 찍는다.
/// </summary>
[RequireComponent(typeof(DifficultyAgent))]
public class DifficultyAgentLogger : MonoBehaviour
{
    private DifficultyAgent agent;
    private ReactionGameManager gm;

    [Header("Logging")]
    public bool logEveryStep = false;   // true면 매 스텝 로그(많이 찍힘)
    public bool logOnRoundEnd = true;   // true면 라운드 끝날 때 로그

    private float lastLife;
    private int stepCounter = 0;

    void Awake()
    {
        agent = GetComponent<DifficultyAgent>();
        gm = agent.gameManager;
        lastLife = gm != null ? gm.targetLifeTime : -1f;
    }

    void Update()
    {
        if (gm == null) return;

        stepCounter++;

        // 난이도 변화 감지
        if (Mathf.Abs(gm.targetLifeTime - lastLife) > 0.0001f)
        {
            if (logEveryStep)
            {
                Debug.Log(
                    $"[STEP {stepCounter}] targetLifeTime changed: {lastLife:F3} -> {gm.targetLifeTime:F3}"
                );
            }
            lastLife = gm.targetLifeTime;
        }
    }

    /// <summary>
    /// DifficultyAgent의 OnActionReceived에서
    /// 이 함수를 직접 호출하게 만들면 액션 로깅 가능
    /// </summary>
    public void LogAction(float actionValue, float delta)
    {
        if (logEveryStep)
        {
            Debug.Log(
                $"[ACTION] a={actionValue:F4}, delta={delta:F4}, newLife={gm.targetLifeTime:F3}"
            );
        }
    }

    /// <summary>
    /// 라운드 종료 시점 로그
    /// (ReactionGameManager.OnTargetClicked/FailRound에서 부를 것)
    /// </summary>
    public void LogRoundEnd(bool success)
    {
        if (!logOnRoundEnd) return;

        float sr = gm.GetSuccessRate();
        float avg = gm.GetAverageReactionTime();

        Debug.Log(
            $"[ROUND END] success={success}, " +
            $"successRate={sr:F2}, avgReact={avg:F3}s, " +
            $"targetLifeTime={gm.targetLifeTime:F3}"
        );
    }
}
