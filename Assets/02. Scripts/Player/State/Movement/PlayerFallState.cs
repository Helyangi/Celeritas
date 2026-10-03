using UnityEngine;

public class PlayerFallState : PlayerBaseMovementState
{
    public PlayerFallState(PlayerController controller) : base(controller) {}
    
    private float _fallMoveInput;
    
    public override void EnterState()
    {
        
    }

    public override void UpdateState()
    {
        // 상태 변경 조건 체크
        ChangeJumpState();

        if (controller.GroundChecker.IsGround())
        {
            controller.ChangeMovementState(controller.IdleState);
        }

        _fallMoveInput = GetMoveInput();
    }

    public override void FixedUpdateState()
    {
        Move(_fallMoveInput);
    }
    public override void ExitState()
    {
        controller.CanJump = true;
    }
}
