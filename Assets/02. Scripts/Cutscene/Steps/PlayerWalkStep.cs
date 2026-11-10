using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어가 스스로 목표 위치까지 좌우로 걸어간다.
/// 평소 이동 코드를 그대로 쓰기 때문에 달리기 애니메이션, 방향 전환, 중력, 발소리가 게임 중과 똑같이 나온다.
/// (컷씬 동안 실제 키보드 입력은 막혀 있다)
/// </summary>
[Serializable, CutsceneStepMenu("플레이어 걷기")]
public class PlayerWalkStep : CutsceneStep
{
    [Tooltip("걸어갈 위치의 이름표 Id (마커, NPC 등). 좌우 위치만 맞추고 높이는 신경 쓰지 않는다")]
    [SerializeField] private string _targetId;
    [Tooltip("평소 이동 속도에 곱할 배율 (1 = 평소 속도, 0.5 = 절반)")]
    [SerializeField, Range(0.1f, 1f)] private float _speedScale = 1f;
    [Tooltip("목표와 이 거리 안이면 도착한 것으로 본다")]
    [SerializeField, Min(0.01f)] private float _arriveDistance = 0.05f;
    [Tooltip("끄면 걸어가는 동안 다음 단계가 동시에 진행된다")]
    [SerializeField] private bool _waitUntilArrive = true;

    private static int _latestWalk; // 걷기가 겹치면 나중에 시작한 것만 움직인다

    public override string Summary => string.IsNullOrEmpty(_targetId) ? "" : $"→ {_targetId}";

    public override IEnumerator Execute(CutsceneManager manager)
    {
        if (_waitUntilArrive) yield return Walk();
        else manager.RunInBackground(this, Walk());
    }

    private IEnumerator Walk()
    {
        PlayerController player = CutsceneTargets.FindPlayer();
        Transform target = CutsceneTargets.FindTransform(_targetId);
        InputManager input = InputManager.Instance;
        if (player == null || target == null || input == null) yield break;

        int walkId = ++_latestWalk;

        // 공중이거나 등장 연출 중이면 착지할 때까지 기다렸다가 출발한다
        yield return CutsceneTargets.WaitUntilPlayerReady(player);
        if (player == null || target == null) yield break;

        float startDistance = target.position.x - player.transform.position.x;
        if (Mathf.Abs(startDistance) <= _arriveDistance) yield break;

        float direction = Mathf.Sign(startDistance);
        float speed = Mathf.Max(0.01f, (player.Stats != null ? player.Stats.MoveSpeed : 5f) * _speedScale);

        // 입력을 멈춘 뒤에도 물리 프레임에서 한 걸음 더 가므로, 그만큼 일찍 멈춘다
        float stopDistance = _arriveDistance + speed * Time.fixedDeltaTime;

        // 벽 등에 막혀 못 가는 경우를 대비한 제한 시간 (예상 시간의 2배 + 1초)
        float timeout = Mathf.Abs(startDistance) / speed * 2f + 1f;
        float time = 0f;

        while (true)
        {
            if (walkId != _latestWalk) yield break; // 다른 걷기 단계가 이어받았다
            if (player == null || target == null) break;

            float distance = target.position.x - player.transform.position.x;
            if (distance * direction <= stopDistance) break; // 도착했거나 살짝 지나쳤다

            time += Time.deltaTime;
            if (time > timeout)
            {
                Debug.LogWarning($"[PlayerWalkStep] '{_targetId}'까지 가지 못했습니다. 벽이나 낭떠러지에 막혔는지 확인하세요.");
                break;
            }

            input.SetScriptedMove(direction * _speedScale);
            yield return null;
        }

        if (walkId != _latestWalk || input == null) yield break;

        input.ClearScriptedInput();
        yield return CutsceneTargets.WaitUntilPlayerStops(player);
    }
}
