using UnityEngine;

public class PlayerWallGrabState : PlayerBaseMovementState
{
    public PlayerWallGrabState (PlayerController controller) : base(controller) { }
    
    // 벽에 붙는 키를 따로 만든다.
    // 기본은 미끄러짐..????
    
    private float _originalGravityScale;

    public override void EnterState()
    {
        _originalGravityScale = controller.RB.gravityScale;
        controller.RB.gravityScale = 0;
        controller.RB.linearVelocity = Vector2.zero;
    }

    public override void UpdateState()
    {
        ChangeWallJumpState();
        
        if (controller.WallChecker.GetWallDirection() * GetMoveInput() <= 0)
        {
            controller.ChangeMovementState(controller.FallState);
        }
    }

    public override void ExitState()
    {
        controller.RB.gravityScale = _originalGravityScale;
    }
}
