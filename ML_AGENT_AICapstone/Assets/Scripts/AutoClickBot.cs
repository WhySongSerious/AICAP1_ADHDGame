using UnityEngine;
using System.Collections;

public class AutoClickBot : MonoBehaviour
{
    [Header("Ref")]
    public ReactionGameManager gm; // 인스펙터에서 연결

    [Header("Bot Reaction Time (sec)")]
    public float meanReact = 0.8f;   // 평균 반응시간
    public float stdReact = 0.25f;  // 반응시간 변동
    public float missProb = 0.15f;  // 일부러 놓칠 확률(ADHD-like)

    private Coroutine loop;

    void OnEnable()
    {
        if (gm == null)
        {
            Debug.LogError("AutoClickBot: gm is not assigned!");
            return;
        }
        loop = StartCoroutine(PlayLoop());
    }

    void OnDisable()
    {
        if (loop != null) StopCoroutine(loop);
    }

    IEnumerator PlayLoop()
    {
        while (true)
        {
            // 타겟이 켜질 때까지 대기
            yield return new WaitUntil(() => gm.IsRoundFinished() == false);

            // 일부러 실패시키는 케이스
            if (Random.value < missProb)
            {
                yield return new WaitForSeconds(gm.targetLifeTime + 0.05f);
                continue;
            }

            // 사람 같은 반응시간 샘플링
            float rt = Mathf.Max(0.05f, RandomGaussian(meanReact, stdReact));
            yield return new WaitForSeconds(rt);

            // 아직 라운드 끝나기 전이면 강제 클릭으로 성공 처리
            if (!gm.IsRoundFinished())
            {
                gm.ForceClickByBot();
            }
        }
    }

    float RandomGaussian(float mean, float std)
    {
        // Box?Muller transform
        float u1 = 1f - Random.value;
        float u2 = 1f - Random.value;
        float randStdNormal =
            Mathf.Sqrt(-2f * Mathf.Log(u1)) * Mathf.Sin(2f * Mathf.PI * u2);
        return mean + std * randStdNormal;
    }
}
