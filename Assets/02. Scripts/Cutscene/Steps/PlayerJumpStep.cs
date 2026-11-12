using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// 플레이어가 스스로 한 번 점프한다. 평소 점프 코드를 그대로 쓴다.
/// </summary>
[Serializable, CutsceneStepMenu("플레이어 점프")]
public class PlayerJumpStep : CutsceneStep
{
    public enum JumpDirection
    {
        [InspectorName("제자리")] InPlace,
        [InspectorName("왼쪽")] Left,
        [InspectorName("오른쪽")] Right
    }

    [SerializeField] private JumpDirection _direction = JumpDirection.InPlace;
    [Tooltip("옆으로 뛸 때 평소 이동 속도에 곱할 배율")]
    [SerializeField, Range(0.1f, 1f)] private float _speedScale = 1f;
    [Tooltip("끄면 점프하는 동안 다음 단계가 동시에 진행된다")]
    [SerializeField] private bool _waitUntilLand = true;

    public override string Summary => _direction == JumpDirection.InPlace ? "제자리" : _direction == JumpDirection.Left ? "왼쪽" : "오른쪽";

    public override IEnumerator Execute(CutsceneManager manager)
    {
        if (_waitUntilLand) yield return Jump();
        else manager.RunInBackground(this, Jump());
    }

    private IEnumerator Jump()
    {
        PlayerController player = CutsceneTargets.FindPlayer();
        InputManager input = InputManager.Instance;
        if (player == null || input == null) yield break;

        // 공중이거나 등장 연출 중이면 착지할 때까지 기다렸다가 뛴다
        yield return CutsceneTargets.WaitUntilPlayerReady(player);
        if (player == null) yield break;

        input.ScriptedJump();

        // 발이 땅에서 떨어질 때까지 (최대 0.5초)
        float time = 0f;
        while (player != null && player.GroundChecker.IsGround() && time < 0.5f)
        {
            time += Time.deltaTime;
            yield return null;
        }

        // 공중에 뜬 다음부터 옆으로 움직인다 (점프 애니메이션이 달리기로 덮이지 않도록)
        float move = _direction == JumpDirection.Left ? -_speedScale : _direction == JumpDirection.Right ? _speedScale : 0f;
        if (move != 0f) input.SetScriptedMove(move);

        // 다시 착지할 때까지 (최대 5초)
        time = 0f;
        while (player != null && !player.GroundChecker.IsGround() && time < 5f)
        {
            time += Time.deltaTime;
            yield return null;
        }

        if (input != null && move != 0f) input.ClearScriptedInput();
        yield return CutsceneTargets.WaitUntilPlayerStops(player);
    }
}
