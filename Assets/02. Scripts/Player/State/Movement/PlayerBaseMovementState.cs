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

    protected void Move(float inputMove)
    {
        controller.RB.linearVelocityX = inputMove * controller.Stats.MoveSpeed;
    }
}
