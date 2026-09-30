using UnityEngine;

// 이동 입력이 없을 때의 상태 (정지 상태)
public class PlayerIdleState : PlayerBaseMovementState
{
    public PlayerIdleState(PlayerController controller) : base(controller) { }

    public override void EnterState()
    {
        // Idle 상태 진입 시 처리할 내용 (필요하면 정지 애니메이션 재생 등 추가)
    }

    public override void UpdateState()
    {
        // 이 State가 직접 입력을 받아서 전환 여부를 판단한다
        float moveInput = GetMoveInput();
        ChangeJumpState();

        // 이동 입력이 감지되면 Move 상태로 전환
        if (moveInput != 0)
        {
            controller.ChangeMovementState(controller.MoveState);
        }
    }

    public override void ExitState()
    {
        // Idle 상태에서 빠져나갈 때 처리할 내용
    }
}
