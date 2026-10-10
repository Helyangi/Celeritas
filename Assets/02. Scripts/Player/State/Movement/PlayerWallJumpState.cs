using UnityEngine;

public class PlayerWallJumpState : PlayerBaseMovementState
{
    public PlayerWallJumpState(PlayerController controller) : base(controller) {}

    public override void EnterState()
    {
        Jump(new Vector2(controller.WallChecker.GetWallDirection() * -1, 1));
    }

    public override void UpdateState()
    {
        ChangeFallState();
    }

    public override void ExitState()
    {
        
    }
}
