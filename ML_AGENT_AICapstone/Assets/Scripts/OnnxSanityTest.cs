using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Policies;

/// <summary>
/// DifficultyAgent가 inference-only에서
/// 진짜 ONNX 추론을 하고 있는지 확인하는 테스트 스크립트.
/// </summary>
[RequireComponent(typeof(DifficultyAgent))]
public class OnnxSanityTest : MonoBehaviour
{
    private DifficultyAgent agent;
    private ReactionGameManager gm;
    private BehaviorParameters bp;

    public float printInterval = 0.5f;
    private float t = 0f;

    void Awake()
    {
        agent = GetComponent<DifficultyAgent>();
        gm = agent.gameManager;
        bp = GetComponent<BehaviorParameters>();
    }

    void Update()
    {
        if (gm == null || bp == null) return;
        t += Time.deltaTime;

        if (t >= printInterval)
        {
            t = 0f;

            float lifeNorm =
                (gm.targetLifeTime - agent.minLifeTime) / (agent.maxLifeTime - agent.minLifeTime);

            float avgReact = gm.GetAverageReactionTime();
            float avgNorm =
                Mathf.Clamp01((avgReact - agent.minLifeTime) / (agent.maxLifeTime - agent.minLifeTime));

            float successRate = gm.GetSuccessRate();

            Debug.Log(
                $"[SANITY] BehaviorType={bp.BehaviorType}, Model={(bp.Model != null ? bp.Model.name : "NULL")}\n" +
                $"   obs: lifeNorm={lifeNorm:F3}, avgNorm={avgNorm:F3}, successRate={successRate:F3}\n" +
                $"   targetLifeTime={gm.targetLifeTime:F3}"
            );
        }
    }
}
