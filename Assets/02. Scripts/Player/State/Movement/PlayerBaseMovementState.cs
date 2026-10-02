using UnityEditor.Callbacks;
using UnityEngine;

public abstract class PlayerBaseMovementState : PlayerBaseState
{
    public PlayerBaseMovementState(PlayerController controller) : base(controller) {}

    protected void ChangeJumpState()
    {
        if (GetJumpInput())
        {
            controller.ChangeMovementState(controller.JumpState);
        }
    }
    protected void ChangeFallState()
    {
        if (controller.RB.linearVelocityY < 0 && !controller.GroundChecker.IsGround())
        {
            controller.ChangeMovementState(controller.FallState);
        }
    }

    protected void Move(float inputMove)
    {
        controller.RB.linearVelocityX = inputMove * controller.Stats.MoveSpeed;
    }
    protected void Jump(Vector2 jumpDirection)
    {
        controller.RB.linearVelocityY = 0;
        controller.RB.AddForce(jumpDirection * controller.Stats.JumpForce, ForceMode2D.Impulse);
    }
}
