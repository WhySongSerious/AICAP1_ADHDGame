using UnityEngine;
using Unity.MLAgents;
using Unity.MLAgents.Sensors;
using Unity.MLAgents.Actuators;
using System.Collections;

public class DifficultyAgent : Agent
{
    [Header("Refs")]
    public ReactionGameManager gameManager; // 인스펙터에서 연결

    [Header("Episode Settings")]
    public int roundsPerEpisode = 20;
    private int roundCount = 0;

    [Header("Difficulty Range (must match GameManager)")]
    public float minLifeTime = 1.0f;
    public float maxLifeTime = 3.0f;

    public override void OnEpisodeBegin()
    {
        roundCount = 0;
        gameManager.ResetStats();

        // UI 준비/초기화 한 프레임 기다린 후 시작
        StartCoroutine(DelayedRoundStart());
    }

    private IEnumerator DelayedRoundStart()
    {
        yield return null;
        gameManager.targetLifeTime = gameManager.startLifeTime;
        gameManager.StartNewRound();
    }

    public override void CollectObservations(VectorSensor sensor)
    {
        // 현재 난이도(타겟 생존시간) 정규화
        float lifeNorm =
            (gameManager.targetLifeTime - minLifeTime) / (maxLifeTime - minLifeTime);

        // 최근 평균 반응시간 정규화 (없으면 lifeTime으로 대체)
        float avgReact = gameManager.GetAverageReactionTime();
        float avgNorm =
            Mathf.Clamp01((avgReact - minLifeTime) / (maxLifeTime - minLifeTime));

        // 누적 성공률
        float successRate = gameManager.GetSuccessRate();

        sensor.AddObservation(lifeNorm);
        sensor.AddObservation(avgNorm);
        sensor.AddObservation(successRate);
    }

    public override void OnActionReceived(ActionBuffers actions)
    {
        // 난이도 조절 값 (continuous 1개)
        float actionValue = actions.ContinuousActions[0];

        // 변화폭 제한(폭주 방지)
        float delta = actionValue * 0.15f;

        gameManager.targetLifeTime =
            Mathf.Clamp(gameManager.targetLifeTime + delta, minLifeTime, maxLifeTime);

        // ✅ 라운드 종료 처리는 GameManager가 OnRoundFinished로 직접 호출
        // 여기서는 난이도만 업데이트하고 끝
    }

    /// <summary>
    /// ✅ GameManager가 라운드가 끝나는 순간 직접 불러주는 콜백
    /// </summary>
    public void OnRoundFinished(bool success)
    {
        roundCount++;

        // 실패 패널티
        if (!success)
            AddReward(-1.0f);

        // 목표 성공률(0.7)에 가까울수록 보상
        float sr = gameManager.GetSuccessRate();
        float r = 1.0f - Mathf.Abs(sr - 0.7f); // 0~1 범위
        AddReward(r * 0.1f); // 스케일 다운

        // 에피소드 종료/다음 라운드 진행
        if (roundCount >= roundsPerEpisode)
        {
            EndEpisode();
        }
        else
        {
            StartCoroutine(gameManager.StartNewRoundDelayed(0.3f));
        }
    }

    public override void Heuristic(in ActionBuffers actionsOut)
    {
        // 사람이 난이도 조절 안 하므로 0 고정
        var continuousActions = actionsOut.ContinuousActions;
        continuousActions[0] = 0f;
    }
}
