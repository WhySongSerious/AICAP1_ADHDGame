# 잊혀진 별자리 — ML-Agents 난이도 조절 프로토타입

<p align="center"><img src="docs/forgotten-constellation.jpg" width="720" alt="잊혀진 별자리"></p>

ADHD 집중력·반응 훈련을 주제로 한 Unity 팀 프로젝트에서, 플레이 기록에 따라 난이도를 자동으로 조절하는 반응 게임 프로토타입입니다.
아주대학교 AI융합캡스톤디자인1 과제로 진행했습니다.

| 항목 | 내용 |
| --- | --- |
| 기간 | 2025.09 – 2025.12 |
| 엔진 | Unity 6 (6000.0.49f1) · C# |
| 형태 | 2인 팀 프로젝트 |
| 기술 | Unity ML-Agents (PPO), ONNX 추론 |
| 시연 영상 | [잊혀진 별자리 시연 (Google Drive)](https://drive.google.com/file/d/1D_GWcWMXCCZPWjGZEGMwXTTCsHGzVDn1/view) |

## 기획 배경

디자인씽킹 과정(인터뷰·AEIOU 관찰 → 5 Whys → 페르소나)으로 문제를 정리했습니다.

- **문제:** 집중력 저하로 "나 ADHD인가?"를 의심하지만 비용·시간·심리적 부담 때문에 검사를 받지 않고, 기존 집중력 검사는 지루하고 점수만 알려 줍니다.
- **페르소나:** 시험 공부 중 쉽게 산만해지는 고등학생, 초반 집중은 좋지만 오래 못 가는 대학생, 충동적으로 일을 바꾸는 직장인 — 공통 니즈는 "부담 없이, 행동 기반으로, 게임처럼 내 집중 패턴을 알고 싶다".
- **기획한 시스템:** 미니게임에서 반응시간·정확도·실수 패턴을 수집하고, ML-Agents 모델이 난이도를 개인화하며, AI 코치 캐릭터 '루나'가 행동 패턴을 해석해 피드백을 주는 구조입니다.

## 게임 구조

화면에 나타난 타깃을 제한 시간 안에 누르는 반응 게임입니다. 라운드마다 반응시간과 성공·실패를 기록하고, 난이도 Agent가 타깃 유지시간을 조절합니다.

## 내가 맡은 일 — 송재혁

- **난이도 Agent:** 현재 타깃 유지시간, 최근 평균 반응시간, 누적 성공률을 관측해 유지시간을 조절합니다. 한 번에 바뀌는 폭(±0.15초)과 범위(1~3초)를 제한해 난이도가 급격히 변하지 않게 했습니다.
- **보상 설계:** 실패에는 패널티를 주고, 성공률이 목표치 70%에 가까울수록 보상을 높여 지나치게 쉽거나 어렵지 않은 구간을 유지하게 했습니다.
- **자동 플레이 봇:** 사람이 학습 플레이를 반복하기 어려워, 정규분포(평균 0.8초, 표준편차 0.25초)로 반응시간을 뽑고 15% 확률로 놓치는 봇을 만들어 학습에 사용했습니다.
- **적용 확인:** 학습된 정책을 ONNX 모델로 게임에 적용하고, 실제 추론 여부와 관측값을 출력하는 검증 스크립트로 확인했습니다.

## 코드 위치

`ML_AGENT_AICapstone/` 아래

| 내용 | 경로 |
| --- | --- |
| 난이도 Agent | [`Assets/Scripts/DifficultyAgent.cs`](ML_AGENT_AICapstone/Assets/Scripts/DifficultyAgent.cs) |
| 반응 게임 | [`Assets/Scripts/ReactionGameManager.cs`](ML_AGENT_AICapstone/Assets/Scripts/ReactionGameManager.cs) |
| 자동 플레이 봇 | [`Assets/Scripts/AutoClickBot.cs`](ML_AGENT_AICapstone/Assets/Scripts/AutoClickBot.cs) |
| ONNX 적용 확인 | [`Assets/Scripts/OnnxSanityTest.cs`](ML_AGENT_AICapstone/Assets/Scripts/OnnxSanityTest.cs) |
| 학습 설정 | [`MLTraining/difficulty_config.yaml`](ML_AGENT_AICapstone/MLTraining/difficulty_config.yaml) |
| 학습된 모델 | [`Assets/results/DifficultyBrain.onnx`](ML_AGENT_AICapstone/Assets/results/DifficultyBrain.onnx) |

## 실행

1. Unity 6000.0.49f1로 `ML_AGENT_AICapstone/` 폴더를 엽니다.
2. `Assets/Scenes/MainGame.unity`에서 실행합니다.
3. 학습을 다시 하려면 ML-Agents Python 패키지를 설치한 뒤 `mlagents-learn MLTraining/difficulty_config.yaml`을 사용합니다.

## 참고

- 기획 단계의 여러 미니게임·리포트 기능 중 이 저장소에는 반응 게임과 난이도 Agent 프로토타입만 포함되어 있습니다.
- `FeedbackClient.cs`는 플레이 기록을 로컬 피드백 서버로 보내는 실험용 코드이며, 서버는 이 저장소에 포함되어 있지 않습니다.
