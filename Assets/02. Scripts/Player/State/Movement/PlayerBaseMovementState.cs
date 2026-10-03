using UnityEditor.Callbacks;
using UnityEngine;

public abstract class PlayerBaseMovementState : PlayerBaseState
{
    public PlayerBaseMovementState(PlayerController controller) : base(controller) {}

    protected void ChangeJumpState() // 점프 상태 전환
    {
        if (GetJumpInput() && controller.CanJump)
        {
            controller.ChangeMovementState(controller.JumpState);
        }
    }

    protected void ChangeFallState()  // 낙하 상태 전환
    {
        if (controller.RB.linearVelocityY < 0 && !controller.GroundChecker.IsGround())
        {
            controller.ChangeMovementState(controller.FallState);
        }
    }

    protected void Move(float inputMove) // 이동 함수
    {
        controller.RB.linearVelocityX = inputMove * controller.Stats.MoveSpeed;
    }
    protected void Jump(Vector2 jumpDirection) // 점프 함수
    {
        controller.RB.linearVelocityY = 0;
        controller.RB.AddForce(jumpDirection * controller.Stats.JumpForce, ForceMode2D.Impulse);
    }
}
