using UnityEngine;

public class PlayerJumpState : PlayerBaseMovementState
{
    public PlayerJumpState(PlayerController controller) : base(controller) {}
    private float _jumpMoveInput;
    
    public override void EnterState()
    {
        controller.CanJump = false;
        Jump(Vector2.up);
    }

    public override void UpdateState()
    {
        // 상태 변경 조건 체크
        ChangeFallState();
        ChangeWallGrabState();
        
        _jumpMoveInput = GetMoveInput();
    }

    public override void FixedUpdateState()
    {
        Move(_jumpMoveInput);
    }
}